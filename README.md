# MailCraft for .NET

Official .NET SDK for the [MailCraft](https://mailcraft.host) email API: transactional email, contacts, lists, segments, templates, campaigns, webhooks and more.

Targets .NET 6 and .NET 8. No dependencies beyond the framework (`HttpClient` + `System.Text.Json`).

## Install

```bash
dotnet add package MailCraft
```

## Quick start

```csharp
using MailCraft;

var mailcraft = new MailCraftClient(Environment.GetEnvironmentVariable("MAILCRAFT_API_KEY"));

var email = await mailcraft.Emails.SendAsync(new SendEmailRequest
{
    From = "hello@yourdomain.com",
    To = new[] { "person@example.com" },
    Subject = "Welcome!",
    Html = "<p>Thanks for signing up.</p>",
});

Console.WriteLine(email.GetProperty("data").GetProperty("id").GetString());
```

Create an API key under **Settings → API keys** in your MailCraft dashboard. Create one `MailCraftClient` and reuse it; it's thread-safe. Every method accepts an optional `CancellationToken`.

## Resources

Every MailCraft SDK has the same resources and methods:

| Resource | Methods |
| --- | --- |
| `Emails` | `SendAsync`, `ListAsync`, `GetAsync`, `ValidateAsync` |
| `Domains` | `CreateAsync`, `ListAsync`, `GetAsync`, `VerifyAsync`, `DeleteAsync` |
| `Senders` | `CreateAsync`, `ListAsync`, `GetAsync`, `DeleteAsync` |
| `Contacts` | `UpsertAsync`, `ListAsync`, `GetAsync`, `DeleteAsync`, `UnsubscribeAsync`, `AddToListsAsync`, `ListsAsync`, `RemoveFromListAsync` |
| `Lists` | `CreateAsync`, `ListAsync`, `GetAsync`, `DeleteAsync` |
| `Segments` | `CreateAsync`, `ListAsync`, `GetAsync`, `DeleteAsync` |
| `Properties` | `CreateAsync`, `ListAsync`, `DeleteAsync` |
| `Templates` | `CreateAsync`, `ListAsync`, `GetAsync`, `UpdateAsync`, `DeleteAsync` |
| `TemplateFolders` | `CreateAsync`, `ListAsync`, `DeleteAsync` |
| `Campaigns` | `CreateAsync`, `ListAsync`, `GetAsync`, `SendAsync`, `DeleteAsync` |
| `Webhooks` | `CreateAsync`, `ListAsync`, `DeleteAsync` |
| `Suppressions` | `AddAsync`, `ListAsync`, `DeleteAsync` |
| `Metrics` | `GetAsync`, `ReputationAsync` |

Requests take typed classes (`SendEmailRequest`, `UpsertContactRequest`, ...); null properties are left out of the request. Responses are the API's JSON as a `JsonElement`. See the [API reference](https://docs.mailcraft.host/api-reference) for every field.

## Errors

Any non-2xx response throws `MailCraftApiException`:

```csharp
try
{
    await mailcraft.Domains.CreateAsync(new CreateDomainRequest { Name = "acme.com" });
}
catch (MailCraftApiException error)
{
    Console.WriteLine(error.Status);  // e.g. 402
    Console.WriteLine(error.Type);    // e.g. "plan_limit_reached" (business-rule errors)
    Console.WriteLine(error.Errors);  // field errors for 422 validation failures
}
```

## Options

```csharp
var mailcraft = new MailCraftClient(
    apiKey,
    baseUrl: "https://api.mailcraft.host/v1", // override for staging or self-hosting
    httpClient: httpClientFactory.CreateClient("mailcraft")); // optional; you own its lifetime and timeout
```

Without a `httpClient`, the SDK creates one with a 30 second timeout and disposes it with the client.

## Development

```bash
dotnet test tests/MailCraft.Tests
```

The tests run the real client through a fake `HttpMessageHandler`, so every request goes through the same code a user's would.

## License

MIT
