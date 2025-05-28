using Abp.Authorization;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Nest;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Entity;
using TtWork.Project.Applications.GroupChatLevelSettings.Dto;

namespace TtWork.Project.Applications.AdvertisingSpace
{
    /// <summary>
    /// 广告位管理
    /// </summary>
    [Route("api/AdvertisingSpace")]
    public class AdvertisingSpaceAppService : AbpAppServiceBase
    {
        private readonly ISqlSugarClient _sqlSugarClient;
        public AdvertisingSpaceAppService(ISqlSugarClient sqlSugar)
        {
            _sqlSugarClient = sqlSugar;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetList")]
        [AbpAuthorize]
        public async Task<dynamic> GetList(AppResultRequestDto input)
        {
            try
            {
                RefAsync<int> totalCount = 0;
                var items = await _sqlSugarClient.Queryable<AdvertisingSpaceEntity>()
                    .WhereIF(!string.IsNullOrEmpty(input.Keyword), w => w.Title.Contains(input.Keyword))
                    .ToPageListAsync(input.SkipCount, input.MaxResultCount, totalCount);
                return new { totalCount = totalCount.Value, items = items };
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"错误信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 根据类型获取数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetTypeList/{type}")]
        public async Task<dynamic> GetTypeList(int type)
        {
            try
            {
                var items = await _sqlSugarClient.Queryable<AdvertisingSpaceEntity>().Where(w => w.Type == type && w.Status == 1).ToListAsync();
                return new { items };
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"错误信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        [AbpAuthorize]
        public async Task Add(AdvertisingSpaceEntity input)
        {
            try
            {
                input.CreateTime = DateTime.Now;
                await _sqlSugarClient.Insertable(input).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"添加失败，错误信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("Edit")]
        [AbpAuthorize]
        public async Task Edit(AdvertisingSpaceEntity input)
        {
            try
            {
                var info = await _sqlSugarClient.Queryable<AdvertisingSpaceEntity>().FirstAsync(f => f.Id == input.Id);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }
                await _sqlSugarClient.Updateable(input).Where(w => w.Id == input.Id).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"修改失败，错误信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 更新状态信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpGet("UpdateState/{id}/{status}")]
        [AbpAuthorize]
        public async Task UpdateState(int id, int status)
        {
            try
            {
                var info = await _sqlSugarClient.Queryable<AdvertisingSpaceEntity>().FirstAsync(f => f.Id == id);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }
                info.Status = status;
                await _sqlSugarClient.Updateable(info).Where(w => w.Id == id).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"修改失败，错误信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Delete/{id}")]
        [AbpAuthorize]
        public async Task Delete(int id)
        {
            try
            {
                var info = await _sqlSugarClient.Queryable<AdvertisingSpaceEntity>().FirstAsync(f => f.Id == id);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }
                await _sqlSugarClient.Deleteable<AdvertisingSpaceEntity>().Where(w => w.Id == id).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"删除失败，错误信息：" + ex.Message);
            }
        }
    }
}
