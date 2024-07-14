namespace microservice.toolkit.entitystoremanager.entity.service
{
    public class ItemDeleteRequest
    {
        public string Id { get; set; }
        public string ExcludeId { get; set; }
        public string[] Ids { get; set; }

        public IWhere Filters { get; set; }
    }

    public class ItemDeleteResponse
    {
        public int DeletedRows { get; set; }
    }
}