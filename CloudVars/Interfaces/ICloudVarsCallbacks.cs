using System;
using System.Threading.Tasks;

namespace CloudVar.Interfaces
{
    public interface ICloudVarsCallbacks
    {
        void RegisterCallback(string name, Func<object, Task> callback);

        Task ExecuteCallbacks(string name, object value);

        void RemoveCallback(string name, Func<object, Task> callback);

        void RemoveAllCallbacks(string name);
    }
}
