using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Group5.Shared
{
    // Session storage using thread-safe dictionary
    // Uses a combination of connection ID and remote IP for Blazor Server compatibility
    public static class UserSession
    {
        // Dictionary to store sessions - keyed by session ID
        private static readonly ConcurrentDictionary<string, SessionData> _sessions = new();
        
        // Service provider to get HttpContextAccessor per request
        private static IServiceProvider? _serviceProvider;

        // Initialize the service provider (call this from Program.cs)
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Get HttpContextAccessor from service provider
        private static IHttpContextAccessor? GetHttpContextAccessor()
        {
            if (_serviceProvider == null) return null;
            try
            {
                return _serviceProvider.GetService<IHttpContextAccessor>();
            }
            catch
            {
                return null;
            }
        }

        // Generate or get session key for current context
        private static string GetSessionKey()
        {
            try
            {
                var httpContextAccessor = GetHttpContextAccessor();
                
                // Try to get from HttpContext first
                if (httpContextAccessor?.HttpContext != null)
                {
                    var httpContext = httpContextAccessor.HttpContext;
                    
                    // PRIORITIZE COOKIE-BASED SESSION ID for persistence across navigation
                    if (httpContext.Request.Cookies.TryGetValue("SessionId", out var cookieSessionId) && !string.IsNullOrEmpty(cookieSessionId))
                    {
                        // Use cookie session ID if available (more persistent)
                        return cookieSessionId;
                    }
                    
                    // Fallback: Use connection ID + remote IP as session key
                    var connectionId = httpContext.Connection.Id;
                    var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var sessionKey = $"session_{connectionId}_{remoteIp}";
                    
                    // Store session ID in cookie for persistence across navigation
                    try
                    {
                        httpContext.Response.Cookies.Append("SessionId", sessionKey, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = false, // Set to true in production with HTTPS
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddHours(24)
                        });
                    }
                    catch
                    {
                        // Cookie might not be writable, that's okay - we'll use connection-based key
                    }
                    
                    return sessionKey;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error getting session key: {ex.Message}");
            }

            // Fallback: use a simple identifier (shouldn't happen in normal operation)
            return $"session_{DateTime.UtcNow.Ticks}";
        }

        public static string Email
        {
            get
            {
                var key = GetSessionKey();
                return _sessions.TryGetValue(key, out var data) ? data.Email : string.Empty;
            }
            set
            {
                var key = GetSessionKey();
                _sessions.AddOrUpdate(key,
                    new SessionData { Email = value, Username = string.Empty, Role = string.Empty },
                    (k, v) => { v.Email = value; return v; });
            }
        }

        public static string Username
        {
            get
            {
                var key = GetSessionKey();
                return _sessions.TryGetValue(key, out var data) ? data.Username : string.Empty;
            }
            set
            {
                var key = GetSessionKey();
                _sessions.AddOrUpdate(key,
                    new SessionData { Email = Email, Username = value, Role = Role },
                    (k, v) => { v.Username = value; return v; });
            }
        }

        public static string Role
        {
            get
            {
                var key = GetSessionKey();
                return _sessions.TryGetValue(key, out var data) ? data.Role : string.Empty;
            }
            set
            {
                var key = GetSessionKey();
                _sessions.AddOrUpdate(key,
                    new SessionData { Email = Email, Username = Username, Role = value },
                    (k, v) => { v.Role = value; return v; });
            }
        }

        public static bool IsLoggedIn
        {
            get
            {
                var key = GetSessionKey();
                return _sessions.TryGetValue(key, out var data) && 
                       !string.IsNullOrWhiteSpace(data.Email) && 
                       !string.IsNullOrWhiteSpace(data.Role);
            }
        }

        public static void Clear()
        {
            var key = GetSessionKey();
            _sessions.TryRemove(key, out _);
            
            // Clear cookie
            var httpContextAccessor = GetHttpContextAccessor();
            if (httpContextAccessor?.HttpContext != null)
            {
                httpContextAccessor.HttpContext.Response.Cookies.Delete("SessionId");
            }
        }

        private class SessionData
        {
            public string Email { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}
