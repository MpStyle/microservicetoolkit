using Microsoft.Data.Sqlite;

using NUnit.Framework;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.connectionmanager.test
{
    [ExcludeFromCodeCoverage]
    public class SQLiteConnectionManagerTest
    {
        private SqliteConnection connectionManager;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await connectionManager.ExecuteAsync(async cmd =>
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
            await connectionManager.ExecuteAsync(async cmd =>
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
                Is.EqualTo(await connectionManager.ExecuteNonQueryAsync(
                    "INSERT INTO films VALUES (123, 'my_title');")));
        }

        [Test]
        public async Task ExecuteScalarAsync()
        {
            await connectionManager.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES (123, 'my_title');");

            Assert.That("my_title",
                Is.EqualTo(await connectionManager.ExecuteScalarAsync<string>(
                    "SELECT title FROM films WHERE code = 123;")));
        }


        [Test]
        public async Task ExecuteScalarAsync_With_Converter()
        {
            await connectionManager.ExecuteAsync(async cmd =>
            {
                cmd.CommandText = """
                                  CREATE TABLE films (
                                      code        INTEGER PRIMARY KEY,
                                      title       TEXT NOT NULL
                                  );
                                  """;
                return await cmd.ExecuteNonQueryAsync();
            });

            await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES (123, 'my_title');");

            var result =
                await connectionManager.ExecuteScalarAsync("SELECT title FROM films WHERE code = @code;",
                    o => (string)o, new Dictionary<string, object> {{"@code", 123}});

            Assert.That("my_title", Is.EqualTo(result));
        }
        
        [Test]
        public void ExecuteScalar()
        {
            connectionManager.Execute(cmd =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        INTEGER PRIMARY KEY,
                        title       TEXT NOT NULL
                    );
                ";
                return cmd.ExecuteNonQuery();
            });

            connectionManager.ExecuteNonQuery("INSERT INTO films VALUES (123, 'my_title');");

            Assert.That("my_title",
                Is.EqualTo(connectionManager.ExecuteScalar<string>(
                    "SELECT title FROM films WHERE code = 123;")));
        }


        [Test]
        public void ExecuteScalar_With_Converter()
        {
            connectionManager.Execute(cmd =>
            {
                cmd.CommandText = """
                                  CREATE TABLE films (
                                      code        INTEGER PRIMARY KEY,
                                      title       TEXT NOT NULL
                                  );
                                  """;
                return cmd.ExecuteNonQuery();
            });

            connectionManager.ExecuteNonQuery("INSERT INTO films VALUES (123, 'my_title');");

            var result = connectionManager.ExecuteScalar("SELECT title FROM films WHERE code = @code;",
                    o => (string)o, new Dictionary<string, object> {{"@code", 123}});

            Assert.That("my_title", Is.EqualTo(result));
        }

        [SetUp]
        public void SetUp()
        {
            this.connectionManager = new SqliteConnection("Data Source=CacheTest;Mode=Memory;Cache=Shared");
        }

        [TearDown]
        public async Task TearDown()
        {
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
            await connectionManager.CloseAsync();
        }
    }
}