using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace TtWork.Abp.Core.Events.Queries
{
    public class QueryWebRequestHost : IRequest<string>
    {
    }

    public class QueryWebRequestHostHandle : IRequestHandler<QueryWebRequestHost, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QueryWebRequestHostHandle(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Handle(QueryWebRequestHost query, CancellationToken cancellationToken)
        {
            var request = _httpContextAccessor.HttpContext!.Request;
            return await Task.FromResult($"{request.Scheme}://{request.Host}");
        }
    }
}