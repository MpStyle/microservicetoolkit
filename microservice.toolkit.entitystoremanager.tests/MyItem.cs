using microservice.toolkit.entitystoremanager.entity;

namespace entity.sql.tests;

public class MyItem:IItem 
{
    public string Id { get; set; }
    public UserRole Role { get; set; }
    public string StringValue { get; set; }
    public string[] StringsValue { get; set; }
    public int IntValue { get; set; }
    public int[] IntsValue { get; set; }
    public float FloatValue { get; set; }
    public float[] FloatsValue { get; set; }
    public bool BoolValue { get; set; }
    public bool[] BooleansValue { get; set; }
    public long LongValue { get; set; }
    public long[] LongsValue { get; set; }
    public bool? Enabled { get; set; }
    public long? Inserted { get; set; }
    public long? Updated { get; set; }

    public string Updater { get; set; }
}

public enum UserRole
{
    User = 20,
    SystemAdministrator = 90
}