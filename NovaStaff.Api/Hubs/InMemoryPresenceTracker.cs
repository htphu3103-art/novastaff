// API/Hubs/InMemoryPresenceTracker.cs
using NovaStaff.BusinessLayers.Interfaces;

namespace NovaStaff.API.Hubs;

public class InMemoryPresenceTracker : IPresenceTracker
{
    private readonly Dictionary<int, HashSet<string>> _connections = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1); // thay lock

    public async Task UserConnectedAsync(int userID, string connectionId)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!_connections.TryGetValue(userID, out var conns))
            {
                conns = [];
                _connections[userID] = conns;
            }
            conns.Add(connectionId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UserDisconnectedAsync(int userID, string connectionId)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_connections.TryGetValue(userID, out var conns))
            {
                conns.Remove(connectionId);
                if (conns.Count == 0) _connections.Remove(userID);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> IsOnlineAsync(int userID)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _connections.TryGetValue(userID, out var conns) && conns.Count > 0;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int[]> GetOnlineUserIDsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _connections.Keys.ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}