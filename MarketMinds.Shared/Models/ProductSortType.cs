namespace MarketMinds.Shared.Models
{
    public class ProductSortType
    {
        private static int nextId = 1;
        public int Id { get; set; }
        public string ExternalAttributeFieldTitle { get; set; } = string.Empty;
        public string InternalAttributeFieldTitle { get; set; } = string.Empty;
        public bool IsAscending { get; set; }

        public ProductSortType(string externalAttributeFieldTitle, string internalAttributeFieldTitle, bool isAscending)
        {
            this.Id = nextId++;
            this.ExternalAttributeFieldTitle = externalAttributeFieldTitle;
            this.InternalAttributeFieldTitle = internalAttributeFieldTitle;
            this.IsAscending = isAscending;
        }

        public string GetDisplayTitle()
        {
            return "Sort by " + this.ExternalAttributeFieldTitle + " (" + (this.IsAscending ? "Ascending" : "Descending") + ")";
        }

        public string GetSqlOrderByStatement()
        {
            return "ORDER BY " + this.InternalAttributeFieldTitle + (this.IsAscending ? " ASC" : " DESC");
        }
    }
}
