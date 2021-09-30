# Cache manager

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

### Redis

<a name="redis"></a>
- Install the dependency:
    ```
    <PackageReference Include="StackExchange.Redis" Version="2.2.62" />
    ```