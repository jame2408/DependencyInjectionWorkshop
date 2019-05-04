using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Proxy
{
    public class FailedCounterProxy
    {
        public void ResetFailedCounter(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCount(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var addCounterResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addCounterResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedCount(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var lockedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            lockedCountResponse.EnsureSuccessStatusCode();
            var lockedCount = lockedCountResponse.Content.ReadAsAsync<int>().Result;
            return lockedCount;
        }
    }
}