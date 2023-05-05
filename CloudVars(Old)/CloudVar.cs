using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Transactions;


namespace CloudVar
{
    public class CloudVars
    {
        private static readonly Lazy<CloudVars> _lazy = new Lazy<CloudVars>(() => new CloudVars());
        private readonly ConcurrentDictionary<string, object> _values = new ConcurrentDictionary<string, object>();
        private readonly Dictionary<string, List<Func<object, Task>>> _callbacks = new Dictionary<string, List<Func<object, Task>>>();
        private readonly ConcurrentDictionary<string, DateTime> _expirationTimes = new ConcurrentDictionary<string, DateTime>();


        private int _updateRate = 0;

        protected CloudVars() { }

        public static CloudVars Instance => _lazy.Value;

        public virtual void add(string name, object value, TimeSpan? expiration = null)
        {
            using (var scope = new TransactionScope())
            {
                if (!_values.TryAdd(name, value))
                {
                    throw new InvalidOperationException("A value with the specified name already exists.");
                }

                if (expiration.HasValue)
                {
                    _expirationTimes[name] = DateTime.UtcNow + expiration.Value;
                }

                scope.Complete();
            }
        }

        public virtual async Task setAsync(string name, object value, TimeSpan? expiration = null)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (!_values.ContainsKey(name))
                {
                    throw new InvalidOperationException("A value with the specified name does not exist.");
                }

                _values[name] = value;

                if (expiration.HasValue)
                {
                    _expirationTimes[name] = DateTime.UtcNow + expiration.Value;
                }

                if (_callbacks.TryGetValue(name, out var callbacks))
                {
                    // Execute callbacks in a serialized manner using a semaphore
                    var semaphore = new SemaphoreSlim(1, 1);
                    foreach (var callback in callbacks)
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await callback(value);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                }

                scope.Complete();
            }
        }

        public virtual async Task setAsyncConcurrent(string name, object value, TimeSpan? expiration = null)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (!_values.ContainsKey(name))
                {
                    throw new InvalidOperationException("A value with the specified name does not exist.");
                }

                _values[name] = value;

                if (expiration.HasValue)
                {
                    _expirationTimes[name] = DateTime.UtcNow + expiration.Value;
                }

                if (_callbacks.TryGetValue(name, out var callbacks))
                {
                    // Execute callbacks concurrently using a ConcurrentQueue
                    var queue = new ConcurrentQueue<Func<object, Task>>(callbacks);
                    var tasks = new List<Task>();
                    while (queue.TryDequeue(out var callback))
                    {
                        tasks.Add(callback(value));
                    }
                    await Task.WhenAll(tasks);
                }

                scope.Complete();
            }
        }


        public virtual T get<T>(string name)
        {
            return (T)_values[name];
        }

        // Type inference version of Get method
        public virtual T get<T>(string name, T defaultValue = default)
        {
            if (_values.TryGetValue(name, out object value))
            {
                return (T)value;
            }
            else
            {
                return defaultValue;
            }
        }

        public virtual void onChange(string name, Action<object> callback)
        {
            // Convert the Action callback to a Func callback that returns a completed Task
            onChange(name, (value) =>
            {
                callback(value);
                return Task.CompletedTask;
            });
        }

        public virtual void onChange(string name, Func<object, Task> callback)
        {
            if (!_callbacks.TryGetValue(name, out var callbacks))
            {
                callbacks = new List<Func<object, Task>>();
                _callbacks[name] = callbacks;
            }

            lock (callbacks) // Add a lock to ensure thread-safety
            {
                callbacks.Add(callback);
            }
        }

        public virtual void remove(string name)
        {
            if (!_values.TryRemove(name, out _))
            {
                throw new InvalidOperationException("A value with the specified name does not exist.");
            }
        }

        public virtual bool contains(string name)
        {
            return _values.ContainsKey(name);
        }

        // Returns a list of all the names of the keys in the store
        public virtual List<string> getAllNames()
        {
            return _values.Keys.ToList();
        }

        // Clears all keys and values from the store
        public virtual void clear()
        {
            _values.Clear();
            _callbacks.Clear();
        }

        // Removes the specified callback for the specified key
        public virtual void removeCallback(string name, Func<object, Task> callback)
        {
            if (_callbacks.TryGetValue(name, out var callbacks))
            {
                lock (callbacks) // Add a lock to ensure thread-safety
                {
                    callbacks.Remove(callback);
                }
            }
        }

        // Removes all callbacks for the specified key
        public virtual void removeAllCallbacks(string name)
        {
            if (_callbacks.TryGetValue(name, out var callbacks))
            {
                lock (callbacks) // Add a lock to ensure thread-safety
                {
                    callbacks.Clear();
                }
            }
        }

        public virtual void addRange(IDictionary<string, object> values)
        {
            foreach (var kvp in values)
            {
                add(kvp.Key, kvp.Value);
            }
        }

        public virtual async Task setRangeAsync(IDictionary<string, object> values)
        {
            foreach (var kvp in values)
            {
                await setAsync(kvp.Key, kvp.Value);
            }
        }

        public virtual async Task setRangeAsyncConcurrent(IDictionary<string, object> values)
        {
            foreach (var kvp in values)
            {
                await setAsyncConcurrent(kvp.Key, kvp.Value);
            }
        }

        public virtual async Task setAsyncHybrid(string name, object value, TimeSpan? expiration = null, int callbackCountThreshold = 10, int updateRateThreshold = 100)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (!_values.ContainsKey(name))
                {
                    throw new InvalidOperationException("A value with the specified name does not exist.");
                }

                _values[name] = value;

                if (expiration.HasValue)
                {
                    _expirationTimes[name] = DateTime.UtcNow + expiration.Value;
                }

                if (_callbacks.TryGetValue(name, out var callbacks))
                {
                    // Determine the most efficient way to execute callbacks based on the current conditions
                    if (callbacks.Count < callbackCountThreshold && _updateRate < updateRateThreshold)
                    {
                        // Execute callbacks in a serialized manner using a semaphore
                        var semaphore = new SemaphoreSlim(1, 1);
                        foreach (var callback in callbacks)
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                await callback(value);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }
                    }
                    else
                    {
                        // Execute callbacks concurrently using a ConcurrentQueue
                        var queue = new ConcurrentQueue<Func<object, Task>>(callbacks);
                        var tasks = new List<Task>();
                        while (queue.TryDequeue(out var callback))
                        {
                            tasks.Add(callback(value));
                        }
                        await Task.WhenAll(tasks);
                    }
                }

                scope.Complete();
            }
        }

    }
}