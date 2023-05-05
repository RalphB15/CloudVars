using System;
using System.Collections.Concurrent;
using CloudVar.Interfaces;

namespace CloudVar.Implementations
{
    public class CloudVarsExpiration : ICloudVarsExpiration
    {
        private readonly ConcurrentDictionary<string, DateTime> _expirationTimes = new ConcurrentDictionary<string, DateTime>();

        public void SetExpiration(string name, TimeSpan expiration)
        {
            _expirationTimes[name] = DateTime.UtcNow + expiration;
        }

        public bool IsExpired(string name)
        {
            if (_expirationTimes.TryGetValue(name, out DateTime expirationTime))
            {
                return DateTime.UtcNow > expirationTime;
            }
            return false;
        }
    }
}
