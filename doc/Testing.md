# Testing

To run unit test locally, you can take advantage of Docker containers.

## Docker containers

It is possible to run each container individually (see the following paragraphs) or use the [docker-compose.yml](docker-compose.yml) file to run them all with a single command:
```bash
docker-compose up -d
```

### MySQL
```bash
docker run --detach --name=microserviceframework-test-mysql --env="MYSQL_ROOT_PASSWORD=root" --env="MYSQL_DATABASE=microservice_framework_tests" --publish 3306:3306 mysql:8
```

### PostgreSQL
```bash
docker run --name  microserviceframework-test-postgres -p 5432:5432 -e POSTGRES_PASSWORD=postgres -d postgres
```

### MS SQL Server
```bash
docker run --name microservice -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=my_root_password123" -p 1444:1433 -d mcr.microsoft.com/mssql/server:2019-latest
docker run --name clm-db-tests -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=My.Password!1434" -p 1434:1433 -ti mcr.microsoft.com/mssql/server:2022-latest
```

### Redis
```bash
docker run --name microserviceframework-test-redis -p 6379:6379 -d redis:alpine
```

### Memcached
```bash
docker run --name microserviceframework-test-memcached -d -p 11211:11211 memcached:alpine
```

### RabbitMQ
```bash
docker run -d --name microserviceframework-test-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:alpine
```

### NATS
```bash
docker run -d --name microserviceframework-test-nats -p 4222:4222 -p 8222:8222 -p 6222:6222 nats:alpine
```