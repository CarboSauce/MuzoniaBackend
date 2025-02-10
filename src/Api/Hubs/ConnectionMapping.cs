using System.Collections.Concurrent;

namespace Muzonia.Api.Hubs;

// Taken from
// https://learn.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/mapping-users-to-connections
public class ConnectionMapping<T>
{
    private readonly ConcurrentDictionary<T, HashSet<string>> _connections =
        new();

    public int Count
    {
        get { return _connections.Count; }
    }

    public void Add(T key, string connectionId)
    {
        _connections.AddOrUpdate(
            key,
            new HashSet<string>() { connectionId },
            (_, connections) =>
            {
                lock (connections)
                {
                    connections.Add(connectionId);
                    return connections;
                }
            }
        );
    }

    public bool Remove(T key, string connectionId)
    {
        if (_connections.TryGetValue(key, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    _connections.TryRemove(key, out _);
                }
                return true;
            }
        }
        return false;
    }

    public void RemoveAll(T key)
    {
        _connections.TryRemove(key, out _);
    }

    public string[] GetAll(T key)
    {
        if (_connections.TryGetValue(key, out var connections))
        {
            lock (connections)
            {
                return connections.ToArray();
            }
        }
        return [];
    }

    public IEnumerable<T> GetKeys()
    {
        return _connections.Keys;
    }
}
