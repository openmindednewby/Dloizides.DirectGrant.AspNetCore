# Dloizides.DirectGrant.AspNetCore

A tiny shared-secret **gate** for "external direct-grant" internal verify
endpoints.

## The pattern this supports

A custom Keycloak **direct-grant authenticator** lets one realm flow accept a
non-password credential (email OTP, event-staff PIN, …) by calling back to the
owning service over the in-cluster network to verify it:

```
KC direct-grant authenticator  ──POST {realm, …credential}──▶  /api/v1/auth/internal/verify-{method}
        (X-{Method}-Internal-Key: <shared secret>)                     │
                                                                       ▼
                                                          service verifies + returns {valid,…}
```

Every such callback endpoint must defend its `AllowAnonymous` route with a
shared-secret check. That check is the **one thing they all duplicate** — and
it is security-critical, so it should exist once, audited.

## What it provides

- `AddExternalDirectGrant(name, configure)` — registers a **named**
  `ExternalDirectGrantOptions` (header name + shared secret) per method, so
  several methods (`"otp"`, `"pin"`, …) coexist in one service. The gate
  service is registered idempotently.
- `AddExternalDirectGrant(name, configuration, sectionName, headerName)` —
  convenience overload that binds the secret from
  `{sectionName}:InternalApiKey` (reloadable).
- `IExternalDirectGrantGate.IsAuthorized(httpContext, name)` — a
  **constant-time**, **fail-closed** header check. Returns `false` (caller
  issues a bodyless 401) when the header is missing/blank/wrong; logs an error
  and rejects when the key or header name is unconfigured (an unset secret
  never authorizes anything).

It deliberately does **not** own the verify request/response DTOs or the
credential-validation logic — those differ per method and stay in each service.

## Usage

```csharp
// Program.cs / startup
builder.Services.AddExternalDirectGrant(
    "otp", builder.Configuration, sectionName: "Otp", headerName: "X-Otp-Internal-Key");

// in the verify endpoint (FastEndpoints / minimal API)
public VerifyOtp(IMediator mediator, IExternalDirectGrantGate gate) { … }

if (!gate.IsAuthorized(HttpContext, "otp"))
{
    await Send.UnauthorizedAsync(ct);   // 401, no body — never leaks which check failed
    return;
}
```

## Design notes

- **Constant-time:** `CryptographicOperations.FixedTimeEquals` over UTF-8 bytes
  — no early-exit timing side channel on the secret.
- **Fail-closed:** unconfigured key/header rejects everything (logged as an
  operator error, not a client 401 reason).
- **No body on reject:** the caller returns 401 with no payload, so the gate
  never reveals missing-vs-wrong-key.
