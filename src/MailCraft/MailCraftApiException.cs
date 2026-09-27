using System.Text.Json;

namespace MailCraft;

/// <summary>
/// Thrown for any non-2xx response from the MailCraft API.
/// </summary>
/// <remarks>
/// Covers both error shapes the API returns: <c>{"error": {"type", "message"}}</c> for
/// business-rule failures (plan limits, suppressed recipients, ...), and
/// <c>{"message", "errors": {"field": [...]}}</c> for validation failures (422).
/// </remarks>
public class MailCraftApiException : Exception
{
    public MailCraftApiException(string message, int status, string? type = null, IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Status = status;
        Type = type;
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    /// <summary>The HTTP status, e.g. 402 or 422.</summary>
    public int Status { get; }

    /// <summary>The business-rule error type, e.g. <c>plan_limit_reached</c>. Null for validation errors.</summary>
    public string? Type { get; }

    /// <summary>Field errors for 422 responses.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    internal static MailCraftApiException FromResponse(int status, string body)
    {
        var fallback = $"Request failed with status {status}";

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return new MailCraftApiException(fallback, status);
            }

            if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
            {
                return new MailCraftApiException(
                    StringOrNull(error, "message") ?? fallback,
                    status,
                    StringOrNull(error, "type"));
            }

            Dictionary<string, string[]>? errors = null;
            if (root.TryGetProperty("errors", out var fields) && fields.ValueKind == JsonValueKind.Object)
            {
                errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(fields.GetRawText());
            }

            return new MailCraftApiException(StringOrNull(root, "message") ?? fallback, status, null, errors);
        }
        catch (JsonException)
        {
            return new MailCraftApiException(fallback, status);
        }
    }

    private static string? StringOrNull(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}
