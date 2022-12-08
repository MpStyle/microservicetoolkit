# Microservice Toolkit

__The library is a work in progress. It is not yet considered production-ready.__

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.core)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.core)

Everything you need for your entire micro services development life cycle. 

__Microservice Toolkit__ is the fastest and smartest way to produce industry-leading microservices that users love.

A progressive .NET and ASP.NET toolkit for coding fast, reliable and scalable server-side and desktop applications.

Written in C#, __Microservice Toolkit__ takes advantage of the power of .NET framework to make possible to write efficient code in a short time. 

![Microservice Toolkit Logo](image/icon.png)

## How to install

It is a modular toolkit. 

The Core library ("__microservice.toolkit.core__") is the foundation and it is a necessary dependency if you want to use __Microservice Toolkit__.

### Package Manager
```
Install-Package microservice.toolkit.core -Version 0.9.1
```

### .NET CLI
```
dotnet add package microservice.toolkit.core --version 0.9.1
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.core" Version="0.9.1" />
```

## Key Features :key:

The other libraries (included in __Microservice Toolkit__) collects and enriches the already powerful .NET framework and other C# libraries. 

- [Message mediator](./microservice.toolkit.messagemediator/README.md): microservices connection on desktop and cloud application
- [Cache manager](./microservice.toolkit.cachemanager/README.md): warehouse for web and cloud application for frequently requested data
- [Configuration Manager](./microservice.toolkit.configurationmanager/README.md): strongly typed configuration reader
- [Database connection manager](./microservice.toolkit.connectionmanager/README.md): enriches the DbConnection object with powerful extension methods
- [Migration Manager](./microservice.toolkit.migrationmanager/README.md): version control for databases
- [TSID](./microservice.toolkit.tsid/README.md): Time Sortable Identifier library

### Release Notes :page_with_curl:
[Version history](https://github.com/MpStyle/microservicetoolkit/releases)

## How to release a new version :rocket:

To release a new version of the package:
1. Update version in `Directory.Build.props` file.
3. Draft a release on [GitHub](https://github.com/MpStyle/microservicetoolkit/releases)

## License :bookmark_tabs:

[MIT License](https://opensource.org/licenses/MIT)

## How to contribute

[Contributing](CONTRIBUTING.md)