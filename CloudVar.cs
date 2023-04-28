using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudVar
{
    internal class CloudVars
    {
        private static readonly Lazy<CloudVars> _lazy = new Lazy<CloudVars>(() => new CloudVars());
        private readonly ConcurrentDictionary<string, object> _values = new ConcurrentDictionary<string, object>();
        private readonly Dictionary<string, List<Func<object, Task>>> _callbacks = new Dictionary<string, List<Func<object, Task>>>();

        private CloudVars() { }

        internal static CloudVars Instance => _lazy.Value;

        internal void add(string name, object value)
        {
            if (!_values.TryAdd(name, value))
            {
                throw new InvalidOperationException("A value with the specified name already exists.");
            }
        }

        internal async Task setAsync(string name, object value)
        {
            if (!_values.ContainsKey(name))
            {
                throw new InvalidOperationException("A value with the specified name does not exist.");
            }

            _values[name] = value;

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
        }

        internal T get<T>(string name)
        {
            return (T)_values[name];
        }

        // Type inference version of Get method
        internal T get<T>(string name, T defaultValue = default)
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

        internal void onChange(string name, Action<object> callback)
        {
            // Convert the Action callback to a Func callback that returns a completed Task
            onChange(name, (value) =>
            {
                callback(value);
                return Task.CompletedTask;
            });
        }

        internal void onChange(string name, Func<object, Task> callback)
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

        internal void remove(string name)
        {
            if (!_values.TryRemove(name, out _))
            {
                throw new InvalidOperationException("A value with the specified name does not exist.");
            }
        }

        internal bool contains(string name)
        {
            return _values.ContainsKey(name);
        }

        // Returns a list of all the names of the keys in the store
        internal List<string> getAllNames()
        {
            return _values.Keys.ToList();
        }

        // Clears all keys and values from the store
        internal void clear()
        {
            _values.Clear();
            _callbacks.Clear();
        }

        // Removes the specified callback for the specified key
        internal void removeCallback(string name, Func<object, Task> callback)
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
        internal void removeAllCallbacks(string name)
        {
            if (_callbacks.TryGetValue(name, out var callbacks))
            {
                lock (callbacks) // Add a lock to ensure thread-safety
                {
                    callbacks.Clear();
                }
            }
        }

        internal void addRange(IDictionary<string, object> values)
        {
            foreach (var kvp in values)
            {
                add(kvp.Key, kvp.Value);
            }
        }

        internal async Task setRangeAsync(IDictionary<string, object> values)
        {
            foreach (var kvp in values)
            {
                await setAsync(kvp.Key, kvp.Value);
            }
        }

    }



    public static class CV
    {
        private static readonly CloudVars _cloudVars = CloudVars.Instance;

        /// <summary>
        /// Adds a new key-value pair to the store with the specified name and value.
        /// If a key with the same name already exists, an exception is thrown.
        /// </summary>
        /// <param name="name">The name of the key to add.</param>
        /// <param name="value">The value to add to the store.</param>
        public static void Add(string name, object value) => _cloudVars.add(name, value);

        /// <summary>
        /// Adds multiple new key-value pairs to the store.
        /// If a key with the same name already exists, an exception is thrown.
        /// </summary>
        /// <param name="values">The key-value pairs to add to the store.</param>
        public static void AddRange(IDictionary<string, object> values) => _cloudVars.addRange(values);

        /// <summary>
        /// Clears all keys and values from the store.
        /// </summary>
        public static void Clear() => _cloudVars.clear();

        /// <summary>
        /// Checks whether a key with the specified name exists in the store.
        /// </summary>
        /// <param name="name">The name of the key to check.</param>
        /// <returns><see langword="true"/> if the key exists in the store; otherwise, <see langword="false"/>.</returns>
        public static bool Contains(string name) => _cloudVars.contains(name);

        /// <summary>
        /// Gets a list of all the names of the keys in the store.
        /// </summary>
        /// <returns>A list of all the names of the keys in the store.</returns>
        public static List<string> GetAllNames() => _cloudVars.getAllNames();

        /// <summary>
        /// Gets the value of the key with the specified name from the store.
        /// If no key with the specified name exists, an exception is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="name">The name of the key to get the value of.</param>
        /// <returns>The value of the key.</returns>
        public static T Get<T>(string name) => _cloudVars.get<T>(name);

        /// <summary>
        /// Gets the value of the key with the specified name from the store.
        /// If no key with the specified name exists, returns the default value for the type.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="name">The name of the key to get the value of.</param>
        /// <param name="defaultValue">The default value to return if the key is not found (optional).</param>
        /// <returns>The value of the key or the default value if the key is not found.</returns>
        public static T Get<T>(string name, T defaultValue) => _cloudVars.get(name, defaultValue);

        /// <summary>
        /// Registers a synchronous callback to be invoked when the value of the key with the specified name changes.
        ///</summary>
        ///<param name="name">The name of the key to register for callback.</param> 
        ///<param name="callback">The callback to invoke when there is a change in value for that specific key.</param> 
        public static void OnChange(string name, Action<object> callback) => _cloudVars.onChange(name, callback);

        ///<summary> 
        ///<para>Registers an asynchronous callback to be invoked when there is a change in value for that specific key. </para> 
        ///</summary> 
        ///<param name="name">The name of that specific key. </param> 
        ///<param name="callback">The callback to invoke when there is a change in value for that specific key. </param> 
        public static void OnChange(string name, Func<object, Task> callback) => _cloudVars.onChange(name, callback);

        ///<summary> 
        ///<para>Removes all callbacks for that specific key. </para> 
        ///</summary> 
        ///<param name="name">The specific key. </param> 
        public static void RemoveAllCallbacks(string name) => _cloudVars.removeAllCallbacks(name);

        ///<summary> 
        ///<para>Removes that specific callback for that specific key. </para> 
        ///</summary> 
        ///<param name="name">The specific key. </param> 
        ///<param name="callback">The specific callback. </param> 
        public static void RemoveCallback(string name, Func<object, Task> callback) => _cloudVars.removeCallback(name, callback);

        /// <summary>
        /// Updates the value of an existing key in the store with the specified name.
        /// If no key with the specified name exists, an exception is thrown.
        /// Invokes all registered callbacks for the key in a serialized manner.
        /// </summary>
        /// <param name="name">The name of the key to update.</param>
        /// <param name="value">The new value to set for the key.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SetAsync(string name, object value) => await CloudVars.Instance.setAsync(name, value);
    
        /// <summary>
        /// Updates the values of multiple existing keys in the store.
        /// If no key with the specified name exists, an exception is thrown.
        /// Invokes all registered callbacks for each key in a serialized manner.
        /// </summary>
        /// <param name="values">The key-value pairs to update in the store.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SetRangeAsync(IDictionary<string, object> values) => await _cloudVars.setRangeAsync(values);
    }
}
