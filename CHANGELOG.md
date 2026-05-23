# Changelog

All notable changes to this project will be documented in this file.

## [1.4.0] - 2026-05-22

### Added
- Docs: 全量代码库分析文档（架构/技术栈/集成/结构/约定/测试/风险）
- Docs: 业务流程分析文档（用户认证/即时通讯/商品拍卖/订单支付/推送通知）
- Docs: API 接口清单（14 组 70+ 接口）
- Docs: 技术债跟踪表（18 项按严重程度分级）
- Docs: 里程碑复盘总结

### Changed
- Flutter App: iOS + Android 双端成功上架发布
- Flutter App: 禁用 iOS 审核期间版本更新弹窗
- Flutter App: Profile 页面提现入口替换为"即将上线"提示
- Flutter App: 移除 6 处 `debugPrint` 调试日志
- 微信小程序: Tabbar 非活跃标签颜色 `#dfdfdf` → `#999999`（WCAG AA 对比度）
- 微信小程序: Tabbar 添加显式 `font-weight` 定义（iOS 兼容）

### Fixed
- 微信小程序: 修复 5 处空 catch 块（App.vue、chatStore、auction、chatMain）
- 微信小程序: 修复 tabbar `setInterval` 定时器泄漏（Tab 切换时时钟堆积）
- 微信小程序: 移除活跃调试 `console.log`（跨 14 个文件清理）

## [1.3.1] - 2026-05-23

### Added
- SMS: 阿里云短信服务集成，使用官方 SDK 替换手写 HMAC-SHA256 签名
- SMS: 验证码登录流程（手机号 + 短信验证码）
- SMS: 验证码防双花机制（原子性 `ExecuteUpdateAsync`）
- SMS: per-key 信号量锁解决 TOCTOU 频率限制竞争
- Docs: SMS 测试验证报告

### Changed
- 全局: 统一系统时间为 `DateTime.Now`（北京时间 UTC+8），仅保留 3 处 Unix 时间戳计算场景使用 `DateTime.UtcNow`
- PayOrder: `SuccessPayTime` 时间字段对齐使用 `DateTime.Now`
- SMS: 先发送短信成功后再入库，避免发送失败产生脏数据
- SMS: `SmsSender` 使用官方 SDK，移除手动 HTTP 签名实现
- SMS: `SmsVerificationCodeService` 使用 `Random.Shared` 替代 `new Random()`
- TokenAuth: `SendSmsCode` 返回 `{success: true}` 结构化响应
- TokenAuth: 手机号正则为 `private const string` 常量

### Fixed
- SMS: 修复 `SmsSender` 每次请求创建 `new HttpClient()` 的 socket 资源泄漏
- SMS: 修复 HTTP 请求无超时设置的安全隐患（官方 SDK 内置超时）
- SMS: 修复短信验证码发送无事务保护（先发送后入库）

### Performance
- SMS: 添加 `SmsVerificationCodes` 复合索引 `IX_PhoneNumber_Purpose_CreationTime` 优化查询

## [1.3.0] - 2026-04-14

### Added
- PC Web: 优化扫码登录 CORS 配置
- PC Web: 完善 axios 响应拦截器，支持无 `__abp` 标记的 API 响应
- App: 集成极光推送与通知横幅
- App: 推送通知支持声音提示
- App: 订阅通知时检查消息通知权限
- App: 优化极光推送配置
- Scripts: 新增 molitao_app 自动化部署脚本，支持 API 发布流程
- H5: 实现 WebPush 订阅拍卖开拍通知功能

### Fixed
- PC Web: 修复响应拦截器无法识别无 `__abp` 标记的 API 响应
- PC Web: 修复本地开发环境扫码登录 CORS 问题
- PC Web: 移除调试日志
- App: 修复推送横幅渲染错误
- Backend: 修复聊天消息首次加载顺序问题
- Backend: PushController 添加 AbpAuthorize 特性确保认证
- Backend: websocketId 在用户登录时设置，避免竞态条件

### Changed
- 全局: 支付页面及相关入口优化
- 全局: 将所有'保证金'改为'诚信履约金'

## [1.2.0] - 2026-03-08

### Added
- 极光推送功能
- 个人资料完善引导

## [1.1.0] - 2026-03-29

### Added
- 微信申诉功能
- 恢复交易帖子功能

## [1.0.0] - 2026-01-24

### Added
- 混合缓存优化
- Initial release
