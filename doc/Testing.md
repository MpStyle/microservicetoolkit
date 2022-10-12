# Testing

To run unit test locally, you can take advantage of Docker containers.

## Docker containers

**MySQL**:
```bash
docker run --detach --name=microserviceframework-test-mysql --env="MYSQL_ROOT_PASSWORD=root" --env="MYSQL_DATABASE=microservice_framework_tests" --publish 3306:3306 mysql:8
```
**PostgreSQL**:
```bash
docker run --name  microserviceframework-test-postgres -p 5432:5432 -e POSTGRES_PASSWORD=postgres -d postgres
```
**MS SQL Server**:
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=my_root_password123" -p 1433:1433 --name microserviceframework-test-sqlserver -d mcr.microsoft.com/mssql/server:2019-latest
```
**Redis**:
```bash
docker run --name microserviceframework-test-redis -p 6379:6379 -d redis:alpine
```
**Memcached**:
```bash
docker run --name microserviceframework-test-memcached -d -p 11211:11211 memcached:alpine
```

**RabbitMQ**
docker run -d --name microserviceframework-test-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:alpine
