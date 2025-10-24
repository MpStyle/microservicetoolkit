using Microsoft.Data.Sqlite;

using NUnit.Framework;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.connection.extensions.test
{
    [ExcludeFromCodeCoverage]
    public class SQLiteConnectionExtensionsTest
    {
        private SqliteConnection dbConnection;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await this.dbConnection.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.That(0, Is.EqualTo(result));
        }

        [Test]
        public async Task ExecuteNonQueryAsync()
        {
            await this.dbConnection.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.That(1,
                Is.EqualTo(await this.dbConnection.ExecuteNonQueryAsync(
                    "INSERT INTO films VALUES (123, 'my_title');")));
        }

        [Test]
        public async Task ExecuteScalarAsync()
        {
            await this.dbConnection.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES (123, 'my_title');");

            Assert.That("my_title",
                Is.EqualTo(await this.dbConnection.ExecuteScalarAsync<string>(
                    "SELECT title FROM films WHERE code = 123;")));
        }


        [Test]
        public async Task ExecuteScalarAsync_With_Converter()
        {
            await this.dbConnection.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = """
                                  CREATE TABLE films (
                                      code        INTEGER PRIMARY KEY,
                                      title       TEXT NOT NULL
                                  );
                                  """;
                return await cmd.ExecuteNonQueryAsync();
            });

            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES (123, 'my_title');");

            var result =
                await this.dbConnection.ExecuteScalarAsync("SELECT title FROM films WHERE code = @code;",
                    o => (string)o, new Dictionary<string, object> {{"@code", 123}});

            Assert.That("my_title", Is.EqualTo(result));
        }
        
        [Test]
        public void ExecuteScalar()
        {
            this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return cmd.ExecuteNonQuery();
            });

            this.dbConnection.ExecuteNonQuery("INSERT INTO films VALUES (123, 'my_title');");

            Assert.That("my_title",
                Is.EqualTo(this.dbConnection.ExecuteScalar<string>(
                    "SELECT title FROM films WHERE code = 123;")));
        }


        [Test]
        public void ExecuteScalar_With_Converter()
        {
            this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText = """
                                  CREATE TABLE films (
                                      code        INTEGER PRIMARY KEY,
                                      title       TEXT NOT NULL
                                  );
                                  """;
                return cmd.ExecuteNonQuery();
            });

            this.dbConnection.ExecuteNonQuery("INSERT INTO films VALUES (123, 'my_title');");

            var result = this.dbConnection.ExecuteScalar("SELECT title FROM films WHERE code = @code;",
                    o => (string)o, new Dictionary<string, object> {{"@code", 123}});

            Assert.That("my_title", Is.EqualTo(result));
        }

        [Test]
        public void ExecuteReader()
        {
            this.dbConnection.Execute(cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return cmd.ExecuteNonQuery();
            });

            this.dbConnection.ExecuteNonQuery("INSERT INTO films VALUES (1, 'my_title 1');");
            this.dbConnection.ExecuteNonQuery("INSERT INTO films VALUES (2, 'my_title 2');");

            using var reader = this.dbConnection.ExecuteReader("SELECT code, title FROM films WHERE code < @code", new System.Collections.Generic.Dictionary<string, object> { { "@code", 3 } });

            var results = new List<(int Code, string Title)>();
            while (reader.Read())
            {
                results.Add((reader.GetInt32(0), reader.GetString(1)));
            }

            Assert.That(2, Is.EqualTo(results.Count));
            Assert.That(1, Is.EqualTo(results[0].Code));
            Assert.That("my_title 1", Is.EqualTo(results[0].Title));
        }

        [Test]
        public async System.Threading.Tasks.Task ExecuteReaderAsync()
        {
            await this.dbConnection.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = """
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                """;
                return await cmd.ExecuteNonQueryAsync();
            });

            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES (1, 'my_title 1');");
            await this.dbConnection.ExecuteNonQueryAsync("INSERT INTO films VALUES (2, 'my_title 2');");

            await using var reader = await this.dbConnection.ExecuteReaderAsync("SELECT code, title FROM films WHERE code < @code", new System.Collections.Generic.Dictionary<string, object> { { "@code", 3 } });

            var results = new System.Collections.Generic.List<(int Code, string Title)>();
            while (await reader.ReadAsync())
            {
                results.Add((reader.GetInt32(0), reader.GetString(1)));
            }

            Assert.That(2, Is.EqualTo(results.Count));
            Assert.That(1, Is.EqualTo(results[0].Code));
            Assert.That("my_title 1", Is.EqualTo(results[0].Title));
        }

        [SetUp]
        public void SetUp()
        {
            this.dbConnection = new SqliteConnection("Data Source=CacheTest;Mode=Memory;Cache=Shared");
        }

        [TearDown]
        public async Task TearDown()
        {
            await this.dbConnection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
            await this.dbConnection.CloseAsync();
        }
    }
}