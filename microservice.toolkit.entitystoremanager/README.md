﻿# Entity Store Manager

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.entitystoremanager)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.entitystoremanager)

Store any entities (data models) in a database without write a query.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.entitystoremanager -Version 1.0.0
```

### .NET CLI
```
dotnet add package microservice.toolkit.entitystoremanager --version 1.0.0
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.entitystoremanager" Version="1.0.0" />
```

## Supported operations
- Get item by ID
- Search items
- Count items
- Upsert items

## Supported databases
- Microsoft SQL Server