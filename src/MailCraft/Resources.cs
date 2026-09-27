using System.Text.Json;

// One resource per API section. Every method returns the API's JSON as a JsonElement.
namespace MailCraft.Resources;

public abstract class Resource
{
    private protected Resource(MailCraftClient client) => Client = client;

    private protected MailCraftClient Client { get; }

    private protected static string Segment(string value) => Uri.EscapeDataString(value);

    private protected static IDictionary<string, string?>? Limit(int? limit) =>
        limit is null ? null : new Dictionary<string, string?> { ["limit"] = limit.Value.ToString() };
}

public sealed class Emails : Resource
{
    internal Emails(MailCraftClient client) : base(client) { }

    public Task<JsonElement> SendAsync(SendEmailRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/emails", request, cancellationToken);

    public Task<JsonElement> ListAsync(int? limit = null, CancellationToken cancellationToken = default) =>
        Client.GetAsync("/emails", Limit(limit), cancellationToken);

    public Task<JsonElement> GetAsync(string id, CancellationToken cancellationToken = default) =>
        Client.GetAsync($"/emails/{Segment(id)}", null, cancellationToken);

    /// <summary>Checks an address's format, MX records and disposable domain, without sending.</summary>
    public Task<JsonElement> ValidateAsync(string email, CancellationToken cancellationToken = default) =>
        Client.GetAsync("/emails/validate", new Dictionary<string, string?> { ["email"] = email }, cancellationToken);
}

public sealed class Domains : Resource
{
    internal Domains(MailCraftClient client) : base(client) { }

