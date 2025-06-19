# Message mediator Abstractions

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.messagemediator.abstractions)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.messagemediator.abstractions)

An interface to define how microservices interact each other across single or multi instances app, using **request-response** or **publish-subscrive** patterns.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.messagemediator.abstractions -Version 2.1.0
```

### .NET CLI
```
dotnet add package microservice.toolkit.messagemediator.abstractions --version 2.1.0
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.messagemediator.abstractions" Version="2.1.0" />
```

## Introduction
In Microservice Toolkit, a microservice has a name (or pattern) and returns with the following structure:
```json5
{
    "error": 12,
    "payload": {
        // ...
    }
}
```
Where:
- __Error__ is the core of the error, it has value when an error occurs during service execution.
- __Payload__ is the output of the service, it has value when the execution goes well.

Only one of the fields can have a value: if "error" has a value, "payload" doesn't have it, and vice versa.

## Implementations

Microservice Toolkit provides message mediator (IMessageMediator) implementations which can exchange messages in a [single instance environment](#local) and in a multi instances environment using a message broker like [RabbitMQ](#rabbitmq), [Azure Service Bus](#servicebus) or [NATS](#nats).

Every implementation of message mediator requires a "service factory" which link service name (pattern) to its instance.
