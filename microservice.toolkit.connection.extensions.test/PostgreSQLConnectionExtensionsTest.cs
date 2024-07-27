using Npgsql;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.connection.extensions.test
{
    [ExcludeFromCodeCoverage]
    public class PostgreSQLConnectionExtensionsTest
    {
        private NpgsqlConnection dbConnection;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await this.dbConnection.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) CONSTRAINT firstkey PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.That(-1, Is.EqualTo(result));
        }

        [Test]
        public async Task ExecuteNonQueryAsync()
        {
            await this.dbConnection.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) CONSTRAINT firstkey PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.That(1,
                Is.EqualTo(await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES ('mycod', 'my_title');")));
        }

        [SetUp]
        public void SetUp()
        {
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "127.0.0.1";
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
            this.dbConnection =
                new NpgsqlConnection($"Server={host};Port={port};User Id=postgres;Password=postgres;Database=postgres");
        }

        [TearDown]
        public async Task TearDown()
        {
            await this.dbConnection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
            await this.dbConnection.CloseAsync();
        }
    }
}