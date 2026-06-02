using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace DoAnCS.Hubs
{
    public class UserSessionHub : Hub
    {
        // Tracks active connections for each UserID: UserID -> Dictionary of ConnectionID -> (SessionId, LoginTime)
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, (string SessionId, long LoginTime)>> UserConnections = new();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var user = httpContext?.User;

            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userIdClaim = user.FindFirst("UserID")?.Value;
                var sessionIdClaim = user.FindFirst("SessionId")?.Value;
                var loginTimeClaim = user.FindFirst("LoginTime")?.Value;

                if (int.TryParse(userIdClaim, out int userId) && !string.IsNullOrEmpty(sessionIdClaim) && long.TryParse(loginTimeClaim, out long loginTime))
                {
                    // Add this connection to the user's active connections list
                    var connections = UserConnections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, (string, long)>());
                    connections[Context.ConnectionId] = (sessionIdClaim, loginTime);

                    // Check for conflicting sessions and logout older sessions if necessary
                    await CheckSessionConflict(userId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var user = httpContext?.User;

            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userIdClaim = user.FindFirst("UserID")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    if (UserConnections.TryGetValue(userId, out var connections))
                    {
                        connections.TryRemove(Context.ConnectionId, out _);
                        if (connections.IsEmpty)
                        {
                            UserConnections.TryRemove(userId, out _);
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task CheckSessionConflict(int userId)
        {
            if (UserConnections.TryGetValue(userId, out var connections))
            {
                // Group active connections by their SessionId
                var activeSessions = connections.ToList()
                    .GroupBy(c => c.Value.SessionId)
                    .Select(g => new
                    {
                        SessionId = g.Key,
                        LoginTime = g.First().Value.LoginTime,
                        ConnectionIds = g.Select(c => c.Key).ToList()
                    })
                    .OrderBy(s => s.LoginTime) // Oldest first
                    .ToList();

                // If there are multiple active sessions at the same time
                if (activeSessions.Count > 1)
                {
                    // The oldest session gets kicked
                    var oldestSession = activeSessions.First();
                    
                    // Send ForceLogout to all connections of the oldest session
                    foreach (var connectionId in oldestSession.ConnectionIds)
                    {
                        await Clients.Client(connectionId).SendAsync("ForceLogout");
                    }
                }
            }
        }
    }
}
