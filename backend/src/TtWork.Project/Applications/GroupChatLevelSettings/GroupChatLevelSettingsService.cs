using Abp.Authorization;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
using TtWork.Project.Caches;

namespace TtWork.Project.Web.Controllers;

/// <summary>
/// 群聊等级设置
/// </summary>
[Route("api/GroupChatLevelSettings")]
public class GroupChatLevelSettingsService : AbpAppServiceBase
{
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly GroupChatLevelCacheService _levelCacheService;

    public GroupChatLevelSettingsService(
        ISqlSugarClient sqlSugar,
        GroupChatLevelCacheService levelCacheService)
    {
        _sqlSugarClient = sqlSugar;
        _levelCacheService = levelCacheService;
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
            var groupChatLevelSettings = _levelCacheService.GetCorrectLevel(input.CumulativeAmount);
            if (groupChatLevelSettings == null)
            {
                throw new UserFriendlyException($"没有匹配的群聊等级信息！");
            }

            var info = await _sqlSugarClient.Queryable<UserGroupLevelEntity>().FirstAsync(f => f.UserId == input.UserId);
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

            _levelCacheService.InvalidateCache();
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

            _levelCacheService.InvalidateCache();
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

            _levelCacheService.InvalidateCache();
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"删除失败，错误信息：" + ex.Message);
        }
    }

    /// <summary>
    /// 获取用户等级信息（包含等级配置）
    /// 兼容性接口：同时支持新旧版本小程序
    /// 新版本小程序使用 levelResponse.userLevel 和 levelResponse.levelSettings 获取数据
    /// 旧版本小程序使用 levelResponse.data.userLevel 和 levelResponse.data.levelSettings 获取数据
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户等级信息和等级配置</returns>
    [HttpGet("GetUserLevelInfo/{id}")]
    [AbpAuthorize]
    public async Task<UserLevelInfoDto> GetUserLevelInfo(int id)
    {
        try
        {
            var userLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                .Where(w => w.UserId == id)
                .FirstAsync();

            if (userLevel == null)
            {
                var defaultLevel = _levelCacheService.GetCorrectLevel(0);
                return new UserLevelInfoDto
                {
                    UserLevel = new UserGroupLevelEntity
                    {
                        UserId = id,
                        CumulativeAmount = 0,
                        GroupChatId = defaultLevel?.Id ?? 0
                    },
                    LevelSettings = defaultLevel ?? new GroupChatLevelSettingsEntity
                    {
                        Level = 0,
                        Name = "普通用户",
                        AmountRequired = 0,
                        BorderColor = "#000000",
                        RightBorderColor = "#000000"
                    }
                };
            }

            var correctSettings = _levelCacheService.GetCorrectLevel(userLevel.CumulativeAmount);

            if (correctSettings != null && userLevel.GroupChatId != correctSettings.Id)
            {
                Logger.Warn($"[GetUserLevelInfo] 用户等级自动修正: UserId={id}, Old={userLevel.GroupChatId}, New={correctSettings.Id}, Amount={userLevel.CumulativeAmount}");

                userLevel.GroupChatId = correctSettings.Id;
                await _sqlSugarClient.Updateable(userLevel).ExecuteCommandAsync();
            }

            return new UserLevelInfoDto
            {
                UserLevel = userLevel,
                LevelSettings = correctSettings ?? new GroupChatLevelSettingsEntity { Level = 0, Name = "普通用户" }
            };
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"获取用户等级信息失败，错误信息：" + ex.Message);
        }
    }

    /// <summary>
    /// 测试数据结构兼容性（仅用于开发测试）
    /// </summary>
    /// <returns>测试用的数据结构示例</returns>
    [HttpGet("TestDataStructure")]
    [AbpAuthorize]
    public UserLevelInfoDto TestDataStructure()
    {
        var testUserLevel = new UserGroupLevelEntity
        {
            UserId = 1270,
            GroupChatId = 6,
            CumulativeAmount = 73164.00m,
            Id = 55
        };

        var testLevelSettings = new GroupChatLevelSettingsEntity
        {
            Name = "诅咒迷宫の双王",
            Level = 5,
            AmountRequired = 38888.00m,
            BorderColor = "#0228FF",
            RightBorderColor = "#0149FF",
            Id = 6
        };

        return new UserLevelInfoDto
        {
            UserLevel = testUserLevel,
            LevelSettings = testLevelSettings
        };
    }
}
