﻿# Configuration manager

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.configuration.extensions)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.configuration.extensions)

Common interface to access to configurations value.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.configuration.extensions -Version 2.1.0
```

### .NET CLI
```
dotnet add package microservice.toolkit.configuration.extensions --version 2.1.0
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.configuration.extensions" Version="2.1.0" />
```

## Available methods

### GetBool(string)
```C#
bool GetBool(string key);
```

Tries to retrieve the boolean _value_ of the entry with _key_.
Returns _false_ if not exists.

### GetString(string)
```C#
string GetString(string key);
```

Tries to retrieve the string _value_ of the entry with _key_.
Returns _null_ if not exists.

### GetInt(string)
```C#
int GetInt(string key);
```

Tries to retrieve the integer _value_ of the entry with _key_.
Returns _0_ if not exists.

### GetStringArray(string)
```C#
string[] GetStringArray(string key);
```

Tries to retrieve the _value_ (array of string) of the entry with _key_.
Returns _default_ value if not exists.

### GetIntArray(string)
```C#
int[] GetIntArray(string key);
```

Tries to retrieve the _value_ (int of string) of the entry with _key_.
Returns _default_ value if not exists.

## Implementations
- [Microsoft extensions configuration](#msec)

### Microsoft extensions configuration

<a name="msec"></a>
```C#
using microservice.toolkit.configuration.extensions.extension;

IConfiguration configuration = ...;

var stringValue = configuration.GetString("stringValue");
```