namespace microservice.toolkit.entitystoremanager.entity.service
{
    public class ItemBulkUpsertRequest<TSource> where TSource : IItem
    {
        public TSource[] Items { get; set; }
        public bool ReturnEmptyResponse { get; set; }
    }

    public class ItemBulkUpsertResponse
    {
        public string[] ItemIds { get; set; }
    }
}