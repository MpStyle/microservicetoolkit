# Microservice Toolkit

__The library is a work in progress. It is not yet considered production-ready.__

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)

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


## Key Features :key:
- [Micro-services mediator](microservicetoolkit/book/messagemediator/README.md)
- Cache manager
- Configuration Manager
- Database connection manager
- Migration Manager

### Release Notes :page_with_curl:
[Version history](https://github.com/MpStyle/microservicetoolkit/releases)

## How to release a new versione :rocket:

To release a new version of the package:
1. Update the version in the `mpstyle.microservice.toolkit.csproj` file.
3. Draft a release on [GitHub](https://github.com/MpStyle/microservicetoolkit/releases)

## License :bookmark_tabs:

[MIT License](https://opensource.org/licenses/MIT)

## How to contribute

If you want to contribute to the project and make it better, your help is very welcome. 

### How to make a clean pull request
- Create a personal fork of this project.
- Clone the fork on your local machine. Your remote repo on Github is called origin.
- Add the original repository as a remote called upstream.
- If you created your fork a while ago be sure to pull upstream changes into your local repository.
- Create a new branch from master to work on.
- Implement/fix your feature, comment and your code.
- Follow the code style of the project, including indentation (see [editor config](.editorconfig) file).
- __Run tests!__
- __Write or adapt tests as needed.__
- Add or change the documentation as needed.
- Squash your commits into a single commit with git's interactive rebase. Create a new branch if necessary.
- Push your branch to your fork on Github, the remote origin.
- From your fork open a pull request in the correct branch. Target the project's master branch!
- …
- If the maintainer requests further changes just push them to your branch. The PR will be updated automatically.
- Once the pull request is approved and merged you can pull the changes from upstream to your local repo and delete your extra branch(es).
And last but not least: Always write your commit messages in the present tense. Your commit message should describe what the commit, when applied, does to the code – not what you did to the code.