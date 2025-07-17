# 生产环境Console.log移除配置

## 配置说明

项目已配置在生产环境中自动移除所有`console.log`、`console.warn`、`console.info`、`console.debug`和`debugger`语句。

## 配置位置

配置文件：`pc/vite.config.mts`

```typescript
build: {
    // 生产环境移除console.log
    minify: 'terser',
    terserOptions: {
        compress: {
            drop_console: mode === 'production',
            drop_debugger: mode === 'production',
        },
    },
},
```

## 工作原理

1. **开发环境**：所有console.log正常输出，便于调试
2. **生产环境**：构建时自动移除所有console.log和debugger语句
3. **条件判断**：通过`mode === 'production'`判断当前环境

## 测试验证

### 开发环境测试
```bash
npm run dev
# 或
yarn dev
```
- 控制台会显示所有console.log输出

### 生产环境测试
```bash
npm run build
# 或
yarn build
```
- 构建后的代码中不包含console.log语句
- 可以通过浏览器开发者工具验证

### 验证方法
1. 运行生产构建：`npm run build`
2. 打开生成的`dist/index.html`
3. 打开浏览器开发者工具
4. 查看控制台，应该没有console.log输出

## 注意事项

1. **重要日志保留**：如果需要保留某些重要的错误日志，请使用`console.error`
2. **调试信息**：开发时仍可正常使用console.log进行调试
3. **性能提升**：移除console.log可以减少生产环境的代码体积和执行时间
4. **安全性**：避免在生产环境中暴露调试信息

## 配置选项说明

- `drop_console: true` - 移除所有console.*方法调用
- `drop_debugger: true` - 移除所有debugger语句
- `mode === 'production'` - 仅在生产模式下启用

## 兼容性

- 支持所有现代浏览器
- 不影响开发环境的调试体验
- 自动处理所有类型的console方法（log, warn, info, debug等）

## 测试文件

项目包含测试文件：`pc/src/utils/console-test.ts`
- 用于验证配置是否生效
- 测试完成后可以删除 
