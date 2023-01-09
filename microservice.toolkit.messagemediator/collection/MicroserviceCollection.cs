using microservice.toolkit.core.extension;

using System;
using System.Collections.Generic;
using System.Linq;

namespace microservice.toolkit.messagemediator.collection;

public class MicroserviceCollection
{
    private readonly Dictionary<string, Type> services = new Dictionary<string, Type>();

    public int Count => this.services.Count;

    public Type this[string pattern]
    {
        get => this.services[pattern];
    }

    internal MicroserviceCollection(Dictionary<string, Type> services)
    {
        this.services = services;
    }

    public bool ContainsPattern(string pattern)
    {
        return this.services.ContainsKey(pattern);
    }

    public Type ByPatternOrDefault(string pattern)
    {
        return this.ContainsPattern(pattern) ? this.services[pattern] : null;
    }

    public Dictionary<string, Type> ToDictionary()
    {
        return services;
    }
}

static class MicroserviceCollectionExtensions
{
    public static MicroserviceCollection ToMicroserviceCollection(this IEnumerable<Type> collections)
    {
        return new MicroserviceCollection(collections.ToDictionary(x => x.ToPattern(), x => x));
    }

    public static MicroserviceCollection ToMicroserviceCollection(this IEnumerable<MicroserviceCollection> collections)
    {
        return new MicroserviceCollection(collections.SelectMany(c => c.ToDictionary()).ToDictionary(x => x.Key, x => x.Value));
    }
}
