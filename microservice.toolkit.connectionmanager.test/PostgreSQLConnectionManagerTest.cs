using Npgsql;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager.test
{
    [ExcludeFromCodeCoverage]
    public class PostgreSQLConnectionManagerTest
    {
        private NpgsqlConnection connectionManager;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await connectionManager.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) CONSTRAINT firstkey PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(-1, result);
        }

        [Test]
        public async Task ExecuteNonQueryAsync()
        {
            await connectionManager.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) CONSTRAINT firstkey PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(1,
                await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES ('mycod', 'my_title');"));
        }

        [SetUp]
        public void SetUp()
        {
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "127.0.0.1";
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
            this.connectionManager =
                new NpgsqlConnection($"Server={host};Port={port};User Id=postgres;Password=postgres;Database=postgres");
        }

        [TearDown]
        public async Task TearDown()
        {
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
            await connectionManager.CloseAsync();
        }
    }
}