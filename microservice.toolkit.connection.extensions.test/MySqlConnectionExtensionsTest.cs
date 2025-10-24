using MySqlConnector;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace microservice.toolkit.connection.extensions.test
{
    [ExcludeFromCodeCoverage]
    public class MySqlConnectionExtensionsTest
    {
        private MySqlConnection dbConnection;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await this.dbConnection.ExecuteAsync(async command =>
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

            Assert.That(4, Is.EqualTo(result.Count));

            for (var i = 0; i < result.Count; i++)
            {
                Assert.That(i + 1, Is.EqualTo(result[i].Code));
                Assert.That($"my_title {i + 1}", Is.EqualTo(result[i].Title));
            }
        }

        [Test]
        public async Task ExecuteAsync_Query()
        {
            var result = await this.dbConnection.ExecuteAsync(
                "SELECT * FROM films WHERE code < @code",
                reader => new { Code = reader.GetInt16(0), Title = reader.GetString(1), },
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.That(2, Is.EqualTo(result.Length));

            for (var i = 0; i < result.Length; i++)
            {
                Assert.That(i + 1, Is.EqualTo(result[i].Code));
                Assert.That($"my_title {i + 1}", Is.EqualTo(result[i].Title));
            }
        }

        [Test]
        public async Task ExecuteAsync_Query_AutoMapping_Transformations()
        {
            var result = await this.dbConnection.ExecuteAsync(
                "SELECT * FROM films WHERE code < @code",
                DbConnectionExtension.MapperFunc<Film>(new Dictionary<string, Func<object, object>>
                {
                    {
                        nameof(Film.Genres),
                        obj => (obj as string).Split(",").Select(p => (FilmGenre)int.Parse(p)).ToArray()
                    }
                }),
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.That(2, Is.EqualTo(result.Length));

            for (var i = 0; i < result.Length; i++)
            {
                Assert.That(i + 1, Is.EqualTo(result[i].Code));
                Assert.That($"my_title {i + 1}", Is.EqualTo(result[i].Title));
            }
        }

        [Test]
        public async Task ExecuteAsync_Query_AutoMapping()
        {
            var result = await this.dbConnection.ExecuteAsync<Film>(
                "SELECT code, title FROM films WHERE code < @code",
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.That(2, Is.EqualTo(result.Length));

            for (var i = 0; i < result.Length; i++)
            {
                Assert.That(i + 1, Is.EqualTo(result[i].Code));
                Assert.That($"my_title {i + 1}", Is.EqualTo(result[i].Title));
            }
        }

        [Test]
        public async Task ExecuteFirstAsync_Query()
        {
            var result = await this.dbConnection.ExecuteFirstAsync(
                "SELECT * FROM films WHERE code < @code",
                reader => new { Code = reader.GetInt16(0), Title = reader.GetString(1), },
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.That(1, Is.EqualTo(result.Code));
            Assert.That("my_title 1", Is.EqualTo(result.Title));
        }

        [Test]
        public async Task ExecuteFirstAsync_Query_AutoMapping()
        {
            var result = await this.dbConnection.ExecuteFirstAsync(
                "SELECT * FROM films WHERE code < @code",
                DbConnectionExtension.MapperFunc<Film>(new Dictionary<string, Func<object, object>>
                {
                    {
                        nameof(Film.Genres),
                        obj => (obj as string).Split(",").Select(p => (FilmGenre)int.Parse(p)).ToArray()
                    }
                }),
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.That(1, Is.EqualTo(result.Code));
            Assert.That("my_title 1", Is.EqualTo(result.Title));
            Assert.That(FilmGenre.Action, Is.EqualTo(result.Genres[0]));
            Assert.That(FilmGenre.Comedy, Is.EqualTo(result.Genres[1]));
        }

        [Test]
        public void ExecuteFirst_Query()
        {
            var result = this.dbConnection.ExecuteFirst(
                "SELECT * FROM films WHERE code < @code",
                reader => new { Code = reader.GetInt16(0), Title = reader.GetString(1), },
                new Dictionary<string, object> { { "@code", 3 } });

            Assert.That(1, Is.EqualTo(result.Code));
            Assert.That("my_title 1", Is.EqualTo(result.Title));
        }

        [Test]
        public async Task ExecuteNonQueryAsync()
        {
            Assert.That(1,
                Is.EqualTo(await this.dbConnection.ExecuteNonQueryAsync(
                    "INSERT INTO films VALUES (@code, @title, @genre);",
                    new Dictionary<string, object>
                    {
                        {"@code", 5}, {"@title", "my_title 5"}, {"@genre", FilmGenre.Action},
                    })));
        }

        [Test]
        public void ExecuteReader()
        {
            using var reader = this.dbConnection.ExecuteReader(
                "SELECT * FROM films WHERE code < @code",
                new Dictionary<string, object> { { "@code", 3 } });

            var results = new List<(int Code, string Title)>();

            while (reader.Read())
            {
                results.Add((reader.GetInt16(0), reader.GetString(1)));
            }

            Assert.That(2, Is.EqualTo(results.Count));
            Assert.That(1, Is.EqualTo(results[0].Code));
            Assert.That("my_title 1", Is.EqualTo(results[0].Title));
        }

        [Test]
        public async Task ExecuteReaderAsync()
        {
            await using var reader = await this.dbConnection.ExecuteReaderAsync(
                "SELECT * FROM films WHERE code < @code",
                new Dictionary<string, object> { { "@code", 3 } });

            var results = new List<(int Code, string Title)>();

            while (await reader.ReadAsync())
            {
                results.Add((reader.GetInt16(0), reader.GetString(1)));
            }

            Assert.That(2, Is.EqualTo(results.Count));
            Assert.That(1, Is.EqualTo(results[0].Code));
            Assert.That("my_title 1", Is.EqualTo(results[0].Title));
        }

        [SetUp]
        public async Task SetUp()
        {
            var host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "127.0.0.1";
            var rootPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? "root";
            var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "microservice_framework_tests";

            this.dbConnection =
                new MySqlConnection($"Server={host};User ID=root;Password={rootPassword};database={database};SSLMode=Required");

            // Creates table
            await this.dbConnection.ExecuteNonQueryAsync(
                "CREATE TABLE films ( code int PRIMARY KEY, title varchar(40) NOT NULL, genres varchar(50) NOT NULL);");

            // Inserts some rows in table
            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES (1, 'my_title 1', '1,2');");
            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES (2, 'my_title 2', '2');");
            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES (3, 'my_title 3', '1');");
            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES (4, 'my_title 4', '3');");
        }

        [TearDown]
        public async Task TearDown()
        {
            await this.dbConnection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
            await this.dbConnection.CloseAsync();
        }

        enum FilmGenre
        {
            Doc,
            Action,
            Comedy
        }

        private record Film
        {
            public int Code { get; init; }
            public string Title { get; init; }
            public FilmGenre[] Genres { get; init; }
        }
    }
}