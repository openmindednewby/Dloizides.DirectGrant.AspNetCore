using Microsoft.AspNetCore.Http;

namespace Dloizides.DirectGrant.AspNetCore;

/// <summary>
/// Validates the shared-secret header on an internal "external direct-grant"
/// verify request. The comparison is constant-time and fails closed when the
/// key is unconfigured, so an unset secret never authorizes anything.
/// </summary>
public interface IExternalDirectGrantGate
{
    /// <summary>
    /// Returns <c>true</c> iff the request presents the correct shared secret
    /// for the named method (e.g. "otp" / "pin"). Returns <c>false</c> when the
    /// header is missing/blank, the presented value is wrong, or the key/header
    /// is unconfigured. The caller is responsible for issuing the 401 (no body)
    /// when this returns <c>false</c>.
    /// </summary>
    /// <param name="httpContext">The incoming request context.</param>
    /// <param name="name">
    /// The method name used when the gate was registered via
    /// <c>AddExternalDirectGrant(name, ...)</c>.
    /// </param>
    bool IsAuthorized(HttpContext httpContext, string name);
}
