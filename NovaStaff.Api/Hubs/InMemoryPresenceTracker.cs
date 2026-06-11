// API/Hubs/InMemoryPresenceTracker.cs
using NovaStaff.BusinessLayers.Interfaces;

namespace NovaStaff.API.Hubs;

public class InMemoryPresenceTracker : IPresenceTracker
{
    private readonly Dictionary<int, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    public Task UserConnectedAsync(int userID, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userID, out var conns))
            {
                conns = [];
                _connections[userID] = conns;
            }
            conns.Add(connectionId);
        }
        return Task.CompletedTask;
    }

    public Task UserDisconnectedAsync(int userID, string connectionId)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(userID, out var conns))
            {
                conns.Remove(connectionId);
                if (conns.Count == 0) _connections.Remove(userID);
            }
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsOnlineAsync(int userID)
    {
        lock (_lock)
            return Task.FromResult(
                _connections.TryGetValue(userID, out var conns) && conns.Count > 0);
    }

    public Task<int[]> GetOnlineUserIDsAsync()
    {
        lock (_lock)
            return Task.FromResult(_connections.Keys.ToArray());
    }
}