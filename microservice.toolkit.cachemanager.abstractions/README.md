# Cache manager abstractions

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.cachemanager)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.cachemanager)

Common interface to manage cache using different providers.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.cachemanager.abstractions -Version 2.1.0
```

### .NET CLI
```
dotnet add package microservice.toolkit.cachemanager.abstractions --version 2.1.0
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.cachemanager.abstractions" Version="2.1.0" />
```

## ICacheManager interface

### bool Set(string, TValue, long);

```C#
Task<bool> Set<TValue>(string key, TValue value)
```

### bool Set<TValue>(string, TValue);

```C#
Task<bool> Set<TValue>(string key, TValue value, long issuedAt)
```

Adds an entry in the cache provider without expiration time.

### TValue Get(string);

```C#
Task<TValue> Get<TValue>(string key)
```

### Delete(string);
```C#
Task<bool> Delete(string key);
```

Removed the entry from the cache provider.