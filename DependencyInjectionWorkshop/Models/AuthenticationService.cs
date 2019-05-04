using Dapper;
using SlackAPI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            // if locked, return.
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }

            // Get DB Hash Password Using SP
            var dbPassword = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                dbPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            // Hash Users Key in Password
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            // Get Otp From Api
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

            // 比對 hash password & otp
            if (dbPassword == hash.ToString() && otp == otpFromApi)
            {
                // Verify success reset count
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
                resetResponse.EnsureSuccessStatusCode();

                return true;
            }

            // Verify failed count add 1
            var addCounterResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addCounterResponse.EnsureSuccessStatusCode();

            // 比對失敗用 Slack 通知使用者
            var message = $"accountId:{accountId} verify failed.";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(r => { }, "my channel", message, "my bot name");

            // 取得失敗次數，並用 NLog 記錄 log 資訊。
            var lockedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            lockedCountResponse.EnsureSuccessStatusCode();
            var lockedCount = lockedCountResponse.Content.ReadAsAsync<int>().Result;

            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"Verify Failed. AccountId: {accountId}, Failed Times: {lockedCount}");

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}