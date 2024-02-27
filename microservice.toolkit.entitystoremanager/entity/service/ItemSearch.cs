namespace microservice.toolkit.entitystoremanager.entity.service
{
    public class ItemSearchRequest
    {
        public string Id { get; set; }
        public string ExcludeId { get; set; }
        public string[] Ids { get; set; }

        public IWhere Filters { get; set; }

        public OrderBy[] OrderBy { get; set; }
        public bool IncludeDisabled { get; set; }
        public int? PageSize { get; set; }
        public int? Page { get; set; }
        public bool ReturnOnlyId { get; set; }
    }

    public class ItemSearchResponse<TSource> where TSource : IItem, new()
    {
        public TSource[] Items { get; set; }
        public string[] ItemIds { get; set; }
    }
}