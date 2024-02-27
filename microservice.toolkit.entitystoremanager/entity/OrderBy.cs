namespace microservice.toolkit.entitystoremanager.entity
{
    public class OrderBy
    {
        public string Field { get; set; }
        public string Order { get; set; } = "ASC";
    }

    public static class Order
    {
        public const string Asc = "ASC";
        public const string Desc = "DESC";
    }
}
