using mpstyle.microservice.toolkit.book.connectionmanager;

using NUnit.Framework;

using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.test.book.connectionmanager
{
    [ExcludeFromCodeCoverage]
    public class PostgreSQLConnectionManagerTest
    {
        private PostgreSQLConnectionManager connectionManager;

        [Test]
        public async Task ExecuteAsync()
        {
            var result = await connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) CONSTRAINT firstkey PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
                return await cmd.ExecuteNonQueryAsync();
            });

            Assert.AreEqual(1, await connectionManager.ExecuteNonQueryAsync("INSERT INTO films VALUES ('mycod', 'my_title');"));
        }

        [SetUp]
        public async Task SetUp()
        {
            this.connectionManager = new PostgreSQLConnectionManager("Server=127.0.0.1;Port=5432;User Id=postgres;Password=postgres;");

            var existDatabase = await connectionManager.ExecuteAsync(async cmd =>
              {
                  cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = 'postgres'";
                  var value = false;
                  using (var reader = await cmd.ExecuteReaderAsync())
                  {
                      while (reader.Read())
                      {
                          value = true;
                      }
                  }
                  return value;
              });

            if (!existDatabase)
            {
                await connectionManager.ExecuteNonQueryAsync("CREATE DATABASE postgresql_test;");
            }

            this.connectionManager = new PostgreSQLConnectionManager("Server=127.0.0.1;Port=5432;User Id=postgres;Password=postgres;Database=postgresql_test");
        }

        [TearDown]
        public async Task TearDown()
        {
            await connectionManager.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
        }
    }
}
