using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Controllers;
using TtWork.Project.Domains;

namespace TtWork.Project.Web.Core.Controllers
{
    [Route("api/localdev")]
    public class LocalDevController : AbpControllerBase
    {
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<LocalDevController> _logger;
        private readonly IRepository<CmsCategory, long> _categoryRepo;
        private readonly IRepository<CmsArticle, long> _articleRepo;

        public LocalDevController(
            UserManager userManager,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger<LocalDevController> logger,
            IRepository<CmsCategory, long> categoryRepo,
            IRepository<CmsArticle, long> articleRepo)
        {
            _userManager = userManager;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _categoryRepo = categoryRepo;
            _articleRepo = articleRepo;
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordInput input)
        {
            if (!IsLocalRequest())
            {
                return Forbid();
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user == null)
            {
                return NotFound($"用户ID {input.UserId} 不存在");
            }

            // 使用 UserManager.ChangePasswordAsync 正确设置密码
            // 这会同时更新 Password 和 SecurityStamp
            var result = await _userManager.ChangePasswordAsync(user, input.NewPassword);
            result.CheckErrors(LocalizationManager);

            await _unitOfWorkManager.Current.SaveChangesAsync();

            _logger.LogInformation("本地开发接口: 用户ID {UserId} 密码已重置", input.UserId);

            return Ok(new { message = $"用户ID {input.UserId} 密码已重置为 {input.NewPassword}" });
        }

        /// <summary>
        /// 种子数据：创建法律协议分类（用户协议 + 隐私政策）
        /// </summary>
        [HttpPost("seed-cms-legal")]
        [UnitOfWork]
        public async Task<ActionResult> SeedCmsLegalData()
        {
            if (!IsLocalRequest())
                return Forbid();

            // 检查是否已存在法律协议分类
            var existingCategory = await _categoryRepo.FirstOrDefaultAsync(x => x.Title == "法律协议");
            if (existingCategory != null)
                return Ok(new { message = "法律协议分类已存在，跳过种子", categoryId = existingCategory.Id });

            // 创建分类
            var category = new CmsCategory
            {
                Title = "法律协议",
                Sort = 1
            };
            var categoryId = await _categoryRepo.InsertAndGetIdAsync(category);

            // 创建用户协议
            await _articleRepo.InsertAsync(new CmsArticle
            {
                CategoryId = categoryId,
                Title = "用户协议",
                Status = AlticleStatusEnum.已发布,
                Sort = 1,
                Content = @"欢迎使用魔力淘！

在使用本应用前，请仔细阅读以下条款：

1. 服务说明
魔力淘是一个信息撮合平台，为用户提供游戏虚拟物品的展示和沟通服务。用户之间的交易由双方自行完成，平台不提供交易担保。

2. 用户责任
用户应遵守相关法律法规，不得利用平台进行违法违规活动。用户需对自己的账号安全负责，不得将账号借给他人使用。

3. 交易规则
所有交易均需遵守平台规则，确保交易公平、公正、公开。

4. 费用说明
平台可能收取一定比例的服务费用，具体费用标准以平台公示为准。

5. 隐私保护
我们重视用户隐私，具体见隐私政策。未经用户同意，我们不会向第三方披露用户个人信息。

6. 免责声明
对于因不可抗力或第三方原因导致的服务中断或损失，平台不承担责任。

7. 协议修改
我们有权根据需要修改本协议，修改后的协议将在平台公布。"
            });

            // 创建隐私政策
            await _articleRepo.InsertAsync(new CmsArticle
            {
                CategoryId = categoryId,
                Title = "隐私政策",
                Status = AlticleStatusEnum.已发布,
                Sort = 2,
                Content = @"魔力淘隐私政策

生效日期：2024年1月1日

我们重视并保护您的隐私，本政策说明我们如何收集、使用和保护您的个人信息：

1. 信息收集
我们收集您注册时提供的基本信息（用户名、手机号等）以及交易过程中产生的数据。这些信息包括但不限于：
- 账号信息：用户名、手机号、微信OpenID
- 交易信息：拍卖记录、出价记录、成交记录
- 设备信息：设备型号、操作系统、唯一设备标识

2. 信息使用
您的信息仅用于提供服务、改进用户体验和保障交易安全：
- 提供信息撮合服务
- 发送交易通知和系统消息
- 改进产品功能和服务质量
- 保障账户和交易安全

3. 信息保护
我们采用多种安全措施保护您的个人信息：
- 数据加密传输和存储
- 严格的访问权限控制
- 定期安全审计和漏洞修复

4. 信息共享
除以下情况外，我们不会向第三方共享您的个人信息：
- 获得您的明确同意
- 法律法规要求
- 保护平台和用户的合法权益

5. 您的权利
您有权查询、更正、删除您的个人信息，有权注销账号。

6. 联系我们
如有任何问题，请通过以下方式联系我们：
- 邮箱：support@molitao.top"
            });

            await _unitOfWorkManager.Current.SaveChangesAsync();

            _logger.LogInformation("种子数据: CMS 法律协议分类(ID={CategoryId})已创建", categoryId);

            return Ok(new { message = "法律协议分类及文章创建成功", categoryId });
        }

        private bool IsLocalRequest()
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            return remoteIp == "127.0.0.1" || remoteIp == "::1" || remoteIp == "::ffff:127.0.0.1";
        }
    }

    public class ResetPasswordInput
    {
        public long UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
