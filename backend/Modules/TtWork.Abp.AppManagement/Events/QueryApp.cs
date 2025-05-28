using System.Threading;
using System.Threading.Tasks;
using Abp.Extensions;
using Abp.UI;
using Castle.Core.Internal;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TtWork.Abp.AppManagement.Applications.TT.Abp.AppManagement.Application;
using TtWork.Abp.AppManagement.Apps;
using TtWork.Abp.Core.Extensions;
using TtWork.Abp.Extensions;

namespace TtWork.Abp.AppManagement.Events {
    public class QueryApp : IRequest<AppDto> {
        public string AppName { get; set; }
        public bool FromHeader { get; set; }
        public bool ThrowError { get; }

        public QueryApp(string appName = "", bool fromHeader = true, bool throwError = true) {
            AppName = appName;
            FromHeader = fromHeader;
            ThrowError = throwError;
        }

        public QueryApp(string appName) {
            AppName = appName;
            FromHeader = false;
        }

        public class QueryAppHandle(
            IHttpContextAccessor httpContextAccessor,
            IAppProvider appProvider,
            ILogger<QueryAppHandle> logger)
            : IRequestHandler<QueryApp, AppDto> {
            public async Task<AppDto> Handle(QueryApp request, CancellationToken cancellationToken) {
                if (request.FromHeader)
                    request.AppName = httpContextAccessor.Get_AppName();

                if (request.AppName.IsNullOrEmpty() && request.ThrowError) {
                    throw new UserFriendlyException("获取APP失败!!");
                }

                var appValues = await appProvider.GetOrNullAsync(request.AppName);

                return appValues != null
                    ? new AppDto {
                        Value = appValues,
                        Name = request.AppName
                    }
                    : null;
            }
        }
    }
}