using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Proxy
{
    public interface IOtp
    {
        string GetCurrentOtp(string accountId);
    }

    public class OtpServiceProxy : IOtp
    {
        public string GetCurrentOtp(string accountId)
        {
            var otpFromApi = "";
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                otpFromApi = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            return otpFromApi;
        }
    }
}