    public Task<JsonElement> CreateAsync(CreateDomainRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/domains", request, cancellationToken);

    public Task<JsonElement> ListAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/domains", null, cancellationToken);

    public Task<JsonElement> GetAsync(long id, CancellationToken cancellationToken = default) =>
        Client.GetAsync($"/domains/{id}", null, cancellationToken);

    public Task<JsonElement> VerifyAsync(long id, CancellationToken cancellationToken = default) =>
        Client.PostAsync($"/domains/{id}/verify", null, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/domains/{id}", cancellationToken);
}

public sealed class Senders : Resource
{
    internal Senders(MailCraftClient client) : base(client) { }

    public Task<JsonElement> CreateAsync(CreateSenderRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/senders", request, cancellationToken);

    public Task<JsonElement> ListAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/senders", null, cancellationToken);

    public Task<JsonElement> GetAsync(long id, CancellationToken cancellationToken = default) =>
        Client.GetAsync($"/senders/{id}", null, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/senders/{id}", cancellationToken);
}

public sealed class Contacts : Resource
{
    internal Contacts(MailCraftClient client) : base(client) { }

    public Task<JsonElement> UpsertAsync(UpsertContactRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/contacts", request, cancellationToken);

    public Task<JsonElement> ListAsync(int? limit = null, CancellationToken cancellationToken = default) =>
        Client.GetAsync("/contacts", Limit(limit), cancellationToken);

    public Task<JsonElement> GetAsync(string id, CancellationToken cancellationToken = default) =>
        Client.GetAsync($"/contacts/{Segment(id)}", null, cancellationToken);

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/contacts/{Segment(id)}", cancellationToken);

    public Task<JsonElement> UnsubscribeAsync(string id, CancellationToken cancellationToken = default) =>
        Client.PostAsync($"/contacts/{Segment(id)}/unsubscribe", null, cancellationToken);

    public Task<JsonElement> AddToListsAsync(string id, IEnumerable<long> listIds, CancellationToken cancellationToken = default) =>
        Client.PostAsync($"/contacts/{Segment(id)}/lists", new Dictionary<string, object> { ["list_ids"] = listIds }, cancellationToken);

    public Task<JsonElement> ListsAsync(string id, CancellationToken cancellationToken = default) =>
        Client.GetAsync($"/contacts/{Segment(id)}/lists", null, cancellationToken);

    public Task RemoveFromListAsync(string id, long listId, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/contacts/{Segment(id)}/lists/{listId}", cancellationToken);
}

public sealed class Lists : Resource
{
    internal Lists(MailCraftClient client) : base(client) { }

    public Task<JsonElement> CreateAsync(CreateListRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/lists", request, cancellationToken);

    public Task<JsonElement> ListAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/lists", null, cancellationToken);

    public Task<JsonElement> GetAsync(long id, CancellationToken cancellationToken = default) =>
        Client.GetAsync($"/lists/{id}", null, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/lists/{id}", cancellationToken);
}

public sealed class Segments : Resource
{
    internal Segments(MailCraftClient client) : base(client) { }

    public Task<JsonElement> CreateAsync(CreateSegmentRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/segments", request, cancellationToken);

    public Task<JsonElement> ListAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/segments", null, cancellationToken);

    public Task<JsonElement> GetAsync(long id, CancellationToken cancellationToken = default) =>
        Client.GetAsync($"/segments/{id}", null, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/segments/{id}", cancellationToken);
}

public sealed class Properties : Resource
{
    internal Properties(MailCraftClient client) : base(client) { }

    public Task<JsonElement> CreateAsync(CreatePropertyRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/properties", request, cancellationToken);

    public Task<JsonElement> ListAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/properties", null, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/properties/{id}", cancellationToken);
}

public sealed class Templates : Resource
{
    internal Templates(MailCraftClient client) : base(client) { }

    public Task<JsonElement> CreateAsync(TemplateRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/templates", request, cancellationToken);

    public Task<JsonElement> ListAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/templates", null, cancellationToken);

    public Task<JsonElement> GetAsync(long id, CancellationToken cancellationToken = default) =>
        Client.GetAsync($"/templates/{id}", null, cancellationToken);

    /// <summary>Each save becomes a new version. Null properties are left as they are.</summary>
    public Task<JsonElement> UpdateAsync(long id, TemplateRequest request, CancellationToken cancellationToken = default) =>
        Client.PatchAsync($"/templates/{id}", request, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/templates/{id}", cancellationToken);
}

public sealed class TemplateFolders : Resource
{
    internal TemplateFolders(MailCraftClient client) : base(client) { }

    public Task<JsonElement> CreateAsync(string name, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/template-folders", new Dictionary<string, string> { ["name"] = name }, cancellationToken);

    public Task<JsonElement> ListAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/template-folders", null, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/template-folders/{id}", cancellationToken);
}

public sealed class Campaigns : Resource
{
    internal Campaigns(MailCraftClient client) : base(client) { }

    public Task<JsonElement> CreateAsync(CreateCampaignRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/campaigns", request, cancellationToken);

    public Task<JsonElement> ListAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/campaigns", null, cancellationToken);

    public Task<JsonElement> GetAsync(long id, CancellationToken cancellationToken = default) =>
        Client.GetAsync($"/campaigns/{id}", null, cancellationToken);

    public Task<JsonElement> SendAsync(long id, CancellationToken cancellationToken = default) =>
        Client.PostAsync($"/campaigns/{id}/send", null, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/campaigns/{id}", cancellationToken);
}

public sealed class Webhooks : Resource
{
    internal Webhooks(MailCraftClient client) : base(client) { }

    public Task<JsonElement> CreateAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/webhooks", request, cancellationToken);

    public Task<JsonElement> ListAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/webhooks", null, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/webhooks/{id}", cancellationToken);
}

public sealed class Suppressions : Resource
{
    internal Suppressions(MailCraftClient client) : base(client) { }

    public Task<JsonElement> AddAsync(AddSuppressionRequest request, CancellationToken cancellationToken = default) =>
        Client.PostAsync("/suppressions", request, cancellationToken);

    public Task<JsonElement> ListAsync(int? limit = null, CancellationToken cancellationToken = default) =>
        Client.GetAsync("/suppressions", Limit(limit), cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        Client.DeleteAsync($"/suppressions/{id}", cancellationToken);
}

public sealed class Metrics : Resource
{
    internal Metrics(MailCraftClient client) : base(client) { }

    /// <param name="startDate">YYYY-MM-DD</param>
    /// <param name="endDate">YYYY-MM-DD</param>
    /// <param name="cancellationToken"></param>
    public Task<JsonElement> GetAsync(string? startDate = null, string? endDate = null, CancellationToken cancellationToken = default) =>
        Client.GetAsync("/metrics", new Dictionary<string, string?> { ["start_date"] = startDate, ["end_date"] = endDate }, cancellationToken);

    public Task<JsonElement> ReputationAsync(CancellationToken cancellationToken = default) =>
        Client.GetAsync("/reputation", null, cancellationToken);
}
