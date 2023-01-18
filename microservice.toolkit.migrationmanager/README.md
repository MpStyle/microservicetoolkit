# Migration Manager

__The library is a work in progress. It is not yet considered production-ready.__

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.migrationmanager)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.migrationmanager)

A collection of extension methods to manage schema evolution across all your environments.

Supported databases:
- PostgreSQL
- SQL Server
- SQLite
- MySQL
- MariaDB
- Cassandra
- CockroachDB

## How to install

### Package Manager
```
Install-Package microservice.toolkit.migrationmanager -Version 0.10.1
```

### .NET CLI
```
dotnet add package microservice.toolkit.migrationmanager --version 0.10.1
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.migrationmanager" Version="0.10.1" />
```

## How to use
Under the hood, the library uses [Evolve](https://evolve-db.netlify.app/) to apply DB migrations, so before to start using Migration Manager, install the required package:

```xml
<PackageReference Include="Evolve" Version="3.0.0" />
```

Example code:
```C#
var dbConnection = new MySqlConnection("Server=<HOST>;User ID=<USERNAME>;Password=<PASSWORD>;database=<DATABASE_NAME>;");
var migrationsFolder = "./migrations";
var migrationExtension = ".mysql";

manager.Apply(migrationsFolder, migrationExtension);
```