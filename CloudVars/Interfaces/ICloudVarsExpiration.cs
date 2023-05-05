using System;

namespace CloudVar.Interfaces
{
    public interface ICloudVarsExpiration
    {
        void SetExpiration(string name, TimeSpan expiration);

        bool IsExpired(string name);
    }
}
