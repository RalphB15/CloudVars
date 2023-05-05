using System.Collections.Generic;

namespace CloudVar.Interfaces
{
    public interface ICloudVarsStore
    {
        void Add(string name, object value);

        void Set(string name, object value);

        T Get<T>(string name);

        T Get<T>(string name, T defaultValue);

        void Remove(string name);

        bool Contains(string name);

        List<string> GetAllNames();

        void Clear();

        Task SaveToFileAsync(string filePath);

        Task LoadFromFileAsync(string filePath);
    }
}
