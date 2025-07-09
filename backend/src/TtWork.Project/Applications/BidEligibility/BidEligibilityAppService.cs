using System.Threading.Tasks;
using Abp.Application.Services;
using Microsoft.AspNetCore.Mvc;
using TtWork.Project.Services;

namespace TtWork.Project.Applications.BidEligibility;

public class BidEligibilityAppService : ApplicationService, IBidEligibilityAppService
{
    private readonly IBidEligibilityService _bidEligibilityService;

    public BidEligibilityAppService(IBidEligibilityService bidEligibilityService)
    {
        _bidEligibilityService = bidEligibilityService;
    }

    /// <summary>
    /// 检查用户是否可以出价
    /// </summary>
    /// <param name="input">出价判断请求</param>
    /// <returns>出价判断结果</returns>
    [HttpPost]
    public async Task<BidEligibilityResult> CheckBidEligibility(CheckBidEligibilityInput input)
    {
        return await _bidEligibilityService.CheckBidEligibilityAsync(input);
    }

    /// <summary>
    /// 根据用户名称检查用户出价能力
    /// </summary>
    /// <param name="userName">用户名称</param>
    /// <returns>用户出价能力检查结果</returns>
    [HttpGet]
    public async Task<UserBidCapabilityResult> CheckUserBidCapability(string userName)
    {
        return await _bidEligibilityService.CheckUserBidCapabilityAsync(userName);
    }

    /// <summary>
    /// 根据用户ID检查用户出价能力
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户出价能力检查结果</returns>
    [HttpGet]
    public async Task<UserBidCapabilityResult> CheckUserBidCapabilityById(long userId)
    {
        return await _bidEligibilityService.CheckUserBidCapabilityAsync(userId);
    }
} 