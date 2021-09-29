# Cache manager

## SQLite cache manager

Before user SQLite cache manager, create the table cache:

```sql
CREATE TABLE cache(
    id TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    issuedAt INTEGER NOT NULL
);
```

## MySQL cache manager

Before user MySQL cache manager, create the table cache:

```sql
CREATE TABLE cache(
    id VARCHAR(256) PRIMARY KEY,
    value TEXT NOT NULL,
    issuedAt BIGINT NOT NULL
);
```