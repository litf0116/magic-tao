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
using TtWork.Abp.Entity;
using TtWork.Project.Applications.GroupChatLevelSettings.Dto;
using TtWork.Project.Applications.MsgConfiguration;

namespace TtWork.Project.Web.Controllers;

/// <summary>
/// 群聊等级设置
/// </summary>
[Route("api/GroupChatLevelSettings")]
public class GroupChatLevelSettingsService : AbpAppServiceBase
{
    private readonly ISqlSugarClient _sqlSugarClient;
    public GroupChatLevelSettingsService(ISqlSugarClient sqlSugar)
    {
        _sqlSugarClient = sqlSugar;
    }
    /// <summary>
    /// 获取数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetList")]
    [AbpAuthorize]
    public async Task<List<GroupChatLevelSettingsEntity>> GetList()
    {
        try
        {
            return await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>().ToListAsync();
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"错误信息：" + ex.Message);
        }
    }
    /// <summary>
    /// 查询用户群聊等级信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetUserGroupLevel/{id}")]
    [AbpAuthorize]
    public async Task<UserGroupLevelEntity> GetUserGroupLevel(int id)
    {
        try
        {
            //查询群等级信息
            var info = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
             .Where(w => w.UserId == id)
             .FirstAsync();
            return info;
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"添加失败，错误信息：" + ex.Message);
        }
    }
    /// <summary>
    /// 添加用户群聊等级累计金额配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    [HttpPost("Add")]
    [AbpAuthorize]
    public async Task Add(UserGroupLevelDto input)
    {
        try
        {
            //查询群等级信息
            var groupChatLevelSettings = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>()
             .Where(w => w.AmountRequired <= input.CumulativeAmount)   // 找到小于等于当前累计金额的等级配置
             .OrderByDescending(o => o.AmountRequired)                       // 按金额要求降序排序，找到最接近的等级
             .FirstAsync();
            if (groupChatLevelSettings == null)
            {
                throw new UserFriendlyException($"没有匹配的群聊等级信息！");
            }
            //查询用户等级信息
            var info = await _sqlSugarClient.Queryable<UserGroupLevelEntity>().FirstAsync(f=>f.UserId==input.UserId);
            if (info != null)
            {
                info.CumulativeAmount = input.CumulativeAmount;
                info.GroupChatId = groupChatLevelSettings.Id;
                await _sqlSugarClient.Updateable(info).ExecuteCommandAsync();
            }
            else
            {
                await _sqlSugarClient.Insertable(new UserGroupLevelEntity
                {
                    UserId = input.UserId,
                    CumulativeAmount = input.CumulativeAmount,
                    GroupChatId = groupChatLevelSettings.Id,
                }).ExecuteCommandAsync();
            }
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"添加失败，错误信息：" + ex.Message);
        }
    }

    /// <summary>
    /// 添加群聊等级信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("AddGroupChatLevelSettings")]
    [AbpAuthorize]
    public async Task AddGroupChatLevelSettings(GroupChatLevelSettingsDto input)
    {
        try
        {
            await _sqlSugarClient.Insertable(new GroupChatLevelSettingsEntity
            {
                Name = input.Name,
                Level = input.Level,
                AmountRequired = input.AmountRequired,
                BorderColor = input.BorderColor,
                RightBorderColor = input.RightBorderColor,
            }).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"添加失败，错误信息：" + ex.Message);
        }
    }
    /// <summary>
    /// 修改群聊等级信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("EditGroupChatLevelSetting")]
    [AbpAuthorize]
    public async Task EditGroupChatLevelSetting(GroupChatLevelSettingsDto input)
    {
        try
        {
            var info = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>().FirstAsync(f => f.Id == input.Id);
            if (info == null)
            {
                throw new UserFriendlyException($"当前数据不存在");
            }
            await _sqlSugarClient.Updateable(new GroupChatLevelSettingsEntity
            {
                Name = input.Name,
                Level = input.Level,
                AmountRequired = input.AmountRequired,
                BorderColor = input.BorderColor,
                RightBorderColor = input.RightBorderColor
            }).Where(w=>w.Id==input.Id).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"修改失败，错误信息：" + ex.Message);
        }
    }
    /// <summary>
    /// 删除群聊等级信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("DeleteGroupChatLevelSetting/{id}")]
    [AbpAuthorize]
    public async Task DeleteGroupChatLevelSetting(int id)
    {
        try
        {
            var info = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>().FirstAsync(f => f.Id == id);
            if (info == null)
            {
                throw new UserFriendlyException($"当前数据不存在");
            }
            await _sqlSugarClient.Deleteable<GroupChatLevelSettingsEntity>().Where(w => w.Id == id).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"删除失败，错误信息：" + ex.Message);
        }
    }
}
