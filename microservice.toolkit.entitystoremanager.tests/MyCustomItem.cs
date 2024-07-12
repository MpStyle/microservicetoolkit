using microservice.toolkit.entitystoremanager.attribute;
using microservice.toolkit.entitystoremanager.entity;

namespace microservice.toolkit.entitystoremanager.tests;

[ItemTable("MyCustom")]
[Item("my-custom-item")]
public class MyCustomItem : IItem
{
    public string Id { get; set; }

    [ItemProperty("role")]
    public UserRole Role { get; set; }

    [ItemProperty("string_value")]
    public string StringValue { get; set; }

    [ItemProperty("strings_value")]
    public string[] StringsValue { get; set; }

    [ItemProperty("int_value")]
    public int IntValue { get; set; }

    [ItemProperty("ints_value")]
    public int[] IntsValue { get; set; }

    [ItemProperty("float_value")]
    public float FloatValue { get; set; }

    [ItemProperty("floats_value")]
    public float[] FloatsValue { get; set; }

    [ItemProperty("bool_value")]
    public bool BoolValue { get; set; }

    [ItemProperty("booleans_value")]
    public bool[] BooleansValue { get; set; }

    [ItemProperty("long_value")]
    public long LongValue { get; set; }

    [ItemProperty("longs_value")]
    public long[] LongsValue { get; set; }

    public bool? Enabled { get; set; }

    public long? Inserted { get; set; }

    public long? Updated { get; set; }

    public string Updater { get; set; }
    
    [ItemPropertyIgnore]
    public string IgnoredProperty { get; set; } = "ignored property value";
}