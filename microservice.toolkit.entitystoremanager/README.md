# Entity Store Manager

[![Build](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/build.yml)
[![Release](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml/badge.svg)](https://github.com/MpStyle/microservicetoolkit/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/dt/microservice.toolkit.entitystoremanager)
![Nuget](https://img.shields.io/nuget/v/microservice.toolkit.entitystoremanager)

The Entity Store Manager allows you to have the flexibility of a no-sql database with the power of a relational database. \
You will no longer have to worry about keeping your database updated with complex queries. \
Save, delete, and search any entity (data models) in a database without writing a query. Leveraging the flexibility of the microservice pattern with asynchronous and decoupled communication.

## How to install

### Package Manager
```
Install-Package microservice.toolkit.entitystoremanager -Version 1.0.0
```

### .NET CLI
```
dotnet add package microservice.toolkit.entitystoremanager --version 1.0.0
```

### Package Reference
```
<PackageReference Include="microservice.toolkit.entitystoremanager" Version="1.0.0" />
```

## Supported databases
- Microsoft SQL Server

## Before start

Migrate DB

## How to use



### Upsert

The `SqlServerItemUpsert` service provides a way to either insert a new item into the database or update it if it already exists. This document outlines a minimal example of how to use this service. \
Ensure your project is set up with a connection to a SQL Server database and that you have the necessary models and services configured. The `SqlServerItemUpsert` service requires an instance of `IDbConnection` to interact with the database. \

```csharp

class MyEntity : IItem
{
    public Guid Id { get; set; }
    public int IntValue { get; set; }
    public string StringValue { get; set; }
    public bool Enabled { get; set; }
    public long Inserted { get; set; }
    public long Updated { get; set; }
    public string Updater { get; set; }
}

var entity = new MyEntity
{
    Id = Guid.NewGuid(),
    IntValue = 1,
    StringValue = "Entity Description",
    Enabled = true,
    Updater = "ExampleUpdater",
    Inserted = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    Updated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
};

var myUpsertService = SqlServerItemUpsert<MyEntity>();
var result = await myUpsertService.Run(new ItemUpsertRequest<MyEntity>
{
    Item = entity
});
```

### Search

The `SqlServerItemSearch` service allows for advanced searches on stored items using various filters and conditions. Below is a minimal example of using this service to search for items based on specific criteria. \
Ensure you have a database connection configured and accessible from your service. The `SqlServerItemSearch` service requires an instance of `IDbConnection` to access the database.

### Code Example

```csharp
using microservice.toolkit.connectionmanager;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.service.sqlserver;
using System;
using System.Threading.Tasks;

[...]

// Creating an instance of the SqlServerItemSearch service
var itemSearchService = new SqlServerItemSearch<MyItem>(yourDbConnection);

// Defining the search request
var searchRequest = new ItemSearchRequest
{
    Filters = new AndWhere
    {
        Conditions = new List<Where>
        {
            new Where { Key = nameof(MyItem.IntValue), Value = 1 },
            new Where { Key = nameof(MyItem.StringValue), Value = "test" }
        }
    }
};

// Executing the search
var searchResponse = await itemSearchService.Run(searchRequest);

// Handling the response
if (!searchResponse.Error.HasValue)
{
    foreach (var item in searchResponse.Payload.Items)
    {
        Console.WriteLine($"Item ID: {item.Id}");
    }
}
else
{
    Console.WriteLine("Error during the search for items.");
}
```

### Count

The `SqlServerItemCount` service enables you to count items in the database based on specific criteria, such as filters on item properties. This guide provides a minimal example of how to use this service. \
Ensure your project is configured with a connection to a SQL Server database. The `SqlServerItemCount` service requires an instance of `DbConnection` to interact with the database.

```csharp
using System.Data.Common;
using microservice.toolkit.connectionmanager;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.service.sqlserver;
using System.Threading.Tasks;

[...]

var itemCountService = new SqlServerItemCount<MyItem>(dbConnection);

var countRequest = new ItemCountRequest
{
    Filters = new List<Where>
    {
        new Where { Key = nameof(MyItem.Enabled), Value = true }
    }
};

var response = await itemCountService.Run(countRequest);

if (!response.Error.HasValue)
{
    return response.Payload.Counter;
}
else
{
    Console.WriteLine($"Error counting items: {response.Error.Value}");
    return -1;
}
```

### Search by id

The `SqlServerItemById` service is designed to retrieve a single item from the database by its ID. This section provides a minimal example of how to use this service in your project. \
Ensure your project is configured with a connection to a SQL Server database. The `SqlServerItemById` service requires an instance of `DbConnection` to interact with the database.
    
```csharp
using System.Data.Common;
using microservice.toolkit.connectionmanager;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.service.sqlserver;
using System.Threading.Tasks;

[...]

var itemByIdService = new SqlServerItemById<MyItem>(dbConnection);

var response = await itemByIdService.Run(new ItemByIdRequest
{
    ItemId = itemId
});

if (!response.Error.HasValue)
{
    return response.Payload.Item;
}
else
{
    Console.WriteLine($"Error retrieving item: {response.Error.Value}");
    return null;
}
```

### Bulk Upsert

The `SqlServerItemBulkUpsert` service allows for the bulk insertion or update of items in a SQL Server database. This can significantly improve performance when dealing with large datasets. Below is a minimal example of how to use this service. \
Ensure your project is configured with a connection to a SQL Server database. The `SqlServerItemBulkUpsert` service requires an instance of `DbConnection` to interact with the database.

```csharp
using System.Data.Common;
using microservice.toolkit.connectionmanager;
using microservice.toolkit.entitystoremanager.entity;
using microservice.toolkit.entitystoremanager.service.sqlserver;
using System.Collections.Generic;
using System.Threading.Tasks;

[...]

var customService = new SqlServerItemBulkUpsert<MyCustomItem>(dbConnection);

var itemsToUpsert = new List<MyCustomItem>
{
    new MyCustomItem { Id = "1", Enabled = true },
    new MyCustomItem { Id = "2", Enabled = false }
    // Add more items as needed
};

await customService.Run(new ItemBulkUpsertRequest<MyCustomItem> { Items = itemsToUpsert });

Console.WriteLine("Bulk upsert completed.");
```

### Delete

## Attributes

### ItemAttribute

The `ItemAttribute` is a custom attribute in C# that can be used to annotate classes representing items in a database. It allows you to specify a name for the item that can be different from the class name. This is particularly useful in scenarios where your entity names do not directly match your C# class names.

```csharp
[Item("Product")]
public class MyItem : IItem
{
    [...]
}
```

The entity will be saved into database with `type` column as `Product`.

### ItemPropertyAttribute

The `ItemPropertyAttribute` is a custom attribute that can be used to map class properties using a custom name. 

```csharp
using microservice.toolkit.entitystoremanager.attribute;

public class User : IItem
{
    public string Id { get; set; }
    
    [ItemProperty("user_name")]
    public string Name { get; set; }
    
    [ItemProperty("user_email")]
    public string Email { get; set; }
    
    [...]
}
```

The `ItemProperty` attribute will be ignored for properties of the `IItem` interface (Id, Enabled, Inserted, Updated, Updater).

### ItemPropertyIgnoreAttribute

### ItemTableAttribute