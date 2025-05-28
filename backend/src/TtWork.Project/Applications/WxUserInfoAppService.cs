using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Nest;
using TtWork.Abp;
using TtWork.Abp.Core;
using TtWork.Abp.DomianServices;

namespace TtWork.Project.Applications;

public class WxUserInfoAppService : AbpAppServiceBase {
    private readonly IRepository<WechatUserinfo, string> _wechatUserinfoRepository;

    public WxUserInfoAppService(IRepository<WechatUserinfo, string> wechatUserinfoRepository) {
        _wechatUserinfoRepository = wechatUserinfoRepository;
    }

    public Task<List<WechatUserinfo>> GetWechatUserinfosAsync() {
        return _wechatUserinfoRepository.GetAllListAsync();
    }
}