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
using System.Drawing.Printing;

namespace TtWork.Project.PostBar
{
    /// <summary>
    /// 帖子管理
    /// </summary>
    [Route("api/Post")]
    public class PostService : AbpAppServiceBase
    {
        private readonly ISqlSugarClient _sqlSugarClient;
        private readonly IAbpSession _abpSession;

        public PostService(ISqlSugarClient sqlSugar, IAbpSession abpSession)
        {
            _sqlSugarClient = sqlSugar;
            _abpSession = abpSession;
        }

        /// <summary>
        /// 后台获取数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAdminList")]
        [AbpAuthorize]
        public dynamic GetAdminList(AppResultRequestDto input)
        {
            try
            {
                int totalCount = 0;
                var items = _sqlSugarClient.Queryable<tb_post>()
                    .LeftJoin<UserInfoEntity>((a, c) => a.userId == c.Id)
                    .WhereIF(input.Status.HasValue, (a, c) => a.status == input.Status.Value)
                    .WhereIF(!string.IsNullOrEmpty(input.Keyword),
                        (a, c) => a.title.Contains(input.Keyword) || a.content.Contains(input.Keyword))
                    .Select((a, c) => new PostDto
                    {
                        title = a.title,
                        content = a.content,
                        postId = a.postId,
                        createdAt = a.createdAt,
                        isEssence = a.isEssence,
                        categoryId = a.categoryId,
                        isTop = a.isTop,
                        likeCount = a.likeCount,
                        replyCount = a.replyCount,
                        viewCount = a.viewCount,
                        userId = a.userId,
                        userName = c.Name,
                        userAvatar = c.HeadImgUrl,
                    })
                    .OrderByDescending(a => a.postId) // 最后按帖子ID降序
                    .ToPageList(input.SkipCount, input.MaxResultCount, ref totalCount);
                //查询帖子类型
                var categoryList = _sqlSugarClient.Queryable<tb_postCategory>().ToList();
                //处理数据
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.categoryId))
                    {
                        //查询帖子类型
                        var categoryId = item.categoryId.Split(',').ToList();
                        var category = categoryList.Where(w => categoryId.Contains(w.categoryId.ToString())).ToList();
                        if (category.Count > 0)
                        {
                            item.categoryName = string.Join(",", category.Select(s => s.name));
                        }
                    }
                }

