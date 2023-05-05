using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using CloudVar.Interfaces;
using CloudVar.Implementations;

namespace CloudVar
{
    public class CloudVars 
    {
        private static readonly Lazy<CloudVars> _lazy = new Lazy<CloudVars>(() => new CloudVars());
        private readonly ICloudVarsStore _store;
        private readonly ICloudVarsCallbacks _callbacks;
        private readonly ICloudVarsExpiration _expiration;

        protected CloudVars()
        {
            _store = new CloudVarsStore();
            _callbacks = new CloudVarsCallbacks();
            _expiration = new CloudVarsExpiration();
        }

        public static CloudVars Instance => _lazy.Value;

        public void Add(string name, object value, TimeSpan? expiration = null)
        {
            _store.Add(name, value);

            if (expiration.HasValue)
            {
                _expiration.SetExpiration(name, expiration.Value);
            }
        }

        public async Task SetAsync(string name, object value, TimeSpan? expiration = null)
        {
            if (!_store.Contains(name))
            {
                throw new InvalidOperationException("A value with the specified name does not exist.");
            }

            _store.Set(name, value);

            if (expiration.HasValue)
            {
                _expiration.SetExpiration(name, expiration.Value);
            }

            await _callbacks.ExecuteCallbacks(name, value);
        }

        public T Get<T>(string name)
        {
            if (_expiration.IsExpired(name))
            {
                throw new InvalidOperationException("The specified value has expired.");
            }

            return _store.Get<T>(name);
        }

        public T Get<T>(string name, T defaultValue)
        {
            if (_expiration.IsExpired(name))
            {
                return defaultValue;
            }

            return _store.Get(name, defaultValue);
        }

        public void Remove(string name)
        {
            _store.Remove(name);
        }

        public void RegisterCallback(string name, Func<object, Task> callback)
        {
            _callbacks.RegisterCallback(name, callback);
        }

        public void RemoveCallback(string name, Func<object, Task> callback)
        {
            _callbacks.RemoveCallback(name, callback);
        }

        public void RemoveAllCallbacks(string name)
        {
            _callbacks.RemoveAllCallbacks(name);
        }

        public async Task SaveToFileAsync(string filePath)
        {
            await _store.SaveToFileAsync(filePath);
        }

        public async Task LoadFromFileAsync(string filePath)
        {
            await _store.LoadFromFileAsync(filePath);
        }

    }

}