# Flutter App 微信支付配置 - 待补充

## 状态
⏳ 待配置

## 背景
Flutter App 需要集成微信 App 支付功能，目前商户号尚未配置完成。

## 需要配置的内容

### 1. 微信开放平台配置
- [ ] 注册微信开放平台账号
- [ ] 创建移动应用并获取 AppID
- [ ] 申请 App 支付能力

### 2. 微信商户平台配置
- [ ] 注册微信商户平台账号
- [ ] 获取商户号 (mch_id)
- [ ] 配置 API 密钥
- [ ] 下载并配置商户证书

### 3. Flutter App 配置
- [ ] 添加微信 SDK 依赖
- [ ] 配置 Android manifest (WXEntryActivity)
- [ ] 配置 iOS info.plist (URL Schemes)
- [ ] 实现微信支付调起逻辑

### 4. 后端配置
- [ ] 配置微信支付 API 密钥
- [ ] 配置商户证书路径
- [ ] 实现统一下单接口
- [ ] 实现支付回调接口

## 相关 API
- `POST /api/services/app/Client/PayDeposit` - 保证金支付
- `GET /api/services/app/Client/TopUp` - 用户充值
- `POST /api/services/app/Client/PayWithdrawal` - 用户提现

## 相关文件
- Flutter: `molitao_app/lib/presentation/pages/profile/profile_page.dart`
- 后端: `backend/src/TtWork.Project/Applications/ClientAppService.cs`

## 备注
配置完成后，需将商户号等信息添加到环境变量或配置文件中，切勿提交到代码仓库。

---
创建时间: 2026-04-05
