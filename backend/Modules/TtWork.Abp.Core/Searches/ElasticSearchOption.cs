namespace TtWork.Abp.Core.Searches
{
    public class ElasticSearchOption
    {
        public string Uri { get; set; } = "http://127.0.1.1:9200";

        public int Timeout { get; set; } = 60;

        public string IndexName { get; set; } = "DefaultIndex";
    }
}