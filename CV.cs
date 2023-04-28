namespace CloudVar
{
    public static class CV
    {
        private static readonly CloudVars _cloudVars = CloudVars.Instance;

        /// <summary>
        /// Adds a new key-value pair to the store with the specified name and value.
        /// If a key with the same name already exists, an exception is thrown.
        /// </summary>
        /// <param name="name">The name of the key to add.</param>
        /// <param name="value">The value to add to the store.</param>
        /// <param name="expiration">The expiration time for the key (optional).</param>
        public static void Add(string name, object value, TimeSpan? expiration = null) => _cloudVars.add(name, value, expiration);

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
        /// <param name="expiration">The new expiration time for the key (optional).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SetAsync(string name, object value, TimeSpan? expiration = null) => await CloudVars.Instance.setAsync(name, value, expiration);

        /// <summary>
        /// Updates the value of an existing key in the store with the specified name.
        /// If no key with the specified name exists, an exception is thrown.
        /// Invokes all registered callbacks for the key concurrently.
        /// </summary>
        /// <param name="name">The name of the key to update.</param>
        /// <param name="value">The new value to set for the key.</param>
        /// <param name="expiration">The new expiration time for the key (optional).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SetAsyncConcurrent(string name, object value, TimeSpan? expiration = null) => await CloudVars.Instance.setAsyncConcurrent(name, value, expiration);

        /// <summary>
        /// Updates the values of multiple existing keys in the store.
        /// If no key with the specified name exists, an exception is thrown.
        /// Invokes all registered callbacks for each key in a serialized manner.
        /// </summary>
        /// <param name="values">The key-value pairs to update in the store.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SetRangeAsync(IDictionary<string, object> values) => await _cloudVars.setRangeAsync(values);

        /// <summary>
        /// Updates the values of multiple existing keys in the store.
        /// If no key with the specified name exists, an exception is thrown.
        /// Invokes all registered callbacks for each key concurrently.
        /// </summary>
        /// <param name="values">The key-value pairs to update in the store.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SetRangeAsyncConcurrent(IDictionary<string, object> values) => await _cloudVars.setRangeAsyncConcurrent(values);

    }
}