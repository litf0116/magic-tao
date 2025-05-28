using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using SqlSugar.DistributedSystem.Snowflake;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TtWork.Abp;
using TtWork.Abp.Entity;

namespace TtWork.Project.Applications.WithdrawalAmount
{
    /// <summary>
    /// 用户提现服务
    /// </summary>
    public class WithdrawalAmountService : AbpAppServiceBase
    {
        private readonly IAbpSession _abpSession;
        private readonly ISqlSugarClient _sqlSugarClient;
        public WithdrawalAmountService(IAbpSession abpSession, ISqlSugarClient sqlSugar)
        {
            _abpSession = abpSession;
            _sqlSugarClient = sqlSugar;
        }
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        [AbpAuthorize]
        public async Task<dynamic> Page([FromQuery] WithdrawalDto input)
        {
            int totalCount = 0;
            var list = await _sqlSugarClient.Queryable<UserInfoEntity, WithdrawalAmountEntity>((u, w) =>
            new JoinQueryInfos(JoinType.Inner, u.Id == w.UserId))
            .WhereIF(!string.IsNullOrEmpty(input.SearchValue), (u, w) => u.Name.Contains(input.SearchValue))
            .Select((u, w) => new WithdrawalOutput
            {
                Id = u.Id,
                Name = u.Name,
                Amount = w.Amount,
                Status = w.Status,
                WithdrawalTime = w.WithdrawalTime
            }).ToPageListAsync(input.PageNo, input.PageSize, totalCount);
            foreach (var item in list)
            {
                //审核状态 1 审核中 2 拒绝 3 审核通过
                switch (item.Status)
                {
                    case 1: item.StatusStr = "审核中"; break;
                    case 2: item.StatusStr = "拒绝"; break;
                    case 3: item.StatusStr = "审核通过"; break;
                }
            }
            return new { item = list, pageNo = input.PageNo, totalCount = totalCount };
        }
        /// <summary>
        /// 审核提现
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize]
        public async Task ApproveWithdrawal(ApproveInfo approve)
        {
            try
            {
                //查询数据
                var info = await _sqlSugarClient.Queryable<WithdrawalAmountEntity>().Where(w => w.Id == approve.Id).FirstAsync();
                if (info == null)
                {
                    throw new UserFriendlyException($"提现数据不存在！");
                }
                //审核状态 1 审核中 2 拒绝 3 审核通过
                if (info.Status != 1)
                {
                    throw new UserFriendlyException($"当前提现数据审核状态不是审核中！");
                }
                info.Status = approve.Status;
                //更新数据
                await _sqlSugarClient.Updateable(info).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"提现审核出错，错误信息：" + ex.Message);
            }
        }

    }

    /// <summary>
    /// 提现信息
    /// </summary>
    public class WithdrawalDto()
    {
        /// <summary>
        /// 搜索值
        /// </summary>
        public virtual string SearchValue { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public virtual int PageNo { get; set; } = 1;

        /// <summary>
        /// 页码容量
        /// </summary>
        public virtual int PageSize { get; set; } = 20;

    }
    /// <summary>
    /// 
    /// </summary>
    public class WithdrawalOutput()
    {
        public int Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 提交金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 提现时间
        /// </summary>
        public DateTime WithdrawalTime { get; set; }
        /// <summary>
        /// 审核状态 1 审核中 2 拒绝 3 审核通过
        /// </summary>
        public int Status { get; set; }

        public string StatusStr { get; set; }

    }
    /// <summary>
    /// 审核信息
    /// </summary>
    public class ApproveInfo
    {
        /// <summary>
        /// 提现记录编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 审核状态 1 审核中 2 拒绝 3 审核通过
        /// </summary>
        public int Status { get; set; }
    }
}
