using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace SmartQuiz.Services
{
    public class UserSessionService
    {
        // Hybrid approach: Use session when available, fallback to session-ID-based storage
        private readonly IHttpContextAccessor _httpContextAccessor;
        // Store sessions by UserId (most stable identifier)
        private static readonly ConcurrentDictionary<int, UserSessionData> _userStorage = new();
        // Map session identifiers to UserId (allows lookup by any session identifier)
        private static readonly ConcurrentDictionary<string, int> _sessionToUserIdMap = new();
        
        private const string SESSION_KEY_USERID = "UserId";
        private const string SESSION_KEY_USERNAME = "UserName";
        private const string SESSION_KEY_USERROLE = "UserRole";

        public UserSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private List<string> GetAllSessionIdentifiers()
        {
            var identifiers = new List<string>();
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (httpContext != null)
            {
                // Try to get session cookie value first (most stable identifier across circuits and requests)
                var sessionCookieName = ".AspNetCore.Session";
                if (httpContext.Request.Cookies.TryGetValue(sessionCookieName, out var sessionCookieValue) && !string.IsNullOrEmpty(sessionCookieValue))
                {
                    // Use the session cookie value directly (without prefix) as it's the most stable
                    identifiers.Add(sessionCookieValue);
                    identifiers.Add($"session_cookie:{sessionCookieValue}");
                }
                
                // Use session ID if available (less stable, but useful as backup)
                if (httpContext.Session != null && httpContext.Session.IsAvailable && !string.IsNullOrEmpty(httpContext.Session.Id))
                {
                    identifiers.Add($"session_id:{httpContext.Session.Id}");
                }
                
                // Use connection ID (least stable, changes with each SignalR connection)
                if (!string.IsNullOrEmpty(httpContext.Connection?.Id))
                {
                    identifiers.Add($"connection_id:{httpContext.Connection.Id}");
                }
                
                Console.WriteLine($"GetAllSessionIdentifiers: Found {identifiers.Count} identifiers: {string.Join(", ", identifiers)}");
            }
            else
            {
                Console.WriteLine("GetAllSessionIdentifiers: HttpContext is null");
            }
            
            return identifiers;
        }
        
        private string? GetSessionId()
        {
            var identifiers = GetAllSessionIdentifiers();
            return identifiers.FirstOrDefault();
        }

        private ISession? GetSession()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session != null)
            {
                // Try to load the session if it's not already loaded
                try
                {
                    if (!httpContext.Session.IsAvailable)
                    {
                        httpContext.Session.LoadAsync().Wait();
                    }
                    if (httpContext.Session.IsAvailable)
                    {
                        return httpContext.Session;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetSession: Error loading session: {ex.Message}");
                }
            }
            return null;
        }

        public int? CurrentUserId
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    Console.WriteLine("CurrentUserId: HttpContext is null");
                    return null;
                }
                
                const string USER_ID_COOKIE = "SmartQuiz_UserId";
                
                // First, try to get from ASP.NET Session (most reliable in Blazor Server)
                var session = GetSession();
                if (session != null)
                {
                    try
                    {
                        Console.WriteLine($"CurrentUserId: Checking ASP.NET Session...");
                        // Try GetString first (simpler)
                        var userDataJson = session.GetString("UserData");
                        if (!string.IsNullOrEmpty(userDataJson))
                        {
                            Console.WriteLine($"CurrentUserId: Found UserData in session: {userDataJson}");
                            var userData = JsonSerializer.Deserialize<UserSessionData>(userDataJson);
                            if (userData != null && _userStorage.ContainsKey(userData.UserId))
                            {
                                var userId = userData.UserId;
                                Console.WriteLine($"CurrentUserId: ✅ Found from ASP.NET Session: {userId}");
                                // Update cookie to match session if different
                                if (httpContext.Request.Cookies.TryGetValue(USER_ID_COOKIE, out var cookieValue))
                                {
                                    if (cookieValue != userId.ToString())
                                    {
                                        Console.WriteLine($"CurrentUserId: Updating cookie from {cookieValue} to {userId} to match session");
                                        if (!httpContext.Response.HasStarted)
                                        {
                                            var cookieOptions = new CookieOptions
                                            {
                                                HttpOnly = true,
                                                Secure = false,
                                                SameSite = SameSiteMode.Lax,
                                                Expires = DateTimeOffset.UtcNow.AddDays(30)
                                            };
                                            httpContext.Response.Cookies.Append(USER_ID_COOKIE, userId.ToString(), cookieOptions);
                                        }
                                        else
                                        {
                                            TrySetCookieOnNextRequest(userId);
                                        }
                                    }
                                }
                                return userId;
                            }
                            else
                            {
                                Console.WriteLine($"CurrentUserId: UserData deserialized but UserId {userData?.UserId} not found in storage");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"CurrentUserId: No UserData found in ASP.NET Session");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"CurrentUserId: Error reading from session: {ex.Message}, StackTrace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine($"CurrentUserId: ASP.NET Session is not available");
                }
                
                // Second, try to get from cookie (fallback)
                if (httpContext.Request.Cookies.TryGetValue(USER_ID_COOKIE, out var userIdCookieValue))
                {
                    Console.WriteLine($"CurrentUserId: Found cookie value: {userIdCookieValue}");
                    if (int.TryParse(userIdCookieValue, out var userId))
                    {
                        // Verify the user data exists in storage
                        if (_userStorage.TryGetValue(userId, out var userData))
                        {
                            Console.WriteLine($"CurrentUserId: ✅ Returning from cookie: {userId}");
                            // Try to update session if it's missing (even if response has started, session can sometimes be written)
                            if (session != null)
                            {
                                try
                                {
                                    var jsonString = JsonSerializer.Serialize(userData);
                                    session.SetString("UserData", jsonString);
                                    Console.WriteLine($"CurrentUserId: ✅ Updated session with user data from cookie");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"CurrentUserId: ⚠️ Error updating session: {ex.Message}");
                                }
                            }
                            return userId;
                        }
                        else
                        {
                            Console.WriteLine($"CurrentUserId: ⚠️ UserId {userId} from cookie not found in storage. Available users: {string.Join(", ", _userStorage.Keys)}");
                            // If cookie has wrong user, but we have users in storage, use the first one as emergency fallback
                            if (_userStorage.Count == 1)
                            {
                                var singleUser = _userStorage.Keys.First();
                                Console.WriteLine($"CurrentUserId: ⚠️ Emergency fallback - using single user in storage: {singleUser}");
                                // Update cookie and session
                                if (!httpContext.Response.HasStarted)
                                {
                                    var cookieOptions = new CookieOptions
                                    {
                                        HttpOnly = true,
                                        Secure = false,
                                        SameSite = SameSiteMode.Lax,
                                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                                    };
                                    httpContext.Response.Cookies.Append(USER_ID_COOKIE, singleUser.ToString(), cookieOptions);
                                }
                                if (session != null)
                                {
                                    try
                                    {
                                        var jsonString = JsonSerializer.Serialize(_userStorage[singleUser]);
                                        session.SetString("UserData", jsonString);
                                    }
                                    catch { }
                                }
                                return singleUser;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"CurrentUserId: Cookie not found. Available cookies: {string.Join(", ", httpContext.Request.Cookies.Keys)}");
                    // If no cookie but we have users in storage, use the first one as emergency fallback
                    if (_userStorage.Count == 1)
                    {
                        var singleUser = _userStorage.Keys.First();
                        Console.WriteLine($"CurrentUserId: ⚠️ Emergency fallback - using single user in storage: {singleUser}");
                        // Update cookie and session
                        if (!httpContext.Response.HasStarted)
                        {
                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = false,
                                SameSite = SameSiteMode.Lax,
                                Expires = DateTimeOffset.UtcNow.AddDays(30)
                            };
                            httpContext.Response.Cookies.Append(USER_ID_COOKIE, singleUser.ToString(), cookieOptions);
                        }
                        if (session != null)
                        {
                            try
                            {
                                var jsonString = JsonSerializer.Serialize(_userStorage[singleUser]);
                                session.SetString("UserData", jsonString);
                            }
                            catch { }
                        }
                        return singleUser;
                    }
                }
                
                Console.WriteLine($"CurrentUserId: Returning null. Storage has {_userStorage.Count} users");
                return null;
            }
        }

        public string? CurrentUserName
        {
            get
            {
                var userId = CurrentUserId;
                if (userId.HasValue && _userStorage.TryGetValue(userId.Value, out var sessionData))
                {
                    return sessionData.UserName;
                }
                
                // Fallback: try ASP.NET Session
                var httpContext = _httpContextAccessor.HttpContext;
                var session = GetSession();
                if (session != null && httpContext != null && !httpContext.Response.HasStarted)
                {
                    try
                {
                    var userName = session.GetString(SESSION_KEY_USERNAME);
                    if (!string.IsNullOrEmpty(userName))
                        return userName;
                }
                    catch (InvalidOperationException)
                {
                        // Response has started
                    }
                }
                
                return null;
            }
        }

        public string? CurrentUserRole
        {
            get
            {
                var userId = CurrentUserId;
                if (userId.HasValue && _userStorage.TryGetValue(userId.Value, out var sessionData))
                {
                    return sessionData.UserRole;
                }
                
                // Fallback: try ASP.NET Session
                var httpContext = _httpContextAccessor.HttpContext;
                var session = GetSession();
                if (session != null && httpContext != null && !httpContext.Response.HasStarted)
                {
                    try
                {
                    var role = session.GetString(SESSION_KEY_USERROLE);
                    if (!string.IsNullOrEmpty(role))
                        return role;
                }
                    catch (InvalidOperationException)
                {
                        // Response has started
                    }
                }
                
                return null;
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return CurrentUserId.HasValue;
            }
        }

        public void SetCurrentUser(int userId, string userName, string role)
        {
            Console.WriteLine($"SetCurrentUser called: UserId={userId}, UserName={userName}, Role={role}");
            
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                Console.WriteLine("SetCurrentUser: HttpContext is null!");
                // Still store in user storage even if HttpContext is null
                _userStorage.AddOrUpdate(userId,
                    new UserSessionData { UserId = userId, UserName = userName, UserRole = role },
                    (key, existing) => new UserSessionData { UserId = userId, UserName = userName, UserRole = role });
                return;
            }
            
            // ALWAYS store in user-based storage first (most stable - UserId doesn't change)
            _userStorage.AddOrUpdate(userId,
                new UserSessionData { UserId = userId, UserName = userName, UserRole = role },
                (key, existing) => new UserSessionData { UserId = userId, UserName = userName, UserRole = role });
            Console.WriteLine($"SetCurrentUser: Stored in user storage. Total users: {_userStorage.Count}");
            
            // Store UserId in a cookie (most stable across requests)
            const string USER_ID_COOKIE = "SmartQuiz_UserId";
            try
            {
                if (!httpContext.Response.HasStarted)
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // Set to true in production with HTTPS
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(30) // 30 days expiration
                    };
                    httpContext.Response.Cookies.Append(USER_ID_COOKIE, userId.ToString(), cookieOptions);
                    Console.WriteLine($"SetCurrentUser: Cookie set successfully: {userId}");
                }
                else
                {
                    Console.WriteLine("SetCurrentUser: Response has started, cannot set cookie. Will be set on next request.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetCurrentUser: Error setting cookie: {ex.Message}");
            }
            
            // Also store in ASP.NET Session as JSON (most reliable in Blazor Server)
            // Try to store even if response has started (session can sometimes be written after response starts)
            var session = GetSession();
            if (session != null)
            {
                try
                {
                    var userData = new UserSessionData { UserId = userId, UserName = userName, UserRole = role };
                    var jsonString = JsonSerializer.Serialize(userData);
                    session.SetString("UserData", jsonString);
                    Console.WriteLine($"SetCurrentUser: ✅ Stored in ASP.NET Session as JSON: {jsonString}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"SetCurrentUser: ⚠️ Cannot set ASP.NET Session (response may have started): {ex.Message}");
                    // Session will be set on next request when cookie is read
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SetCurrentUser: ⚠️ Error setting ASP.NET Session: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"SetCurrentUser: ⚠️ ASP.NET Session is not available");
            }
            
            // Also store mapping from ALL session identifiers to UserId (backup)
            // Prioritize the session cookie value as it's the most stable identifier
            var sessionIdentifiers = GetAllSessionIdentifiers();
            foreach (var sessionId in sessionIdentifiers)
            {
                _sessionToUserIdMap.AddOrUpdate(sessionId, userId, (key, existing) => userId);
                Console.WriteLine($"SetCurrentUser: Stored session mapping: {sessionId} -> {userId}");
            }
            
            // Also store a reverse mapping: UserId -> session cookie (for quick lookup)
            // This helps when we need to find which session belongs to which user
            var sessionCookieName = ".AspNetCore.Session";
            if (httpContext.Request.Cookies.TryGetValue(sessionCookieName, out var sessionCookieValue) && !string.IsNullOrEmpty(sessionCookieValue))
            {
                // Store the raw cookie value as a key too
                _sessionToUserIdMap.AddOrUpdate(sessionCookieValue, userId, (key, existing) => userId);
                Console.WriteLine($"SetCurrentUser: Stored session cookie mapping: {sessionCookieValue} -> {userId}");
            }
            
            // If cookie wasn't set, try to set it on the next request
            if (httpContext.Response.HasStarted)
            {
                // Store a flag that we need to set the cookie on next request
                // We'll check this in CurrentUserId getter
            }
        }

        public void ClearCurrentUser()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = CurrentUserId;
            
            // Remove cookie
            if (httpContext != null)
            {
                const string USER_ID_COOKIE = "SmartQuiz_UserId";
                httpContext.Response.Cookies.Delete(USER_ID_COOKIE);
            }
            
            // Remove from ASP.NET Session
            var session = GetSession();
            if (session != null)
            {
                session.Remove(SESSION_KEY_USERID);
                session.Remove(SESSION_KEY_USERNAME);
                session.Remove(SESSION_KEY_USERROLE);
            }
            
            // Remove from user storage
            if (userId.HasValue)
            {
                _userStorage.TryRemove(userId.Value, out _);
            }
            
            // Remove session mapping
            var sessionId = GetSessionId();
            if (!string.IsNullOrEmpty(sessionId))
            {
                _sessionToUserIdMap.TryRemove(sessionId, out _);
            }
        }

        private void TrySetCookieOnNextRequest(int userId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.Response.HasStarted)
                return;
                
            const string USER_ID_COOKIE = "SmartQuiz_UserId";
            // Always update cookie if it's different from the current userId
            var currentCookieValue = httpContext.Request.Cookies[USER_ID_COOKIE];
            if (currentCookieValue == null || currentCookieValue != userId.ToString())
            {
                try
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // Set to true in production with HTTPS
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(30) // 30 days expiration
                    };
                    httpContext.Response.Cookies.Append(USER_ID_COOKIE, userId.ToString(), cookieOptions);
                    Console.WriteLine($"TrySetCookieOnNextRequest: Cookie set successfully: {userId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TrySetCookieOnNextRequest: Error setting cookie: {ex.Message}");
                }
            }
        }

        private class UserSessionData
        {
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public string? UserRole { get; set; }
        }
    }
}

