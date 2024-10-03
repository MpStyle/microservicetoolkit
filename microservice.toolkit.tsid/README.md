# TSID Creator

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.tsid)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.tsid)

This is library for Time Sortable Identifier (TSID). It is a C# porting of Java library [tsid-creator](https://github.com/f4b6a3/tsid-creator) by [f4b6a3](https://github.com/f4b6a3)

It brings together ideas from [Twitter's Snowflake](https://github.com/twitter-archive/snowflake/tree/snowflake-2010) and [ULID Spec](https://github.com/ulid/spec).

In summary:

*   Sorted by generation time;
*   Can be stored as an integer of 64 bits;
*   Can be stored as a string of 13 chars;
*   String format is encoded to [Crockford's base32](https://www.crockford.com/base32.html);
*   String format is URL safe, is case insensitive, and has no hyphens;
*   Shorter than UUID, ULID and KSUID.

## How to Use

Create a TSID:

```C#
var factory = new TsidFactory();
var tsid = factory.Create();
```

Create a TSID number:

```C#
var factory = new TsidFactory();
var numberTsid = factory.Create().Number; // 38352658567418872
```

Create a TSID string:

```C#
var factory = new TsidFactory();
var stringTsid = factory.Create().ToString(); // 01226N0640J7Q
```

There are three predefined node ranges: 256, 1024 and 4096.

The TSID generator is [thread-safe](https://en.wikipedia.org/wiki/Thread_safety).

### TSID as Long

This section shows how to create TSID numbers.

The property `Tsid.Number` simply unwraps the internal `long` value.

```C#
// Create a TSID for up to 256 nodes and 16384 ID/ms
var factory = new TsidFactory({
    TsidLength = TsidLength.Tsid256
});
var tsid = factory.Create().Number;
```

```C#
// Create a TSID for up to 1024 nodes and 4096 ID/ms
var factory = new TsidFactory();
var tsid = factory.Create().Number;
```

```C#
// Create a TSID for up to 4096 nodes and 1024 ID/ms
var factory = new TsidFactory({
    TsidLength = TsidLength.Tsid4096
});
var tsid = factory.Create().Number;
```

Sequence of TSIDs:

```text
2274459083952128
2274459088146433 < millisecond changed
2274459088146434
2274459088146435
2274459088146436
2274459088146437
2274459088146438
2274459088146439
2274459088146440
2274459088146441
2274459088146442
2274459088146443
2274459088146444
2274459088146445
2274459088146446
         ^      ^ look
                                   
|--------|------|
   time   random
```

### TSID as String

This section shows how to create TSID strings.

The TSID string is a 13 characters long string encoded to [Crockford's base 32](https://www.crockford.com/base32.html).

```C#
// Create a TSID string for up to 256 nodes and 16384 ID/ms
var factory = new TsidFactory({
    TsidLength = TsidLength.Tsid256
});
var tsid = factory.Create().ToString();
```

```C#
// Create a TSID string for up to 1024 nodes and 4096 ID/ms
var factory = new TsidFactory({
    TsidLength = TsidLength.Tsid1024
});
var tsid = factory.Create().ToString();
```

```C#
// Create a TSID string for up to 4096 nodes and 1024 ID/ms
var factory = new TsidFactory({
    TsidLength = TsidLength.Tsid4096
});
var tsid = factory.Create().ToString();
```

Sequence of TSID strings:

```text
0020N58ZC2M00
0020N58ZG2M01 < millisecond changed
0020N58ZG2M02
0020N58ZG2M03
0020N58ZG2M04
0020N58ZG2M05
0020N58ZG2M06
0020N58ZG2M07
0020N58ZG2M08
0020N58ZG2M09
0020N58ZG2M0A
0020N58ZG2M0B
0020N58ZG2M0C
0020N58ZG2M0D
0020N58ZG2M0E
        ^   ^ look
                                   
|-------|---|
   time random
```

### TSID Structure

The term TSID stands for (roughly) Time Sortable ID. A TSID is a number that is formed by the creation time along with random bits.

The TSID has 3 components:

*   Time component (42 bits)
*   Node ID (0 to 20 bits)
*   Sequence (2 to 22 bits)

The time component is the count of milliseconds since 10th December 2022 08.15.00 +01:00.

The sequence bits depend on the node bits. If the node bits are 10, the sequence bits are limited to 12. In this example, the maximum node value is 2^10-1 = 1023 and the maximum sequence value is 2^12-1 = 4095. So the maximum TSIDs that can be generated per millisecond is 4096.

The node identifier uses 10 bits of the random component by default in the `TsidFactory`. It's possible to adjust the node bits to a value between 0 and 20. The sequence bits are affected by the node bits.

This is the default TSID structure:

```
                                            adjustable
                                           <---------->
|------------------------------------------|----------|------------|
       time (msecs since 2021-10-12)           node       sequence
                42 bits                       10 bits     12 bits

- time:    2^42 = ~139 years (with adjustable epoch)
- node:    2^10 = 1,024 (with adjustable bits)
- sequence: 2^12 = 4,096 (initially random)

Notes:
The node is adjustable from 0 to 20 bits.
The node bits affect the sequence bits.
The time component can be used for ~69 years if stored in a SIGNED 64 bits integer field.
```

The time component can be 1 ms or more ahead of the system time when necessary to maintain monotonicity and generation speed.

The node identifier is a random number from 0 to 1023 (default). It can be replaced by a value given to the `TsidFactory` constructor or method factory.

### More Examples

Create a TSID from a canonical string (13 chars):

```C#
var tsid = TsidFactory.From("0123456789ABC");
```

---

Convert a TSID into a canonical string in lower case:

```C#
var string = tsid.ToLowerCase(); // 0123456789abc
```

---

A `TsidFactory` with a FIXED node identifier and CUSTOM node bits:

```C#
// setup a factory for up to 64 nodes and 65536 ID/ms.
var factory = new TsidFactory(new TsidSettings
{
    Node = 83
});

// use the factory
var tsid = factory.Create();
```

---

A `TsidFactory` with a CUSTOM epoch:

```C#
// use a CUSTOM epoch that starts from the fall of the Berlin Wall
var factory = new TsidFactory(new TsidSettings
{
    CustomTsidTimeEpoch = DateTimeOffset.ParseExact("09/11/1989 00.00.00 +01:00", "dd/MM/yyyy HH.mm.ss zzz", CultureInfo.InvariantCulture)
});

// use the factory
var tsid = factory.Create();
```

---

A `TsidFactory` with node function factory:

```C#
// use a `java.util.Random` instance for fast generation
var factory = new TsidFactory(new TsidSettings
{
    NodeFactory = () => {
        ... // for example: retrieve some HW ID
    }
});

// use the factory
var tsid = factory.Create();
```

---

A `TsidFactory` that creates TSIDs similar to [Twitter Snowflakes](https://github.com/twitter-archive/snowflake):

```C#
// Twitter Snowflakes have 5 bits for datacenter ID and 5 bits for worker ID
var datacenter = 1; // max: 2^5-1 = 31
var worker = 1;     // max: 2^5-1 = 31
var node = (datacenter << 5 | worker); // max: 2^10-1 = 1023

// Twitter Epoch is fixed in 1288834974657 (2010-11-04T01:42:54.657Z)
var customEpoch = DateTimeOffset.FromUnixTimeMilliseconds(1288834974657L);

// RESET the sequence to ZERO when the millisecond changes
var sequenceResetType = SequenceResetType.OnTimeChange;

// a factory that returns TSIDs similar to Twitter Snowflakes
var factory = new TsidFactory(new TsidSettings
{
    Node = node,
    CustomTsidTimeEpoch = customEpoch,
    SequenceResetType = sequenceResetType
});

// use the factory
var tsid = factory.Create();
```

---

A `TsidFactory` that creates TSIDs similar to [Discord Snowflakes](https://discord.com/developers/docs/reference#snowflakes):

```C#
// Discord Snowflakes have 5 bits for worker ID and 5 bits for process ID
var worker = 1;  // max: 2^5-1 = 31
var process = 1; // max: 2^5-1 = 31
var node = (worker << 5 | process); // max: 2^10-1 = 1023

// Discord Epoch starts in the first millisecond of 2015
var customEpoch = DateTimeOffset.Parse("2015-01-01T00:00:00.000Z");

// RESET the sequence to ZERO when the millisecond changes
var sequenceResetType = SequenceResetType.OnTimeChange;

// a factory that returns TSIDs similar to Twitter Snowflakes
var factory = new TsidFactory(new TsidSettings
{
    Node = node,
    CustomTsidTimeEpoch = customEpoch
});

// use the factory
var tsid = factory.Create();
```