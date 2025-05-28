using System;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;

namespace TtWork.Abp.Core.Searches
{
    public interface ISearchClient
    {
        ElasticLowLevelClient LowLevelClient { get; }
    }

    public class SearchClient : ISearchClient
    {
        public ElasticLowLevelClient LowLevelClient { get; }

        public SearchClient(IOptionsSnapshot<ElasticSearchOption> optionsAccessor)
        {
            var settings = new ConnectionConfiguration(new Uri(optionsAccessor.Value.Uri))
                .RequestTimeout(TimeSpan.FromSeconds(optionsAccessor.Value.Timeout));

            LowLevelClient = new ElasticLowLevelClient(settings);
        }
    }  
    
    
    
    
    
    
}