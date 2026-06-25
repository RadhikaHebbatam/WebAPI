namespace ProductCatalogueAPI.Middleware;

/// <summary>
/// WHY: HTTP security headers tell browsers and clients how to
/// behave safely with your API responses.
/// These are checked by security scanners such as OWASP ZAP
/// and are expected in any production API.
/// Missing headers means an immediate fail on security audits.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // WHY remove these headers:
        // Attackers use Server and X-Powered-By to fingerprint
        // your stack and target known vulnerabilities.
        // Never advertise what software you are running.
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        context.Response.Headers.Remove("X-AspNet-Version");
        context.Response.Headers.Remove("X-AspNetMvc-Version");

        // WHY nosniff:
        // Stops browsers from guessing content type.
        // Prevents MIME sniffing attacks that can lead to XSS.
        context.Response.Headers.Append(
            "X-Content-Type-Options", "nosniff");

        // WHY DENY:
        // Prevents your API responses being embedded in iframes
        // on malicious sites — blocks clickjacking attacks.
        context.Response.Headers.Append(
            "X-Frame-Options", "DENY");

        // WHY XSS-Protection:
        // Older browser protection against reflected XSS attacks.
        context.Response.Headers.Append(
            "X-XSS-Protection", "1; mode=block");

        // WHY Referrer-Policy:
        // Controls how much referrer info is sent with requests.
        // Protects sensitive data that might be in your URLs.
        context.Response.Headers.Append(
            "Referrer-Policy", "strict-origin-when-cross-origin");

        // WHY Permissions-Policy:
        // Disables browser features your API does not need.
        // Reduces attack surface on any client consuming your API.
        context.Response.Headers.Append(
            "Permissions-Policy",
            "camera=(), microphone=(), geolocation=(), payment=()");

        await _next(context);
    }
}