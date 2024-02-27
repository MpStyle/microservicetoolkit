# Testing

To run unit test locally, you can take advantage of Docker containers.

## Docker containers

### MySQL
```bash
docker run -d --name microserviceframework-test-mysql -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=microservice_framework_tests -p 3306:3306 mysql:8
```

### PostgreSQL
```bash
docker run -d --name  microserviceframework-test-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16
```

### MS SQL Server
```bash
docker run -d --name microserviceframework-test-sqlserver -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=my_root_password123" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

### Redis
```bash
docker run -d --name microserviceframework-test-redis -p 6379:6379 redis:alpine
```

### Memcached
```bash
docker run -d --name microserviceframework-test-memcached -p 11211:11211 memcached:alpine
```

### RabbitMQ
```bash
docker run -d --name microserviceframework-test-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:alpine
```

### NATS
```bash
docker run -d --name microserviceframework-test-nats -p 4222:4222 -p 8222:8222 -p 6222:6222 nats:alpine
```