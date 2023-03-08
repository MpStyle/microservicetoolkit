namespace microservice.toolkit.entitystoremanager.entity.service
{
    public class ItemCountRequest
    {
        public string Id { get; set; }
        public string[] Ids { get; set; }
        public IWhere[] Filters { get; set; }
        public bool IncludeDisabled { get; set; }
    }

    public class ItemCountResponse
    {
        public long Counter { get; set; }
    }
}
