using System.Collections.Concurrent;

public class CloudVars
{
    private static readonly Lazy<CloudVars> _lazy = new Lazy<CloudVars>(() => new CloudVars());
    public static CloudVars Instance => _lazy.Value;

    private readonly ConcurrentDictionary<string, object> _values = new ConcurrentDictionary<string, object>();
    private readonly Dictionary<string, Action<object>> _callbacks = new Dictionary<string, Action<object>>();

    public void Add(string name, object value)
    {
        if (!_values.TryAdd(name, value))
        {
            throw new InvalidOperationException("A value with the specified name already exists.");
        }
    }

    public void Set(string name, object value)
    {
        if (!_values.ContainsKey(name))
        {
            throw new InvalidOperationException("A value with the specified name does not exist.");
        }
        _values[name] = value;
        if (_callbacks.TryGetValue(name, out var callback))
        {
            callback(value);
        }
    }

    public T Get<T>(string name)
    {
        return (T)_values[name];
    }

    public void OnChange(string name, Action<object> callback)
    {
        _callbacks[name] = callback;
    }
}

public static class CloudVarsShorthand
{
    public static CloudVars CV => CloudVars.Instance;
}