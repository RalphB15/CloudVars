using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudVar.Interfaces;

namespace CloudVar.Implementations
{
    public class CloudVarsCallbacks : ICloudVarsCallbacks
    {
        private readonly Dictionary<string, List<Func<object, Task>>> _callbacks = new Dictionary<string, List<Func<object, Task>>>();

        public void RegisterCallback(string name, Func<object, Task> callback)
        {
            if (!_callbacks.TryGetValue(name, out var callbacks))
            {
                callbacks = new List<Func<object, Task>>();
                _callbacks[name] = callbacks;
            }

            lock (callbacks)
            {
                callbacks.Add(callback);
            }
        }

        public async Task ExecuteCallbacks(string name, object value)
        {
            if (_callbacks.TryGetValue(name, out var callbacks))
            {
                var tasks = new List<Task>();
                lock (callbacks)
                {
                    foreach (var callback in callbacks)
                    {
                        tasks.Add(callback(value));
                    }
                }
                await Task.WhenAll(tasks);
            }
        }

        public void RemoveCallback(string name, Func<object, Task> callback)
        {
            if (_callbacks.TryGetValue(name, out var callbacks))
            {
                lock (callbacks)
                {
                    callbacks.Remove(callback);
                }
            }
        }

        public void RemoveAllCallbacks(string name)
        {
            if (_callbacks.TryGetValue(name, out var callbacks))
            {
                lock (callbacks)
                {
                    callbacks.Clear();
                }
            }
        }
    }
}