                return new { totalCount = totalCount, items = items };
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"错误信息：" + ex.Message);
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetList")]
        public async Task<dynamic> GetList(AppResultRequestDto input)
        {
            try
            {
                var items = await _sqlSugarClient.Queryable<tb_post>()
                    .LeftJoin<UserInfoEntity>((a, c) => a.userId == c.Id)
                    .Where(a => a.status == 1) // 只返回正常状态的帖子 (1=正常, 2=关闭, 3=删除)
                    .WhereIF(!string.IsNullOrEmpty(input.Keyword),
                        (a, c) => a.title.Contains(input.Keyword) || a.content.Contains(input.Keyword))
                    .WhereIF(input.Type != -1, (a, c) => a.categoryId.Contains(input.Type.ToString()))
                    .WhereIF(input.IsTop.HasValue, (a, c) => a.isTop == input.IsTop.Value) // New filter condition
                    .Select((a, c) => new PostDto
                    {
                        title = a.title,
                        content = a.content,
                        postId = a.postId,
                        createdAt = a.createdAt,
                        isEssence = a.isEssence,
                        categoryId = a.categoryId,
                        isTop = a.isTop,
                        likeCount = a.likeCount,
                        replyCount = a.replyCount,
                        viewCount = a.viewCount,
                        userId = a.userId,
                        userName = c.Name,
                        userAvatar = c.HeadImgUrl,
                    })
                    // .OrderByDescending(a => a.isTop)         // 先按置顶降序
                    .OrderByDescending(a => a.isEssence) // 再按精华降序
                    .OrderByDescending(a => a.postId) // 最后按帖子ID降序
                    .ToPagedListAsync(input.SkipCount, input.MaxResultCount);
                //查询帖子类型
                var categoryList = _sqlSugarClient.Queryable<tb_postCategory>().ToList();
                //处理数据
                foreach (var item in items.Items)
                {
                    if (!string.IsNullOrEmpty(item.categoryId))
                    {
                        //查询帖子类型
                        var categoryId = item.categoryId.Split(',').ToList();
                        var category = categoryList.Where(w => categoryId.Contains(w.categoryId.ToString())).ToList();
                        if (category.Count > 0)
                        {
                            item.categoryName = string.Join(",", category.Select(s => s.name));
                        }
                    }
                }

                return new { totalCount = items.TotalCount, items = items.Items, HasNextPages = items.HasNextPages };
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"错误信息：" + ex.Message);
            }
        }

        /// <summary>
        /// 获取数据详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("PostDetail/{id}")]
        [AbpAuthorize]
        public async Task<dynamic> PostDetail(int id)
        {
            try
            {
                //查询详情
                var info = await _sqlSugarClient.Queryable<tb_post>()
                    .LeftJoin<UserInfoEntity>((a, c) => a.userId == c.Id)
                    .Where((a, c) => a.postId == id)
                    .Select((a, c) => new PostDto
                    {
                        title = a.title,
                        content = a.content,
                        postId = a.postId,
                        categoryId = a.categoryId,
                        createdAt = a.createdAt,
                        isEssence = a.isEssence,
                        isTop = a.isTop,
                        likeCount = a.likeCount,
                        replyCount = a.replyCount,
                        viewCount = a.viewCount,
                        userId = a.userId,
                        userName = c.Name,
                        userAvatar = c.HeadImgUrl,
                        wechat = c.wx,
                        qq = c.qq,
                        LastModifierUserId = c.Id
                    }).FirstAsync();
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }

                if (!string.IsNullOrEmpty(info.categoryId))
                {
                    //查询帖子类型
                    var categoryId = info.categoryId.Split(',').ToList();
                    var categoryList = _sqlSugarClient.Queryable<tb_postCategory>()
                        .Where(w => categoryId.Contains(w.categoryId.ToString())).ToList();
                    if (categoryList.Count > 0)
                    {
                        info.categoryName = string.Join(",", categoryList.Select(s => s.name));
                    }
                }

                return info;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"删除失败，错误信息：" + ex.Message);
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        [AbpAuthorize]
        public async Task Add(tb_post input)
        {
            try
            {
                input.userId = _abpSession.UserId.Value;
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
        public async Task Edit(tb_post input)
        {
            try
            {
                var info = await _sqlSugarClient.Queryable<tb_post>().FirstAsync(f => f.postId == input.postId);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }

                await _sqlSugarClient.Updateable(input)
                    .IgnoreColumns(it => new { it.createdAt, it.updatedAt })
                    .Where(w => w.postId == input.postId).ExecuteCommandAsync();
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
                var info = await _sqlSugarClient.Queryable<tb_post>().FirstAsync(f => f.postId == id);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }

                await _sqlSugarClient.Deleteable<tb_post>().Where(w => w.postId == id).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"删除失败，错误信息：" + ex.Message);
            }
        }

        /// <summary>
        /// 设置置顶帖
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [HttpGet("SetTop/{id}")]
        [AbpAuthorize]
        public async Task SetTop(int id)
        {
            try
            {
                var info = await _sqlSugarClient.Queryable<tb_post>().FirstAsync(f => f.postId == id);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }

                info.isTop = !info.isTop;
                await _sqlSugarClient.Updateable<tb_post>(info).Where(w => w.postId == id).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"置顶失败，错误信息：" + ex.Message);
            }
        }

        /// <summary>
        /// 设置精华帖
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [HttpGet("SetEssence/{id}")]
        [AbpAuthorize]
        public async Task SetEssence(int id)
        {
            try
            {
                var info = await _sqlSugarClient.Queryable<tb_post>().FirstAsync(f => f.postId == id);
                if (info == null)
                {
                    throw new UserFriendlyException($"当前数据不存在");
                }

                info.isEssence = !info.isEssence;
                await _sqlSugarClient.Updateable<tb_post>(info).Where(w => w.postId == id).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"设置精华帖失败，错误信息：" + ex.Message);
            }
        }
    }
}