using MySqlConnector;

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace microservice.toolkit.connection.extensions.test;

[ExcludeFromCodeCoverage]
public class DbDataReaderExtensionTest
{
    private MySqlConnection dbConnection;

    [Test]
    public async Task GetValuesAsync()
    {
        var result = await await this.dbConnection.ExecuteFirstAsync("SELECT * FROM myTable", async reader =>
        {
            var i = 0;

            var myInt = await reader.GetNullableInt32Async(i++);
            var myLong = await reader.GetNullableInt64Async(i++);
            var myShort = await reader.GetNullableInt16Async(i++);
            var myBool = await reader.GetNullableBooleanAsync(i++);
            var myDecimal = await reader.GetNullableDecimalAsync(i++);
            var myDouble = await reader.GetNullableDoubleAsync(i++);
            var myFloat = await reader.GetNullableFloatAsync(i++);
            var myGuid = await reader.GetNullableGuidAsync(i++);
            var myString = await reader.GetNullableStringAsync(i++);
            var nullValue = await reader.GetNullableInt64Async(i++);

            Assert.Throws<IndexOutOfRangeException>(() => reader.GetNullableInt32(i++));

            return new
            {
                myInt,
                myLong,
                myShort,
                myBool,
                myDecimal,
                myDouble,
                myFloat,
                myGuid,
                myString,
                nullValue,
            };
        });

        Assert.That(1, Is.EqualTo(result.myInt));
        Assert.That(1647860184000, Is.EqualTo(result.myLong));
        Assert.That(32767, Is.EqualTo(result.myShort));
        Assert.That(true, Is.EqualTo(result.myBool));
        Assert.That(999.99, Is.EqualTo(result.myDecimal));
        Assert.That(10.9998, Is.EqualTo(result.myDouble));
        Assert.That(10.9997997f, Is.EqualTo(result.myFloat));
        Assert.That("0f8fad5b-d9cb-469f-a165-70867728950e", Is.EqualTo(result.myGuid.ToString()));
        Assert.That("0f8fad5b-d9cb-469f-a165-70867728950e", Is.EqualTo(result.myString));
        Assert.That(result.nullValue, Is.Null);
    }
    
    [Test]
    public async Task GetValues()
    {
        var result = await this.dbConnection.ExecuteFirstAsync("SELECT * FROM myTable", reader =>
        {
            var i = 0;

            var myInt = reader.GetNullableInt32(i++);
            var myLong = reader.GetNullableInt64(i++);
            var myShort = reader.GetNullableInt16(i++);
            var myBool = reader.GetNullableBoolean(i++);
            var myDecimal = reader.GetNullableDecimal(i++);
            var myDouble = reader.GetNullableDouble(i++);
            var myFloat = reader.GetNullableFloat(i++);
            var myGuid = reader.GetNullableGuid(i++);
            var myString = reader.GetNullableString(i++);
            var nullValue = reader.GetNullableInt64(i++);

            Assert.Throws<IndexOutOfRangeException>(() => reader.GetNullableInt32(i++));

            return new
            {
                myInt,
                myLong,
                myShort,
                myBool,
                myDecimal,
                myDouble,
                myFloat,
                myGuid,
                myString,
                nullValue,
            };
        });

        Assert.That(1, Is.EqualTo(result.myInt));
        Assert.That(1647860184000, Is.EqualTo(result.myLong));
        Assert.That(32767, Is.EqualTo(result.myShort));
        Assert.That(true, Is.EqualTo(result.myBool));
        Assert.That(999.99, Is.EqualTo(result.myDecimal));
        Assert.That(10.9998, Is.EqualTo(result.myDouble));
        Assert.That(10.9997997f, Is.EqualTo(result.myFloat));
        Assert.That("0f8fad5b-d9cb-469f-a165-70867728950e", Is.EqualTo(result.myGuid.ToString()));
        Assert.That("0f8fad5b-d9cb-469f-a165-70867728950e", Is.EqualTo(result.myString));
        Assert.That(result.nullValue, Is.Null);
    }

    [Test]
    public async Task TryGetValues()
    {
        var result = await this.dbConnection.ExecuteFirstAsync("SELECT * FROM myTable", reader =>
        {
            var i = 0;
            reader.TryGetInt32(i++, out var myInt);
            reader.TryGetInt64(i++, out var myLong);
            reader.TryGetInt16(i++, out var myShort);
            reader.TryGetBoolean(i++, out var myBool);
            reader.TryGetDecimal(i++, out var myDecimal);
            reader.TryGetDouble(i++, out var myDouble);
            reader.TryGetFloat(i++, out var myFloat);
            reader.TryGetGuid(i++, out var myGuid);
            reader.TryGetString(i++, out var myString);
            var nullResult = reader.TryGetInt64(i++, out var nullValue);
            var invalidPositionResult = reader.TryGetInt32(i++, out var invalidPositionValue);

            return new
            {
                myInt,
                myLong,
                myShort,
                myBool,
                myDecimal,
                myDouble,
                myFloat,
                myGuid,
                myString,
                invalidPosition = (invalidPositionResult, invalidPositionValue),
                nullValue = (nullResult, nullValue),
            };
        });

        Assert.That(1, Is.EqualTo(result.myInt));
        Assert.That(1647860184000, Is.EqualTo(result.myLong));
        Assert.That(32767, Is.EqualTo(result.myShort));
        Assert.That(true, Is.EqualTo(result.myBool));
        Assert.That(999.99, Is.EqualTo(result.myDecimal));
        Assert.That(10.9998, Is.EqualTo(result.myDouble));
        Assert.That(10.9997997f, Is.EqualTo(result.myFloat));
        Assert.That("0f8fad5b-d9cb-469f-a165-70867728950e", Is.EqualTo(result.myGuid.ToString()));
        Assert.That("0f8fad5b-d9cb-469f-a165-70867728950e", Is.EqualTo(result.myString));

        Assert.That(result.invalidPosition.invalidPositionResult, Is.False);
        Assert.That(default(int), Is.EqualTo(result.invalidPosition.invalidPositionValue));

        Assert.That(result.nullValue.nullResult, Is.False);
        Assert.That(default(long), Is.EqualTo(result.nullValue.nullValue));
    }

    [SetUp]
    public async Task SetUp()
    {
        var host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "127.0.0.1";
        var rootPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? "root";
        var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "microservice_framework_tests";

        this.dbConnection =
            new MySqlConnection(
                $"Server={host};User ID=root;Password={rootPassword};database={database};SSLMode=Required");

        // Creates table
        await this.dbConnection.ExecuteNonQueryAsync(@"
            CREATE TABLE myTable ( 
                myInt INT, 
                myLong BIGINT,
                myShort SMALLINT,
                myBool BIT,
                -- myByte ???, 
                myDecimal DECIMAL(5,2), 
                myDouble DOUBLE, 
                myFloat FLOAT, 
                myGuid BINARY(36), 
                myString VARCHAR(40),
                myNullValue SMALLINT
            );
        ");


        // Inserts some rows in table
        await this.dbConnection.ExecuteNonQueryAsync(@"
            INSERT INTO myTable VALUES (
                1, 
                1647860184000,
                32767,
                1,
                999.99,
                10.9998,
                10.9998,
                '0f8fad5b-d9cb-469f-a165-70867728950e',
                '0f8fad5b-d9cb-469f-a165-70867728950e',
                NULL
            );
        ");
    }

    [TearDown]
    public async Task TearDown()
    {
        await this.dbConnection.ExecuteNonQueryAsync("DROP TABLE IF EXISTS myTable");
        await this.dbConnection.CloseAsync();
    }
}