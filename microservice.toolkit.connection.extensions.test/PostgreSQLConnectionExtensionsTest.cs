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

        [Test]
        public void ExecuteReader()
        {
            this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) CONSTRAINT firstkey PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return cmd.ExecuteNonQuery();
            });

            this.dbConnection.ExecuteNonQuery("INSERT INTO films VALUES ('a0001', 'my_title 1');");
            this.dbConnection.ExecuteNonQuery("INSERT INTO films VALUES ('a0002', 'my_title 2');");

            using var reader = this.dbConnection.ExecuteReader("SELECT code, title FROM films WHERE title LIKE 'my_title %' ORDER BY code");

            var results = new System.Collections.Generic.List<(string Code, string Title)>();
            while (reader.Read())
            {
                results.Add((reader.GetString(0), reader.GetString(1)));
            }

            Assert.That(2, Is.EqualTo(results.Count));
            Assert.That("a0001", Is.EqualTo(results[0].Code));
            Assert.That("my_title 1", Is.EqualTo(results[0].Title));
        }

        [Test]
        public async System.Threading.Tasks.Task ExecuteReaderAsync()
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

            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES ('a0001', 'my_title 1');");
            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES ('a0002', 'my_title 2');");

            await using var reader = await this.dbConnection.ExecuteReaderAsync("SELECT code, title FROM films WHERE title LIKE 'my_title %' ORDER BY code");

            var results = new System.Collections.Generic.List<(string Code, string Title)>();
            while (await reader.ReadAsync())
            {
                results.Add((reader.GetString(0), reader.GetString(1)));
            }

            Assert.That(2, Is.EqualTo(results.Count));
            Assert.That("a0001", Is.EqualTo(results[0].Code));
            Assert.That("my_title 1", Is.EqualTo(results[0].Title));
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