using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager.test
{
    [ExcludeFromCodeCoverage]
    public class SQLServerExecuteStoredProcedureTest
    {
        private SqlConnection dbConnection;

        [Test]
        public async Task ExecuteStoredProcedureAsync()
        {
            var itemId = Guid.NewGuid().ToString();
            await this.dbConnection.ExecuteStoredProcedureAsync("ItemUpsert",
                new Dictionary<string, object>
                {
                    {"@Id", itemId},
                    {"@Type", Guid.NewGuid().ToString()},
                    {"@Inserted", DateTimeOffset.UtcNow.ToUnixTimeSeconds()},
                    {"@Updated", DateTimeOffset.UtcNow.ToUnixTimeSeconds()},
                    {"@Enabled", true},
                });

            var itemIds = this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText = "SELECT * FROM Item";
                using var reader = cmd.ExecuteReader();
                var objects = new List<string>();

                while (reader.Read())
                {
                    objects.Add(reader.GetString(0));
                }

                return objects;
            });

            Assert.AreEqual(itemId, itemIds[0]);
        }

        [SetUp]
        public void SetUp()
        {
            var host = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "127.0.0.1";
            var port = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
            var rootPassword = Environment.GetEnvironmentVariable("SQLSERVER_ROOT_PASSWORD") ?? "my_root_password123";
            this.dbConnection =
                new SqlConnection($"Server={host},{port};Database=Master;User Id=SA;Password={rootPassword}");

            var currentAssemblyLocation =
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText =
                    File.ReadAllText(Path.Combine(currentAssemblyLocation, "data", "CreateItemTable.sql"));
                return cmd.ExecuteNonQuery();
            });

            this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText =
                    File.ReadAllText(Path.Combine(currentAssemblyLocation, "data", "CreateItemUpsertProcedure.sql"));
                return cmd.ExecuteNonQuery();
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText = "DROP TABLE Item; DROP PROCEDURE ItemUpsert;";
                return cmd.ExecuteNonQuery();
            });
        }
    }
}