﻿# Migration Manager

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.migration.extensions)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.migration.extensions)

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
Install-Package microservice.toolkit.migration.extensions -Version 2.1.0
```

### .NET CLI
```
dotnet add package microservice.toolkit.migration.extensions --version 2.1.0
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.migration.extensions" Version="2.1.0" />
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