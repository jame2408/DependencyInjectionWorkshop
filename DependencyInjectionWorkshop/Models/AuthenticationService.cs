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
    public class Sha256Adapter
    {
        public string GetHashedPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
    }

    public class OtpService
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

    public class AuthenticationService
    {
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();

        public bool Verify(string accountId, string password, string otp)
        {
            // if locked, return.
            CheckAccountIsLocked(accountId);

            // Get DB Hash Password Using SP
            var passwordFromDb = GetPasswordFromDb(accountId);

            // Hash Users Key in Password
            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            // Get Otp From Api
            var currentOpt = _otpService.GetCurrentOpt(accountId, new HttpClient() { BaseAddress = new Uri("http://joey.dev/") });

            // 比對 hash password & otp
            if (passwordFromDb == hashedPassword.ToString() && otp == currentOpt)
            {
                // Verify success reset count
                ResetFailedCounter(accountId);

                return true;
            }

            // Verify failed count add 1
            AddFailedCount(accountId);

            // 取得失敗次數，並用 NLog 記錄 log 資訊。
            var failedCount = GetFailedCount(accountId);
            LogFailedCount(accountId, failedCount);

            // 比對失敗用 Slack 通知使用者
            Notify($"accountId:{accountId} verify failed.");

            return false;
        }

        private static void LogFailedCount(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"Verify Failed. AccountId: {accountId}, Failed Times: {failedCount}");
        }

        private static int GetFailedCount(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var lockedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            lockedCountResponse.EnsureSuccessStatusCode();
            var lockedCount = lockedCountResponse.Content.ReadAsAsync<int>().Result;
            return lockedCount;
        }

        private static void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(r => { }, "my channel", message, "my bot name");
        }

        private static void AddFailedCount(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var addCounterResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addCounterResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCounter(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetPasswordFromDb(string accountId)
        {
            var dbPassword = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                dbPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return dbPassword;
        }

        private static void CheckAccountIsLocked(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}