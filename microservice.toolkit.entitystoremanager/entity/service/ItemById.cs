namespace microservice.toolkit.entitystoremanager.entity.service
{
    public class ItemByIdRequest
    {
        public string ItemId { get; set; }
        public bool ReturnOnlyId { get; set; }
    }

    public class ItemByIdResponse<TSource> where TSource : IItem
    {
        public TSource Item { get; set; }
        public string ItemId { get; set; }
    }
}
