using Microsoft.Data.SqlClient;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.connection.extensions.test;

[ExcludeFromCodeCoverage]
public class SQLServerConnectionExtensionsTest
{
    private SqlConnection dbConnection;

    [Test]
    public void Execute()
    {
        var result = this.dbConnection.Execute(cmd =>
        {
            cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
            return cmd.ExecuteNonQuery();
        });

        Assert.That(-1, Is.EqualTo(result));

        Assert.That(1,
            Is.EqualTo(this.dbConnection.ExecuteNonQuery(
                "INSERT INTO films VALUES ('12345', 'my_title');")));

        var selectResult = this.dbConnection.Execute("SELECT code, title FROM films ORDER BY code", record => new { code = record.GetString(0), title = record.GetString(1) });

        Assert.That(selectResult, Is.Not.Null);
        Assert.That(selectResult[0].code, Is.EqualTo("12345"));
        Assert.That(selectResult[0].title, Is.EqualTo("my_title"));
    }

    [Test]
    public void ExecuteFirst()
    {
        var result = this.dbConnection.Execute(cmd =>
        {
            cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
            return cmd.ExecuteNonQuery();
        });

        Assert.That(-1, Is.EqualTo(result));

        Assert.That(1,
            Is.EqualTo(this.dbConnection.ExecuteNonQuery(
                "INSERT INTO films VALUES ('12345', 'my_title');")));
        Assert.That(1,
            Is.EqualTo(this.dbConnection.ExecuteNonQuery(
                "INSERT INTO films VALUES ('12346', 'my_title_01');")));

        var selectResult = this.dbConnection.ExecuteFirst("SELECT code, title FROM films ORDER BY code DESC", record => new { code = record.GetString(0), title = record.GetString(1) });

        Assert.That(selectResult, Is.Not.Null);
        Assert.That(selectResult.code, Is.EqualTo("12346"));
        Assert.That(selectResult.title, Is.EqualTo("my_title_01"));
    }

    [Test]
    public async Task ExecuteAsync()
    {
        var result = await this.dbConnection.ExecuteAsync(async cmd =>
        {
            cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
            return await cmd.ExecuteNonQueryAsync();
        });

        Assert.That(-1, Is.EqualTo(result));
    }

    [Test]
    public void ExecuteNonQuery()
    {
        this.dbConnection.Execute(cmd =>
        {
            cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
            return cmd.ExecuteNonQuery();
        });

        Assert.That(1,
            Is.EqualTo(this.dbConnection.ExecuteNonQuery(
                "INSERT INTO films VALUES ('12345', 'my_title');")));
    }

    [Test]
    public async Task ExecuteNonQueryAsync()
    {
        await this.dbConnection.ExecuteAsync(async cmd =>
        {
            cmd.CommandText = @"
                    CREATE TABLE films (
                        code        char(5) PRIMARY KEY,
                        title       varchar(40) NOT NULL
                    );
                ";
            return await cmd.ExecuteNonQueryAsync();
        });

        Assert.That(1,
            Is.EqualTo(await this.dbConnection.ExecuteNonQueryAsync(
                "INSERT INTO films VALUES ('12345', 'my_title');")));
    }

    [SetUp]
    public void SetUp()
    {
        var host = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "127.0.0.1";
        var port = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
        var rootPassword = Environment.GetEnvironmentVariable("SQLSERVER_ROOT_PASSWORD") ?? "my_root_password123";
        this.dbConnection =
            new SqlConnection(
                $"Server={host},{port};Database=Master;User Id=SA;Password={rootPassword};TrustServerCertificate=true");
    }

    [TearDown]
    public async Task TearDown()
    {
        await this.dbConnection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS films");
        await this.dbConnection.CloseAsync();
    }
}