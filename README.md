# Microservice Toolkit

![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)
![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)

Everything you need for your entire micro services development life cycle. 

__Microservice Toolkit__ is the fastest and smartest way to produce industry-leading microservices that users love.

## Introduction
In Microservice Toolkit, a service is an object like this:
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


## Key Features
- [Micro-services mediator](microservicetoolkit/book/messagemediator/README.md)
- Cache manager
- Configuration Manager
- Database connection manager
- Migration Manager

### Release Notes
[Version history](https://github.com/MpStyle/microservicetoolkit/releases)

## How to release a new versione :rocket:

To release a new version of the package:
1. Update the version in the `mpstyle.microservice.toolkit.csproj` file.
3. Draft a release on [GitHub](https://github.com/MpStyle/microservicetoolkit/releases)

## License

[MIT License](https://opensource.org/licenses/MIT)