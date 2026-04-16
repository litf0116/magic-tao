# Android设备兼容性测试环境

## 🚀 快速开始

```bash
# 一键启动Android测试
./scripts/quick_android_test.sh
```

## 📱 测试设备

| 设备 | 尺寸 | 优先级 | 说明 |
|------|------|--------|------|
| **Pixel 6** | 412x915 | 🥇 最高 | 主力测试设备 |
| **Small Android** | 360x640 | 🥇 最高 | 小屏边缘情况 |
| **Pixel 4a** | 392x850 | 🥈 高 | 常见小屏设备 |

## 📋 测试内容

### 核心功能
- ✅ 底部导航栏适配
- ✅ 聊天界面布局
- ✅ 表单输入体验
- ✅ 图片加载性能

### 布局检查
- ✅ 文本显示完整
- ✅ 按钮易于点击
- ✅ 间距合理
- ✅ 响应式布局

## 🔧 使用步骤

1. **启动测试**:
   ```bash
   ./scripts/quick_android_test.sh
   ```

2. **选择设备**:
   - 在右侧Device Preview面板中选择"Android设备"
   - 按优先级测试设备

3. **执行检查**:
   - 使用`ANDROID_DEVICE_TEST_CHECKLIST.md`逐项检查
   - 记录发现的问题

4. **完成测试**:
   - 确认核心设备测试通过
   - 更新测试报告

## 📚 参考文档

- [快速测试指南](ANDROID_QUICK_TEST.md)
- [详细检查清单](ANDROID_DEVICE_TEST_CHECKLIST.md)

## ✅ 完成标准

- Pixel 6 测试通过
- Small Android 测试通过
- 核心功能正常
- 无严重布局问题

## 🔄 更新日志

- 2026-04-16: 创建Android专用测试环境
- 集成Device Preview工具
- 提供一键启动脚本
- 完善测试文档