# Message mediator

__The library is a work in progress. It is not yet considered production-ready.__

An interface to define how microservices interact each other.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.messagemediator -Version 0.4.0
```

### .NET CLI
```
dotnet add package microservice.toolkit.messagemediator --version 0.4.0
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.messagemediator" Version="0.4.0" />
```

## Introduction
The response of a microservice is an object like this:
```json
{
    "error": 12,
    "payload": {
        ...
    }
}
```
- The "error" is valorized when an error occurs during service execution.
- Payload is the output of the service.

Only one of the fields can have a value: if "error" has a value, "payload" doesn't have it, and vice versa

To implement a service, extends the abstract class "_Service<TRequest, TPayload>_", where:
- "_TRequest_" is the input of the service
- "_TPayload_" is the output of the service.

Example code:

```C#
public class UserExists : Service<UserExistsRequest, UserExistsResponse>
{
    public async override Task<ServiceResponse<UserExistsResponse>> Run(UserExistsRequest request)
    {
        return this.SuccessfulResponse(new UserExistsResponse
        {
            Exists = "Alice" == request.Username
        });
    }
}
```

## Implementations

Microservice Toolkit provides some implementation of the message mediator interface:
- [Local](#local)
- [RabbitMQ](#rabbitmq)
- [Azure Service Bus](#servicebus)

Every implementation can work in a single instance environment

![Single instance](./docs/mediator_single_instance.png)

and in a multi instances environment:

![Single instance](./docs/mediator_multi_instances.png)

### Local

<a name="local"></a>
To use in a single instance environment or for testing.

### RabbitMQ

<a name="rabbitmq"></a>
RabbitMQ is an open-source and lightweight message broker which supports multiple messaging protocols. It can be deployed in distributed and federated configurations to meet high-scale, high-availability requirements. In addition, it's the most widely deployed message broker, used worldwide at small startups and large enterprises.

#### Installation

To start building RabbitMQ-based microservices, first install the required packages:
```
Install-Package RabbitMQ.Client -Version 6.2.2
```
Or:
```
<PackageReference Include="RabbitMQ.Client" Version="6.2.2" />
```

### Azure Service Bus

<a name="servicebus"></a>
```
Install-Package Microsoft.Azure.ServiceBus -Version 5.1.3
```
Or:
```
<PackageReference Include="Microsoft.Azure.ServiceBus" Version="5.1.3"/>
```

## Overview

To make know the mediator how to retrieve the instances of micro-services implement the delegate "_ServiceFactory_":

Example:
```C#
// Service factory
var serviceFactory = new ServiceFactory((string pattern) => serviceProvider.GetService(microservices[pattern]) as IService)

[...]

// Message mediator, you can use the dependency injection to instantiate it
var messageMediator = new LocalMessageMediator(logger, serviceFactory)

[...]

// Send a message
var response = await messageMediator.Send<UserExistsRequest, UserExistsResponse>(nameof(UserExists), new UserExistsRequest
{
    Username = "Alice"
});
```