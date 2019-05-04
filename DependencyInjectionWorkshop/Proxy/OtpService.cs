﻿using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Proxy
{
    public class OtpServiceProxy
    {
        public string GetCurrentOpt(string accountId, HttpClient httpClient)
        {
            var otpFromApi = "";
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