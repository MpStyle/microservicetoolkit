# Entity Store Manager

__The library is a work in progress. It is not yet considered production-ready.__

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.entitystoremanager)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.entitystoremanager)

Store any entities (data models) in a database without write a query.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.entitystoremanager -Version 0.11.1
```

### .NET CLI
```
dotnet add package microservice.toolkit.entitystoremanager --version 0.11.1
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.entitystoremanager" Version="0.11.1" />
```

## Supported operations
- Get item by ID
- Search items
- Count items
- Upsert items

## Supported databases
- Microsoft SQL Server