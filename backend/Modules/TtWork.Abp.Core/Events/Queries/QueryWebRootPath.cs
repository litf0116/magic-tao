using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Hosting;

namespace TtWork.Abp.Core.Events.Queries
{
    public class QueryWebRootPath : IRequest<string>
    {
    }

    public class QueryWebRootPathHandle : IRequestHandler<QueryWebRootPath, string>
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public QueryWebRootPathHandle(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public async Task<string> Handle(QueryWebRootPath request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_hostEnvironment.WebRootPath);
        }
    }
}