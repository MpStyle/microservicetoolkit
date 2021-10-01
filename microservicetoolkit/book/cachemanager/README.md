# Cache manager

Common interface to manage cache using different providers.

## Available methods

### Set(string, string, long);
```C#
Task<bool> Set(string key, string value, long issuedAt);
```

Adds an entry in the cache provider with an expiration time (Unix timestamp in milliseconds).

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
Before user SQLite cache manager:

- Install the dependency:
    ```
    <PackageReference Include="Microsoft.Data.SQLite" Version="5.0.10" />
    ```

- Create the table cache:
    ```sql
    CREATE TABLE cache(
        id TEXT PRIMARY KEY,
        value TEXT NOT NULL,
        issuedAt INTEGER NOT NULL
    );
    ```

### MySQL cache manager

<a name="mysql"></a>
Before user MySQL cache manager:

- Install the dependency:
    ```
    <PackageReference Include="MySqlConnector" Version="1.3.12" />
    ```

- Create the table cache:
    ```sql
    CREATE TABLE cache(
        id VARCHAR(256) PRIMARY KEY,
        value TEXT NOT NULL,
        issuedAt BIGINT NOT NULL
    );
    ```

### Memcached

<a name="memcached"></a>
- Install the dependency:
    ```
    <PackageReference Include="Enyim.Memcached2" Version="0.6.8" />
    ```
- Instantiate:
    ```C#
    var manager = new MemcachedCacheManager("localhost:11211");
    ```
    Or, if you are using a cluster:
    ```C#
    var manager = new MemcachedCacheManager("localhost:11211,localhost:11212");
    ```

### Redis

<a name="redis"></a>
- Install the dependency:
    ```
    <PackageReference Include="StackExchange.Redis" Version="2.2.62" />
    ```