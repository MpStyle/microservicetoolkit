using NUnit.Framework;

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace microservice.toolkit.connection.extensions.test
{
    [ExcludeFromCodeCoverage]
    public class SqlServerExecuteStoredProcedureTest
    {
        private SqlConnection dbConnection;

        [Test]
        public async Task ExecuteStoredProcedureAsync()
        {
            var itemId = Guid.NewGuid().ToString();
            await this.dbConnection.ExecuteStoredProcedureAsync("mt_cm_test_ItemUpsert",
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
                cmd.CommandText = "SELECT * FROM mt_cm_test_Item";
                using var reader = cmd.ExecuteReader();
                var objects = new List<string>();

                while (reader.Read())
                {
                    objects.Add(reader.GetString(0));
                }

                return objects;
            });

            Assert.That(itemId, Is.EqualTo(itemIds[0]));
        }

        [Test]
        public async Task ExecuteStoredProcedureAsync_NullValue()
        {
            var itemPropertyId = Guid.NewGuid().ToString();
            await this.dbConnection.ExecuteStoredProcedureAsync("mt_cm_test_ItemPropertyUpsert",
                new Dictionary<string, object>
                {
                    {"@Id", itemPropertyId},
                    {"@ItemId", Guid.NewGuid().ToString()},
                    {"@Key", Guid.NewGuid().ToString()},
                    {"@StringValue", null},
                    {"@IntValue", 1},
                    {"@LongValue", null},
                    {"@FloatValue", null},
                    {"@BoolValue", null},
                    {"@Order", null},
                });

            var itemPropertyIds = this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText = "SELECT * FROM mt_cm_test_ItemProperty";
                using var reader = cmd.ExecuteReader();
                var objects = new List<string>();

                while (reader.Read())
                {
                    objects.Add(reader.GetString(0));
                }

                return objects;
            });

            Assert.That(itemPropertyId, Is.EqualTo(itemPropertyIds[0]));
        }

        [Test]
        public void ExecuteStoredProcedure_NullValue()
        {
            var itemPropertyId = Guid.NewGuid().ToString();
            this.dbConnection.ExecuteStoredProcedure("mt_cm_test_ItemPropertyUpsert",
                new Dictionary<string, object>
                {
                    {"@Id", itemPropertyId},
                    {"@ItemId", Guid.NewGuid().ToString()},
                    {"@Key", Guid.NewGuid().ToString()},
                    {"@StringValue", null},
                    {"@IntValue", 1},
                    {"@LongValue", null},
                    {"@FloatValue", null},
                    {"@BoolValue", null},
                    {"@Order", null},
                });

            var itemPropertyIds = this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText = "SELECT * FROM mt_cm_test_ItemProperty";
                using var reader = cmd.ExecuteReader();
                var objects = new List<string>();

                while (reader.Read())
                {
                    objects.Add(reader.GetString(0));
                }

                return objects;
            });

            Assert.That(itemPropertyId, Is.EqualTo(itemPropertyIds[0]));
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

            var setupFiles = new[]
            {
                "CreateItemTable.sql",
                "CreateItemUpsertProcedure.sql",
                "CreateItemPropertyTable.sql",
                "CreateItemPropertyUpsertProcedure.sql"
            };

            foreach (var setupFile in setupFiles)
            {
                this.dbConnection.Execute(cmd =>
                {
                    cmd.CommandText =
                        File.ReadAllText(Path.Combine(currentAssemblyLocation, "data", setupFile));
                    return cmd.ExecuteNonQuery();
                });
            }
        }

        [TearDown]
        public void TearDown()
        {
            this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText = @"
                DROP TABLE mt_cm_test_Item; 
                DROP PROCEDURE mt_cm_test_ItemUpsert;
                DROP TABLE mt_cm_test_ItemProperty; 
                DROP PROCEDURE mt_cm_test_ItemPropertyUpsert;
                ";
                return cmd.ExecuteNonQuery();
            });
        }
    }
}