using System.Text.Json;
using System.Text.Json.Serialization;

namespace MailCraft;

// Request bodies. Null properties are left out of the request.

/// <summary>A transactional email to send.</summary>
public class SendEmailRequest
{
    [JsonPropertyName("from")] public string From { get; set; } = "";
    [JsonPropertyName("to")] public IEnumerable<string> To { get; set; } = Array.Empty<string>();
    [JsonPropertyName("subject")] public string Subject { get; set; } = "";
    [JsonPropertyName("html")] public string? Html { get; set; }
    [JsonPropertyName("text")] public string? Text { get; set; }
    [JsonPropertyName("cc")] public IEnumerable<string>? Cc { get; set; }
    [JsonPropertyName("bcc")] public IEnumerable<string>? Bcc { get; set; }
    [JsonPropertyName("reply_to")] public string? ReplyTo { get; set; }
    [JsonPropertyName("headers")] public IDictionary<string, string>? Headers { get; set; }
    [JsonPropertyName("tags")] public IDictionary<string, string>? Tags { get; set; }
}

/// <summary>Registers a sending domain.</summary>
public class CreateDomainRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("region")] public string? Region { get; set; }
}

/// <summary>Creates a sender identity on a verified domain.</summary>
public class CreateSenderRequest
{
    [JsonPropertyName("domain_id")] public long DomainId { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("reply_to")] public string? ReplyTo { get; set; }
}

/// <summary>Creates a contact, or updates it if one exists for this email.</summary>
public class UpsertContactRequest
{
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("first_name")] public string? FirstName { get; set; }
    [JsonPropertyName("last_name")] public string? LastName { get; set; }
    /// <summary><c>subscribed</c>, <c>unsubscribed</c> or <c>pending</c>.</summary>
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("properties")] public IDictionary<string, object?>? Properties { get; set; }
}

/// <summary>Creates a list. <see cref="Type"/> is <c>static</c> or <c>dynamic</c> (with <see cref="SegmentId"/>).</summary>
public class CreateListRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("segment_id")] public long? SegmentId { get; set; }
}

/// <summary>Creates a segment from filter conditions.</summary>
public class CreateSegmentRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("filters")] public SegmentFilters Filters { get; set; } = new();
}

/// <summary>Matches contacts on all (<c>and</c>) or any (<c>or</c>) of its conditions.</summary>
public class SegmentFilters
{
    [JsonPropertyName("operator")] public string? Operator { get; set; }
    [JsonPropertyName("conditions")] public IEnumerable<SegmentCondition> Conditions { get; set; } = Array.Empty<SegmentCondition>();
}

/// <summary>Compares a contact field. Operator is eq, neq, gt, gte, lt, lte or contains.</summary>
public class SegmentCondition
{
    [JsonPropertyName("field")] public string Field { get; set; } = "";
    [JsonPropertyName("operator")] public string? Operator { get; set; }
    [JsonPropertyName("value")] public object? Value { get; set; }
}

/// <summary>Creates a custom contact property. Type is text, number, boolean, date or list.</summary>
public class CreatePropertyRequest
{
    [JsonPropertyName("key")] public string Key { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "text";
    [JsonPropertyName("default_value")] public object? DefaultValue { get; set; }
}

/// <summary>Creates or updates a template. Name and Subject are required to create.</summary>
public class TemplateRequest
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("subject")] public string? Subject { get; set; }
    [JsonPropertyName("html_body")] public string? HtmlBody { get; set; }
    [JsonPropertyName("text_body")] public string? TextBody { get; set; }
    [JsonPropertyName("template_folder_id")] public long? TemplateFolderId { get; set; }
    [JsonPropertyName("variables")] public IEnumerable<JsonElement>? Variables { get; set; }
}

/// <summary>Creates a draft campaign for a list or a segment.</summary>
public class CreateCampaignRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("subject")] public string Subject { get; set; } = "";
    [JsonPropertyName("template_id")] public long TemplateId { get; set; }
    [JsonPropertyName("sender_id")] public long SenderId { get; set; }
    [JsonPropertyName("list_id")] public long? ListId { get; set; }
    [JsonPropertyName("segment_id")] public long? SegmentId { get; set; }
}

/// <summary>Subscribes a URL to events.</summary>
public class CreateWebhookRequest
{
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("events")] public IEnumerable<string> Events { get; set; } = Array.Empty<string>();
    [JsonPropertyName("description")] public string? Description { get; set; }
}

/// <summary>Stops sending to an address.</summary>
public class AddSuppressionRequest
{
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}
