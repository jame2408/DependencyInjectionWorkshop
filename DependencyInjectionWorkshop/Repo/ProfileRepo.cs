using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using Dapper;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Repo
{
    public interface IProfile
    {
        string GetPassword(string accountId);
    }

    public class ProfileRepo : IProfile
    {
        public string GetPassword(string accountId)
        {
            var dbPassword = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                dbPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return dbPassword;
        }
    }
}