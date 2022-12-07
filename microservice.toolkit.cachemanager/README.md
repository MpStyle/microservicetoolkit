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
Install-Package microservice.toolkit.cachemanager -Version 0.9.1
```

### .NET CLI
```
dotnet add package microservice.toolkit.cachemanager --version 0.9.1
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.cachemanager" Version="0.9.1" />
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

## Implementations
- [In-memory](#inmemory)
- [SqLite](#sqlite)
- [MySql](#mysql)
- [Memcached](#memcached)
- [Redis](#redis)

### In memory
<a name="inmemory"></a>
Naif version of an in-memory cache implementation.

How to use:
```C#
var manager = new InMemoryCacheManager();
```

You can specify value serializer, choose between "_Newtonsoft JSON_", "_XML_" and "_System.Text.Json_" (default).\

**Newtonsoft JSON**\
Install Newtonsoft JSON dependency:
```
dotnet add package Newtonsoft.Json --version 13.0.2
```
And use the serializer:
```C#
var manager = new InMemoryCacheManager(new NewtonsoftJsonCacheValueSerializer());
```

**System.Text.Json**
```C#
var manager = new InMemoryCacheManager(new JsonCacheValueSerializer());
```

**XML**
```C#
var manager = new InMemoryCacheManager(new XmlCacheValueSerializer());
```

### SQLite cache manager

<a name="sqlite"></a>
To start using SQLite cache manager, first install the required package:
```xml
<PackageReference Include="Microsoft.Data.SQLite" Version="5.0.10" />
```

Create the table cache:

```sql
CREATE TABLE cache(
    id TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    issuedAt INTEGER NOT NULL
);
```

And instantiate the cache manger:
```C#
var connectionManager = new SqliteConnection($"[CONNECTION_STRING]");
var manager = new SQLiteCacheManager(connectionManager);
```

You can specify value serializer, choose between "_Newtonsoft JSON_", "_XML_" and "_System.Text.Json_" (default).\

**Newtonsoft JSON**\
Install Newtonsoft JSON dependency:
```
dotnet add package Newtonsoft.Json --version 13.0.2
```
And use the serializer:
```C#
var manager = new SQLiteCacheManager(connectionManager, new NewtonsoftJsonCacheValueSerializer());
```

**System.Text.Json**
```C#
var manager = new SQLiteCacheManager(connectionManager, new JsonCacheValueSerializer());
```

**XML**
```C#
var manager = new SQLiteCacheManager(connectionManager, new XmlCacheValueSerializer());
```

### MySQL cache manager

<a name="mysql"></a>
To start using MySql cache manager, first install the required package:
```xml
<PackageReference Include="MySqlConnector" Version="1.3.12" />
```

Create the table cache:

```sql
CREATE TABLE cache(
    id VARCHAR(256) PRIMARY KEY,
    value TEXT NOT NULL,
    issuedAt BIGINT NOT NULL
);
```

And instantiate the cache manger:
```C#
var connectionManager = new MySqlConnection($"[CONNECTION_STRING]");
var manager = new MysqlCacheManager(connectionManager);
```

You can specify value serializer, choose between "_Newtonsoft JSON_", "_XML_" and "_System.Text.Json_" (default).\

**Newtonsoft JSON**\
Install Newtonsoft JSON dependency:
```
dotnet add package Newtonsoft.Json --version 13.0.2
```
And use the serializer:
```C#
var manager = new MysqlCacheManager(connectionManager, new NewtonsoftJsonCacheValueSerializer());
```

**System.Text.Json**
```C#
var manager = new MysqlCacheManager(connectionManager, new JsonCacheValueSerializer());
```

**XML**
```C#
var manager = new MysqlCacheManager(connectionManager, new XmlCacheValueSerializer());
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

You can specify value serializer, choose between "_Newtonsoft JSON_", "_XML_" and "_System.Text.Json_" (default).\

**Newtonsoft JSON**\
Install Newtonsoft JSON dependency:
```
dotnet add package Newtonsoft.Json --version 13.0.2
```
And use the serializer:
```C#
var manager = new RedisCacheManager("localhost:6379", new NewtonsoftJsonCacheValueSerializer());
```

**System.Text.Json**
```C#
var manager = new RedisCacheManager("localhost:6379", new JsonCacheValueSerializer());
```

**XML**
```C#
var manager = new RedisCacheManager("localhost:6379", new XmlCacheValueSerializer());
```