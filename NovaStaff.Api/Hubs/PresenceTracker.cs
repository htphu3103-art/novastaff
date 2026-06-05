using System.Collections.Generic;
using System.Threading.Tasks;
using NovaStaff.BusinessLayers.Interfaces;

namespace NovaStaff.Hubs;

/// <summary>
/// Thread-safe in-memory tracker: UserID ↔ Set of ConnectionIDs
/// Đủ dùng cho team nội bộ. Nếu scale multi-server thì chuyển sang Redis.
/// </summary>
public class PresenceTracker : IPresenceTracker
{
    // userID → set of connectionIds (user có thể mở nhiều tab)
    private readonly Dictionary<int, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    public Task UserConnected(int userID, string connectionId)
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

    public Task UserDisconnected(int userID, string connectionId)
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

    public bool IsOnline(int userID)
    {
        lock (_lock)
        {
            return _connections.TryGetValue(userID, out var conns) && conns.Count > 0;
        }
    }

    public int[] GetOnlineUserIDs()
    {
        lock (_lock)
        {
            return [.. _connections.Keys];
        }
    }
}
