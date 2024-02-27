using System.Collections.Generic;

namespace microservice.toolkit.entitystoremanager.entity;

internal class DbFilter
{
    internal string Condition { get; set; }
    internal Dictionary<string, object> Parameters { get; set; }
}