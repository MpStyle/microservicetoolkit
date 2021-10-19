﻿# Connection Manager

__The library is a work in progress. It is not yet considered production-ready.__

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

### Execute<T>(string, Func<DbDataReader, T>, Dictionary<string, object>)
```C#
List<T> Execute<T>(string sql, Func<DbDataReader, T> lambda, Dictionary<string, object> parameters = null);
```

### ExecuteFirst<T>(string, Func<DbDataReader, T>, Dictionary<string, object>)
```C#
T ExecuteFirst<T>(string sql, Func<DbDataReader, T> lambda, Dictionary<string, object> parameters = null);
```

### ExecuteAsync<T>(Func<DbCommand, Task<T>>)
```C#
Task<T> ExecuteAsync<T>(Func<DbCommand, Task<T>> lambda);
```

### ExecuteAsync<T>(string, Func<DbDataReader, T>, Dictionary<string, object>)
```C#
Task<List<T>> ExecuteAsync<T>(string sql, Func<DbDataReader, T> lambda, Dictionary<string, object> parameters = null);
```

### ExecuteFirstAsync<T>(string, Func<DbDataReader, T>, Dictionary<string, object>);
```C#
Task<T> ExecuteFirstAsync<T>(string sql, Func<DbDataReader, T> lambda, Dictionary<string, object> parameters = null);
```

### ExecuteNonQueryAsync(string, Dictionary<string, object>)
```C#
Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters);
```
Executes the _query_ using _parameters_

### GetCommand()
```C#
DbCommand GetCommand();
```
Creates a _DbCommand_ for the current connection. 

### GetCommand(DbConnection)
```C#
DbCommand GetCommand(DbConnection dbConnection);
```
Creates a _DbCommand_ for _connection_.

### GetParameter<T>(string, T)
```C#
DbParameter GetParameter<T>(string name, T value);
```
Creates a _DbParameter_. If value is null will be used _DBNull.Value_.

### Open()
```C#
void Open();
```
Opens a connection to the database if it is not already opened.

### OpenAsync()
```C#
Task OpenAsync();
```
Opens a connection to the database if it is not already opened.

### Close()
```C#
void Close();
```
Closes a connection to the database if it is not already closed.

### CloseAsync()
```C#
Task CloseAsync();
```
Closes a connection to the database if it is not already closed.

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