# Cache manager

__The library is a work in progress. It is not yet considered production-ready.__

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.cachemanager)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.cachemanager)

Common interface to manage cache using different providers.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.cachemanager -Version 0.4.2
```

### .NET CLI
```
dotnet add package microservice.toolkit.cachemanager --version 0.4.2
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.cachemanager" Version="0.4.2" />
```

## Available methods

### Set(string, string, long);
```C#
Task<bool> Set(string key, string value, long issuedAt);
```

Adds an entry in the cache provider with an expiration time (UTC Unix timestamp in milliseconds).
```C#
// 2 days until expiration time
var issuedAt = DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds()
```

### Set(string, string);
```C#
Task<bool> Set(string key, string value);
```

Adds an entry in the cache provider without an expiration time.

### Get(string);
```C#
Task<string> Get(string key);
```

Tries to retrieve the _value_ of the entry using the _key_. If the entry does not exist or is expired the method returns _null_.

### Delete(string);
```C#
Task<bool> Delete(string key);
```

Removed the entry from the cache provider.

## Implementations
- [SqLite](#sqlite)
- [MySql](#mysql)
- [Memcached](#memcached)
- [Redis](#redis)

### SQLite cache manager

<a name="sqlite"></a>
To start using SQLite cache manager, first install the required package:
```xml
<PackageReference Include="Microsoft.Data.SQLite" Version="5.0.10" />
```

and create the table cache:

```sql
CREATE TABLE cache(
    id TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    issuedAt INTEGER NOT NULL
);
```

### MySQL cache manager

<a name="mysql"></a>
To start using MySql cache manager, first install the required package:
```xml
<PackageReference Include="MySqlConnector" Version="1.3.12" />
```

and create the table cache:

```sql
CREATE TABLE cache(
    id VARCHAR(256) PRIMARY KEY,
    value TEXT NOT NULL,
    issuedAt BIGINT NOT NULL
);
```

### Memcached

<a name="memcached"></a>
To start using Memcached cache manager, first install the required package:
```xml
<PackageReference Include="Enyim.Memcached2" Version="0.6.8" />
```

How to use:
```C#
var manager = new MemcachedCacheManager("localhost:11211");
```
Or, if you are using a cluster:
```C#
var manager = new MemcachedCacheManager("localhost:11211,localhost:11212");
```

### Redis

<a name="redis"></a>
To start using Redis cache manager, first install the required package:
```xml
<PackageReference Include="StackExchange.Redis" Version="2.2.62" />
```

How to use:

```C#
var manager = new RedisCacheManager("localhost:6379");
```