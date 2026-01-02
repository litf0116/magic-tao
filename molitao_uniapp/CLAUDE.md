# UniApp 小程序模块 AI 指令扩展

## 技术栈
- UniApp
- Vue 3
- TypeScript
- uView UI
- Pinia (状态管理)
- 小程序云开发 (可选)

## UniApp 开发规范
- 使用 Vue 3 Composition API (`<script setup>`)
- 遵循跨平台兼容性原则
- 使用条件编译处理平台差异 (#ifdef、#endif)
- 页面路由使用 `uni.navigateTo` 等 API
- 合理使用 `uni-app` 的生命周期钩子

## 跨平台兼容性
- 优先使用 UniApp 提供的 API，避免平台原生 API
- 使用 `uni.getSystemInfoSync()` 获取设备信息做适配
- 样式使用 `rpx` 响应式单位，适配不同屏幕
- 图片使用 `@2x`、`@3x` 多倍图适配高分辨率
- 测试覆盖主要平台: 微信小程序、支付宝小程序、App

## 组件开发规范
- 组件命名使用 kebab-case
- 使用 `defineComponent` 定义组件
- Props 定义类型和默认值
- 使用 `emits` 定义组件事件
- 组件样式使用 `scoped`

## TypeScript 规范
- 开启严格模式
- UniApp API 类型使用 `@dcloudio/types`
- 定义页面参数类型: `OnLoadOptions`
- 使用接口定义数据模型
- 避免使用 `any`

## 样式规范
- 使用 Flex 布局
- 字体单位使用 `rpx`
- 避免使用不兼容的 CSS 属性
- 使用 uView UI 组件库保持风格统一
- 自定义样式注意小程序平台限制

## 项目结构
```
src/
├── api/            # API 接口
├── common/         # 公共文件
│   ├── constants/  # 常量
│   ├── utils/      # 工具函数
│   └── styles/     # 公共样式
├── components/     # 组件
├── pages/          # 页面
│   └── [module]/   # 模块页面
├── static/         # 静态资源
├── store/          # Pinia stores
├── types/          # TypeScript 类型
├── pages.json      # 页面配置
├── manifest.json   # 应用配置
└── uni.scss        # 全局样式变量
```

## 页面开发规范
- 页面路径在 `pages.json` 中注册
- 使用 `onLoad` 接收页面参数
- 页面生命周期: `onLoad`、`onShow`、`onReady`
- 下拉刷新使用 `onPullDownRefresh`
- 上拉加载使用 `onReachBottom`
- 分享使用 `onShareAppMessage`

## API 请求规范
- 使用 `uni.request` 封装请求
- 请求拦截处理 Token
- 响应拦截统一处理错误
- 上传文件使用 `uni.uploadFile`
- 下载文件使用 `uni.downloadFile`

## 性能优化
- 图片懒加载
- 长列表分页加载
- 使用 `uni.preloadPage` 预加载页面
- 合理使用 `setData` 避免频繁更新
- 资源文件放在 CDN

## 小程序特有功能
- 授权登录使用 `uni.login`
- 获取用户信息使用 `uni.getUserInfo`
- 微信支付使用 `uni.requestPayment`
- 扫码使用 `uni.scanCode`
- 选择位置使用 `uni.chooseLocation`

## 调试和发布
- 使用微信开发者工具调试
- 使用 `console.log` 调试，发布时移除
- 版本号管理: `manifest.json` 中的 version
- 小程序发布需要审核，注意合规性

## 特定约定
- 状态管理使用 Pinia，适配小程序
- 路由传参使用 query 参数
- 本地存储使用 `uni.setStorageSync`
- 网络状态监听 `uni.onNetworkStatusChange`
- 页面跳转使用 uni-app 的路由 API