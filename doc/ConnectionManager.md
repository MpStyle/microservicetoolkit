# Connection Manager

Common interface to simplify the access to a SQL database.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.connectionmanager -Version 0.4.0
```

### .NET CLI
```
dotnet add package microservice.toolkit.connectionmanager --version 0.4.0
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.connectionmanager" Version="0.4.0" />
```

## Available methods

### Execute<T>(Func<DbCommand, T>)
```C#
T Execute<T>(Func<DbCommand, T> lambda);
```
### ExecuteAsync<T>(Func<DbCommand, Task<T>>)
```C#
Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda);
```
### ExecuteNonQueryAsync(string, Dictionary<string, object>)
```C#
Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters);
```
### GetCommand()
```C#
DbCommand GetCommand();
```
### GetCommand(DbConnection)
```C#
DbCommand GetCommand(DbConnection dbConnection);
```
### GetParameter<T>(string, T)
```C#
DbParameter GetParameter<T>(string name, T value);
```
### Open()
```C#
void Open();
```
### OpenAsync()
```C#
Task OpenAsync();
```
### Close()
```C#
void Close();
```
### CloseAsync()
```C#
Task CloseAsync();
```

## Implementations
- [SqLite](#sqlite)
- [MySql](#mysql)
- [PostgreSQL](#postgresql)
- [MS Sql Server](#mssqlserver)

### SQLite connection manager

<a name="sqlite"></a>
To start using SQLite cache manager, first install the required package:
```xml
<PackageReference Include="Microsoft.Data.SQLite" Version="5.0.10" />
```
### MySQL connection manager

<a name="mysql"></a>
To start using MySql cache manager, first install the required package:
```xml
<PackageReference Include="MySqlConnector" Version="1.3.12" />
```
### PostgreSQL connection manager

<a name="postgresql"></a>
To start using PostgreSQL cache manager, first install the required package:
```xml
<PackageReference Include="Npgsql" Version="5.0.10" />
```
### MS SQL Server connection manager

<a name="mssqlserver"></a>
To start using MS SQL Server cache manager, first install the required package:
```xml
<PackageReference Include="System.Data.SqlClient" Version="4.8.3" />
```