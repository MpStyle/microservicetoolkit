namespace microservice.toolkit.entitystoremanager.entity.service
{
    public class ItemUpsertRequest<TSource> where TSource : IItem
    {
        public TSource Item { get; set; }
        public bool ReturnEmptyResponse { get; set; }
    }

    public class ItemUpsertResponse
    {
        public string ItemId { get; set; }
    }
}