using Microsoft.AspNetCore.Http;

namespace Dloizides.DirectGrant.AspNetCore;

/// <summary>Validates the shared-secret header on an internal external direct-grant verify request.</summary>
public interface IExternalDirectGrantGate
{
    /// <summary>Returns true only when the request carries the correct shared secret for the named method.</summary>
    /// <param name="httpContext">The incoming request context.</param>
    /// <param name="name">The method name passed to AddExternalDirectGrant.</param>
    bool IsAuthorized(HttpContext httpContext, string name);
}
