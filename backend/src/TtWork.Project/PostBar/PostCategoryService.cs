using Abp.Authorization;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Entity;

namespace TtWork.Project.PostBar
{
    /// <summary>
    /// 帖子类型
    /// </summary>
    [Route("api/PostCategory")]
    public class PostCategoryService : AbpAppServiceBase
    {
        private readonly ISqlSugarClient _sqlSugarClient;
        public PostCategoryService(ISqlSugarClient sqlSugar)
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
                var items = await _sqlSugarClient.Queryable<tb_postCategory>()
                    .WhereIF(!string.IsNullOrEmpty(input.Keyword), w => w.name.Contains(input.Keyword))
                    .ToPagedListAsync(input.SkipCount, input.MaxResultCount);
                return new { totalCount = items.TotalCount, items = items.Items };
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
        [HttpGet("GetCategoryList")]
        public async Task<dynamic> GetList()
        {
            try
            {
                var items = await _sqlSugarClient.Queryable<tb_postCategory>().ToListAsync();
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
        public async Task Add(tb_postCategory input)
        {
            try
            {
                await _sqlSugarClient.Insertable(input)
                    .IgnoreColumns(it => new { it.createdAt, it.updatedAt }).ExecuteCommandAsync();
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
        public async Task Edit(tb_postCategory input)
        {
            try
            {
                var info = await _sqlSugarClient.Queryable<tb_postCategory>().FirstAsync(f => f.categoryId == input.categoryId);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }
                await _sqlSugarClient.Updateable(input).Where(w => w.categoryId == input.categoryId).ExecuteCommandAsync();
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
                var info = await _sqlSugarClient.Queryable<tb_postCategory>().FirstAsync(f => f.categoryId == id);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }
                info.status = status;
                await _sqlSugarClient.Updateable(info).Where(w => w.categoryId == id).ExecuteCommandAsync();
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
                var info = await _sqlSugarClient.Queryable<tb_postCategory>().FirstAsync(f => f.categoryId == id);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }
                await _sqlSugarClient.Deleteable<tb_postCategory>().Where(w => w.categoryId == id).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"删除失败，错误信息：" + ex.Message);
            }
        }
    }
}
