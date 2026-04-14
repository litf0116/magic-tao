# Changelog

All notable changes to this project will be documented in this file.

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
