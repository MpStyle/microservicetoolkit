# Configuration manager

Common interface to access to configurations value.

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
IConfiguration configuration = ...;
var configurationManager = new ConfigurationManager(configuration);
var stringValue = configurationManager.GetString("stringValue");
```