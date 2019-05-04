using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using Dapper;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Repo
{
    public class ProfileRepo
    {
        public string GetPasswordFromDb(string accountId)
        {
            var dbPassword = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                dbPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return dbPassword;
        }

        public void CheckAccountIsLocked(string accountId)
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
}