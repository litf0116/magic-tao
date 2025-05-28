using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TtWork.Abp;
using TtWork.Abp.Entity;

namespace TtWork.Project.Applications.MsgConfiguration
{
    /// <summary>
    /// 消息配置服务
    /// </summary>
    [Route("api/MsgConfiguration")]
    public class MsgConfigurationService : AbpAppServiceBase
    {
        private readonly ISqlSugarClient _sqlSugarClient;
        public MsgConfigurationService(ISqlSugarClient sqlSugar)
        {
            _sqlSugarClient = sqlSugar;
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [HttpGet("GetList")]
        [AbpAuthorize]
        public async Task<List<MsgConfigurationEntity>> GetList()
        {
            try
            {
                return await _sqlSugarClient.Queryable<MsgConfigurationEntity>().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"添加消息配置失败，错误信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 添加消息配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [HttpPost("Add")]
        [AbpAuthorize]
        public async Task Add(List<MsgConfigurationInput> input)
        {
            try
            {
                var list = new List<MsgConfigurationEntity>();
                foreach (var item in input)
                {
                    list.Add(new MsgConfigurationEntity
                    {
                        Id = item.Id,
                        Type = item.Type,
                        Msg = item.Msg,
                    });
                }
                if (list.Where(w => w.Id != 0).Count() > 0)
                {
                    await _sqlSugarClient.Updateable(list).ExecuteCommandAsync();
                }
                else
                {
                    await _sqlSugarClient.Insertable(list).ExecuteCommandAsync();
                }


            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"添加消息配置失败，错误信息：" + ex.Message);
            }
        }
    }

    public class MsgConfigurationInput
    {
        public int Id { get; set; }
        /// <summary>
        /// 类型 1、新用户出价提示 2、提现提示
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }
    }
}
