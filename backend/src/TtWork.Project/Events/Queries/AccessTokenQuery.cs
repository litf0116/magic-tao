using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TtWork.Abp.DomianServices.Weixin;
using TtWork.Lib;

namespace TtWork.Project.Events.Queries {
    public class AccessTokenQuery : IRequest<string> {
        public string Appid { get; }
        public string Appsec { get; }

        public AccessTokenQuery(string appid, string appsec) {
            Appid = appid;
            Appsec = appsec;
        }

        public class AccessTokenQueryHandle : IRequestHandler<AccessTokenQuery, string> {
            private readonly WeixinManger _weixinManger;

            public AccessTokenQueryHandle(WeixinManger weixinManger) {
                _weixinManger = weixinManger;
            }

            public async Task<string> Handle(AccessTokenQuery request, CancellationToken cancellationToken) {
                if (request.Appid.IsNullOrEmptyOrWhiteSpace() || request.Appsec.IsNullOrEmptyOrWhiteSpace())
                    return null;

                var result = await _weixinManger.GetAccessTokenAsync(request.Appid, request.Appsec);
                return result;
            }
        }
    }
}