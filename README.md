# Microservice Toolkit

__The library is a work in progress. It is not yet considered production-ready.__

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)

Everything you need for your entire micro services development life cycle. 

__Microservice Toolkit__ is the fastest and smartest way to produce industry-leading microservices that users love.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.core -Version 0.4.0
```

### .NET CLI
```
dotnet add package microservice.toolkit.core --version 0.4.0
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.core" Version="0.4.0" />
```

## Key Features :key:
- [Message mediator](./microservice.toolkit.messagemediator/README.md): microservices connection on desktop and cloud application
- [Cache manager](./microservice.toolkit.cachemanager/README.md): warehouse for frequently requested data
- [Configuration Manager](./microservice.toolkit.configurationmanager/README.md): strongly typed configuration reader
- [Database connection manager](./microservice.toolkit.connectionmanager/README.md): link to SQL databases
- [Migration Manager](./microservice.toolkit.migrationmanager/README.md): version control for databases

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