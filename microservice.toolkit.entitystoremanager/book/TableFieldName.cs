namespace microservice.toolkit.entitystoremanager.book;

internal class TableFieldName
{
    internal class Item
    {
        internal static string Type => nameof(Type);
    }
    
    internal class ItemProperty
    {
        internal static string ItemId => nameof(ItemId);
        internal static string Key => nameof(Key);
        internal static string StringValue => nameof(StringValue);
        internal static string IntValue => nameof(IntValue);
        internal static string LongValue => nameof(LongValue);
        internal static string FloatValue => nameof(FloatValue);
        internal static string BoolValue => nameof(BoolValue);
        internal static string Order => nameof(Order);
    }
}