using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Proxy
{
    public interface IFailedCounter
    {
        void Reset(string accountId);

        void Add(string accountId);

        int Get(string accountId);

        bool CheckAccountIsLocked(string accountId);
    }

    public class FailedCounterProxy : IFailedCounter
    {
        public void Reset(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void Add(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var addCounterResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addCounterResponse.EnsureSuccessStatusCode();
        }

        public int Get(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var lockedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            lockedCountResponse.EnsureSuccessStatusCode();
            var lockedCount = lockedCountResponse.Content.ReadAsAsync<int>().Result;
            return lockedCount;
        }

        public bool CheckAccountIsLocked(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var result = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return result;
        }

    }
}