using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MailCraft.Tests;

/// <summary>Records the request and replies with a canned response.</summary>
internal sealed class FakeHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public FakeHandler(int status, string body)
    {
        _status = (HttpStatusCode)status;
        _body = body;
    }

    public HttpRequestMessage? Request { get; private set; }

    public JsonElement Body { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Request = request;
        if (request.Content is not null)
        {
            Body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
        }

        return new HttpResponseMessage(_status) { Content = new StringContent(_body, Encoding.UTF8, "application/json") };
    }
}

public class MailCraftClientTests
{
    private static (MailCraftClient Client, FakeHandler Handler) ClientWith(int status, string body)
    {
        var handler = new FakeHandler(status, body);
        var client = new MailCraftClient("mc_test_key", "https://api.test/v1/", new HttpClient(handler));

        return (client, handler);
    }

    [Fact]
    public async Task SendsAnEmail()
    {
        var (client, handler) = ClientWith(202, """{"data":{"id":"em_1","status":"queued"}}""");

        var result = await client.Emails.SendAsync(new SendEmailRequest
        {
            From = "hello@yourdomain.com",
            To = new[] { "person@example.com" },
            Subject = "Welcome!",
            Html = "<p>Hi</p>",
        });

        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://api.test/v1/emails", handler.Request.RequestUri!.ToString());
        Assert.Equal("Bearer mc_test_key", handler.Request.Headers.Authorization!.ToString());
        Assert.Equal($"mailcraft-dotnet/{MailCraftClient.Version}", handler.Request.Headers.UserAgent.ToString());
        Assert.Equal("person@example.com", handler.Body.GetProperty("to")[0].GetString());
        Assert.False(handler.Body.TryGetProperty("text", out _), "null properties should be omitted");
        Assert.Equal("em_1", result.GetProperty("data").GetProperty("id").GetString());
    }

    [Fact]
    public async Task ListSendsLimit()
    {
        var (client, handler) = ClientWith(200, """{"data":[]}""");

        await client.Contacts.ListAsync(limit: 5);

        Assert.Equal("/v1/contacts?limit=5", handler.Request!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task ValidateEscapesTheEmail()
    {
        var (client, handler) = ClientWith(200, """{"valid":true}""");

        await client.Emails.ValidateAsync("a+b@example.com");

        Assert.Equal("/v1/emails/validate?email=a%2Bb%40example.com", handler.Request!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task TemplateUpdateUsesPatchWithOnlyGivenFields()
    {
        var (client, handler) = ClientWith(200, """{"data":{"id":3}}""");

        await client.Templates.UpdateAsync(3, new TemplateRequest { Subject = "New" });

        Assert.Equal("PATCH", handler.Request!.Method.Method);
        Assert.Equal("/v1/templates/3", handler.Request.RequestUri!.AbsolutePath);
        Assert.Equal("""{"subject":"New"}""", handler.Body.GetRawText());
    }

    [Fact]
    public async Task ManagesContactListMembership()
    {
        var (client, handler) = ClientWith(204, "");

        await client.Contacts.AddToListsAsync("c_1", new long[] { 1, 2 });
        Assert.Equal("/v1/contacts/c_1/lists", handler.Request!.RequestUri!.AbsolutePath);
        Assert.Equal(2, handler.Body.GetProperty("list_ids").GetArrayLength());

        await client.Contacts.RemoveFromListAsync("c_1", 2);
        Assert.Equal(HttpMethod.Delete, handler.Request!.Method);
        Assert.Equal("/v1/contacts/c_1/lists/2", handler.Request.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task MetricsSkipMissingDates()
    {
        var (client, handler) = ClientWith(200, """{"data":[]}""");

        await client.Metrics.GetAsync(startDate: "2026-01-01");
        Assert.Equal("/v1/metrics?start_date=2026-01-01", handler.Request!.RequestUri!.PathAndQuery);

        await client.Metrics.ReputationAsync();
        Assert.Equal("/v1/reputation", handler.Request!.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task TemplateFoldersUseTheHyphenatedPath()
    {
        var (client, handler) = ClientWith(201, """{"data":{"id":1}}""");

        await client.TemplateFolders.CreateAsync("Onboarding");

        Assert.Equal("/v1/template-folders", handler.Request!.RequestUri!.AbsolutePath);
        Assert.Equal("Onboarding", handler.Body.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ThrowsBusinessRuleErrors()
    {
        var (client, _) = ClientWith(402, """{"error":{"type":"plan_limit_reached","message":"Monthly limit reached."}}""");

        var error = await Assert.ThrowsAsync<MailCraftApiException>(() => client.Campaigns.SendAsync(4));

        Assert.Equal(402, error.Status);
        Assert.Equal("plan_limit_reached", error.Type);
        Assert.Equal("Monthly limit reached.", error.Message);
    }

    [Fact]
    public async Task ThrowsValidationErrors()
    {
        var (client, _) = ClientWith(422, """{"message":"The email field is required.","errors":{"email":["The email field is required."]}}""");

        var error = await Assert.ThrowsAsync<MailCraftApiException>(() => client.Contacts.UpsertAsync(new UpsertContactRequest()));

        Assert.Equal(422, error.Status);
        Assert.Null(error.Type);
        Assert.Equal(new[] { "The email field is required." }, error.Errors["email"]);
    }

    [Fact]
    public async Task FallsBackToTheStatusForUnparseableErrors()
    {
        var (client, _) = ClientWith(502, "<html>Bad gateway</html>");

        var error = await Assert.ThrowsAsync<MailCraftApiException>(() => client.Lists.ListAsync());

        Assert.Equal(502, error.Status);
        Assert.Equal("Request failed with status 502", error.Message);
    }

    [Fact]
    public void RequiresAnApiKey()
    {
        Assert.Throws<ArgumentException>(() => new MailCraftClient(""));
    }
}
