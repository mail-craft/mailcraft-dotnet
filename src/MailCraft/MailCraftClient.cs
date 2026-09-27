using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MailCraft.Resources;

namespace MailCraft;

/// <summary>
/// The MailCraft API client. Create one per application and reuse it; it is thread-safe.
/// </summary>
/// <example>
/// <code>
/// var mailcraft = new MailCraftClient(Environment.GetEnvironmentVariable("MAILCRAFT_API_KEY"));
///
/// await mailcraft.Emails.SendAsync(new SendEmailRequest
/// {
///     From = "hello@yourdomain.com",
///     To = new[] { "person@example.com" },
///     Subject = "Welcome!",
///     Html = "&lt;p&gt;Thanks for signing up.&lt;/p&gt;",
/// });
/// </code>
/// </example>
public sealed class MailCraftClient : IDisposable
{
    public const string Version = "0.1.0";
    public const string DefaultBaseUrl = "https://api.mailcraft.host/v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;
    private readonly string _baseUrl;

    /// <param name="apiKey">Create one under Settings &gt; API keys.</param>
    /// <param name="baseUrl">Override for staging or self-hosting.</param>
    /// <param name="httpClient">
    /// A pre-built client (e.g. from <c>IHttpClientFactory</c>). When given, you own its lifetime
    /// and timeout; otherwise a client with a 30 second timeout is created.
    /// </param>
    public MailCraftClient(string? apiKey, string baseUrl = DefaultBaseUrl, HttpClient? httpClient = null)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentException("An API key is required. Find yours under Settings > API Keys.", nameof(apiKey));
        }

        _baseUrl = baseUrl.TrimEnd('/');
        _ownsHttpClient = httpClient is null;
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        ApiKey = apiKey;

        Emails = new Emails(this);
        Domains = new Domains(this);
        Senders = new Senders(this);
        Contacts = new Contacts(this);
        Lists = new Lists(this);
        Segments = new Segments(this);
        Properties = new Properties(this);
        Templates = new Templates(this);
        TemplateFolders = new TemplateFolders(this);
        Campaigns = new Campaigns(this);
        Webhooks = new Webhooks(this);
        Suppressions = new Suppressions(this);
        Metrics = new Metrics(this);
    }

    private string ApiKey { get; }

    public Emails Emails { get; }
    public Domains Domains { get; }
    public Senders Senders { get; }
    public Contacts Contacts { get; }
    public Lists Lists { get; }
    public Segments Segments { get; }
    public Properties Properties { get; }
    public Templates Templates { get; }
    public TemplateFolders TemplateFolders { get; }
    public Campaigns Campaigns { get; }
    public Webhooks Webhooks { get; }
    public Suppressions Suppressions { get; }
    public Metrics Metrics { get; }

    internal Task<JsonElement> GetAsync(string path, IDictionary<string, string?>? query, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Get, path + QueryString(query), null, cancellationToken);

    internal Task<JsonElement> PostAsync(string path, object? body, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Post, path, body, cancellationToken);

    internal Task<JsonElement> PatchAsync(string path, object body, CancellationToken cancellationToken) =>
        SendAsync(new HttpMethod("PATCH"), path, body, cancellationToken);

    internal async Task DeleteAsync(string path, CancellationToken cancellationToken) =>
        await SendAsync(HttpMethod.Delete, path, null, cancellationToken).ConfigureAwait(false);

    private async Task<JsonElement> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, _baseUrl + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd($"mailcraft-dotnet/{Version}");

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, body.GetType(), JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
#if NET8_0_OR_GREATER
        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
        var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        var status = (int)response.StatusCode;

        if (status >= 400)
        {
            throw MailCraftApiException.FromResponse(status, raw);
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            return default;
        }

        using var document = JsonDocument.Parse(raw);
        return document.RootElement.Clone();
    }

    private static string QueryString(IDictionary<string, string?>? query)
    {
        if (query is null)
        {
            return "";
        }

        var pairs = query
            .Where(pair => pair.Value is not null)
            .Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value!)}")
            .ToList();

        return pairs.Count == 0 ? "" : "?" + string.Join("&", pairs);
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _http.Dispose();
        }
    }
}
