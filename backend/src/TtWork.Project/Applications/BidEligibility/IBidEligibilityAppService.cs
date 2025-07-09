using System.Threading.Tasks;
using Abp.Application.Services;
using TtWork.Project.Services;

namespace TtWork.Project.Applications.BidEligibility;

public interface IBidEligibilityAppService : IApplicationService
{
    /// <summary>
    /// 检查用户是否可以出价
    /// </summary>
    /// <param name="input">出价判断请求</param>
    /// <returns>出价判断结果</returns>
    Task<BidEligibilityResult> CheckBidEligibility(CheckBidEligibilityInput input);

    /// <summary>
    /// 根据用户名称检查用户出价能力
    /// </summary>
    /// <param name="userName">用户名称</param>
    /// <returns>用户出价能力检查结果</returns>
    Task<UserBidCapabilityResult> CheckUserBidCapability(string userName);

    /// <summary>
    /// 根据用户ID检查用户出价能力
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户出价能力检查结果</returns>
    Task<UserBidCapabilityResult> CheckUserBidCapabilityById(long userId);
} 