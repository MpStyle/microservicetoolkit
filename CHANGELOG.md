# Changelog

## Current Release

**What's New**
- Added Service<TRequest, TPayload> extension methods to create a service response
- Improved migration to Nullable projects
- Improved error handling in RabbitMQ message mediator
- Added support for cancellation tokens in asynchronous methods within the Connection Extensions project.
- Add nullable support for boolean and integer configuration retrieval methods in Configuration Extensions library.
- Add ExecuteReader and ExecuteReaderAsync methods for DbConnection and SQLConnection extensions methods to facilitate data retrieval operations and integration with ORM libraries.

**Fixes**
- 

**Breaking Changes**
- Changed type of service response error to string 
- Service errors are changed:
  - Unknown: "mt_0"
  - ServiceNotFound: "mt_1"
  - InvalidPattern: "mt_2"
  - InvalidServiceExecution: "mt_4"
  - ExecutionTimeout: "mt_5"
  - EmptyResponse: "mt_6"
  - EmptyRequest: "mt_7"
  - Timeout: "mt_8"
  - InvalidRequestType: "mt_9"
  - NullRequest: "mt_10"
  - NullResponse: "mt_11"
- Removed default value of CancellationToken in RunAsync method of IService interface 