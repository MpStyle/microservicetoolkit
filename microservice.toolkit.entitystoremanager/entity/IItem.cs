namespace microservice.toolkit.entitystoremanager.entity;

public interface IItem
{
    public string Id { get; set; }
    public long? Inserted { get; set; }
    public long? Updated { get; set; }
    public string Updater { get; set; }
    public bool? Enabled { get; set; }
}