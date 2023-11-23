using microservice.toolkit.orm;

using MySqlConnector;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager.test
{
    [ExcludeFromCodeCoverage]
    public class MySqlConnectionManagerTest
    {
        private MySqlConnection connectionManager;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await this.connectionManager.ExecuteAsync(async command =>
            {
                command.CommandText = "SELECT * FROM films";

                var objects = new List<Film>();

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    objects.Add(new Film { Code = reader.GetInt16(0), Title = reader.GetString(1), });
                }

                return objects;
            });

            Assert.AreEqual(4, result.Count);

            for (var i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(i + 1, result[i].Code);
                Assert.AreEqual($"my_title {i + 1}", result[i].Title);
            }
        }

        [Test]
        public async Task ExecuteAsync_Query()
        {
            var result = await this.connectionManager.ExecuteAsync(
                "SELECT * FROM films WHERE code < @code",
                reader => new { Code = reader.GetInt16(0), Title = reader.GetString(1), },
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.AreEqual(2, result.Length);

            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(i + 1, result[i].Code);
                Assert.AreEqual($"my_title {i + 1}", result[i].Title);
            }
        }

        [Test]
        public async Task ExecuteAsync_Query_AutoMapping()
        {
            var result = await this.connectionManager.ExecuteAsync<Film>(
                "SELECT code, title FROM films WHERE code < @code",
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.AreEqual(2, result.Length);

            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(i + 1, result[i].Code);
                Assert.AreEqual($"my_title {i + 1}", result[i].Title);
            }
        }

        [Test]
        public async Task ExecuteFirstAsync_Query()
        {
            var result = await this.connectionManager.ExecuteFirstAsync(
                "SELECT * FROM films WHERE code < @code",
                reader => new { Code = reader.GetInt16(0), Title = reader.GetString(1), },
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.AreEqual(1, result.Code);
            Assert.AreEqual("my_title 1", result.Title);
        }

        [Test]
        public void ExecuteFirst_Query()
        {
            var result = this.connectionManager.ExecuteFirst(
                "SELECT * FROM films WHERE code < @code",
                reader => new { Code = reader.GetInt16(0), Title = reader.GetString(1), },
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.AreEqual(1, result.Code);
            Assert.AreEqual("my_title 1", result.Title);
        }

        [Test]
        public async Task ExecuteNonQueryAsync()
        {
            Assert.AreEqual(1,
                await connectionManager.ExecuteNonQueryAsync(
                    "INSERT INTO films VALUES (@code, @title, @genre);",
                    new Dictionary<string, object>
                    {
                        {"@code", 5}, {"@title", "my_title 5"}, {"@genre", FilmGenre.Action},
                    }));
        }

        [SetUp]
        public async Task SetUp()
        {
            var host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "127.0.0.1";
            var rootPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? "root";
            var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "microservice_framework_tests";

            this.connectionManager =
                new MySqlConnection($"Server={host};User ID=root;Password={rootPassword};database={database};SSLMode=Required");

            // Creates table
            await this.connectionManager.ExecuteNonQueryAsync(
                "CREATE TABLE films ( code int PRIMARY KEY, title varchar(40) NOT NULL, genres varchar(50) NOT NULL);");

            // Inserts some rows in table
            await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES (1, 'my_title 1', '1,2');");
            await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES (2, 'my_title 2', '2');");
            await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES (3, 'my_title 3', '1');");
            await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES (4, 'my_title 4', '3');");
        }

        [TearDown]
        public async Task TearDown()
        {
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
            await connectionManager.CloseAsync();
        }

        enum FilmGenre
        {
            Doc,
            Action,
            Comedy
        }

        private record Film
        {
            [MitoColumn("code")]
            public int Code { get; init; }
            
            [MitoColumn("code")]
            public string Title { get; init; }
            
            [MitoColumn("genres")]
            public FilmGenre[] Genres { get; init; }
        }
    }
}