using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Nest;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Entity;
using TtWork.Project.PostBar.Dto;
using TtWork.Abp.Core;

namespace TtWork.Project.PostBar
{
    /// <summary>
    /// 帖子公告
    /// </summary>
    [Route("api/PostBulletin")]
    public class PostBulletinService : AbpAppServiceBase
    {
        private readonly ISqlSugarClient _sqlSugarClient;
        private readonly IAbpSession _abpSession;
        public PostBulletinService(ISqlSugarClient sqlSugar, IAbpSession abpSession)
        {
            _sqlSugarClient = sqlSugar;
            _abpSession = abpSession;
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
                var items = await _sqlSugarClient.Queryable<tb_postBulletinEntity>()
                   .WhereIF(!string.IsNullOrEmpty(input.Keyword), (a) => a.Title.Contains(input.Keyword))
                   .OrderByDescending((a) =>a.Id)
                   .ToPagedListAsync(input.SkipCount, input.MaxResultCount);
                return new { totalCount = items.TotalCount, items = items.Items };
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"错误信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 获取最新数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLatestBulletin")]
        public async Task<dynamic> GetLatestBulletin()
        {
            try
            {
                var items = await _sqlSugarClient.Queryable<tb_postBulletinEntity>()
                   .OrderByDescending((a) => a.Id)
                   .FirstAsync();
                return items;
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
        public async Task Add(tb_postBulletinEntity input)
        {
            try
            {
                await _sqlSugarClient.Insertable(input)
                    .IgnoreColumns(it => new { it.CreateTime}).ExecuteCommandAsync();
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
        public async Task Edit(tb_postBulletinEntity input)
        {
            try
            {
                var info = await _sqlSugarClient.Queryable<tb_postBulletinEntity>().FirstAsync(f => f.Id == input.Id);
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
                var info = await _sqlSugarClient.Queryable<tb_postBulletinEntity>().FirstAsync(f => f.Id == id);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }
                await _sqlSugarClient.Deleteable<tb_postBulletinEntity>().Where(w => w.Id == id).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"删除失败，错误信息：" + ex.Message);
            }
        }
    }
}
