using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;
using CloudVar.Interfaces;

namespace CloudVar.Implementations
{
    public class CloudVarsStore : ICloudVarsStore
    {
        private readonly ConcurrentDictionary<string, object> _values = new ConcurrentDictionary<string, object>();

        public void Add(string name, object value)
        {
            if (!_values.TryAdd(name, value))
            {
                throw new InvalidOperationException("A value with the specified name already exists.");
            }
        }

        public void Set(string name, object value)
        {
            _values[name] = value;
        }

        public T Get<T>(string name)
        {
            return (T)_values[name];
        }

        public T Get<T>(string name, T defaultValue)
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

        public void Remove(string name)
        {
            if (!_values.TryRemove(name, out _))
            {
                throw new InvalidOperationException("A value with the specified name does not exist.");
            }
        }

        public bool Contains(string name)
        {
            return _values.ContainsKey(name);
        }

        public List<string> GetAllNames()
        {
            return _values.Keys.ToList();
        }

        public void Clear()
        {
            _values.Clear();
        }

        public async Task SaveToFileAsync(string filePath)
        {
            var dataToSave = _values.ToDictionary(entry => entry.Key, entry => entry.Value);
            using StreamWriter file = new StreamWriter(filePath);
            string json = JsonConvert.SerializeObject(dataToSave, Newtonsoft.Json.Formatting.Indented);
            await file.WriteAsync(json);
        }

        public async Task LoadFromFileAsync(string filePath)
        {
            using StreamReader file = new StreamReader(filePath);
            string json = await file.ReadToEndAsync();
            var loadedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            foreach (var entry in loadedData)
            {
                _values[entry.Key] = entry.Value;
            }
        }
    }
}
