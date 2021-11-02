# Connection Manager

__The library is a work in progress. It is not yet considered production-ready.__

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.connectionmanager)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.connectionmanager)

Common interface to simplify the access to a SQL database.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.connectionmanager -Version 0.4.4
```

### .NET CLI
```
dotnet add package microservice.toolkit.connectionmanager --version 0.4.4
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.connectionmanager" Version="0.4.4" />
```

## Available extensions methods

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

### SafeOpen()
```C#
void SafeOpen();
```
Opens a connection to the database if it is not already opened.

### SafeOpenAsync()
```C#
Task SafeOpenAsync();
```
Opens a connection to the database if it is not already opened.

### SafeClose()
```C#
void SafeClose();
```
Closes a connection to the database if it is not already closed.

### SafeCloseAsync()
```C#
Task SafeCloseAsync();
```
Closes a connection to the database if it is not already closed.