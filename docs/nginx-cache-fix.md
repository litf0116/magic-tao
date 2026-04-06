# Nginx 缓存配置修复方案

## 问题原因

部分客户访问网站时出现白屏，原因是：

1. **浏览器缓存了旧版 index.html**
2. 旧版 index.html 引用的 JS/CSS 文件已被新部署删除
3. 浏览器加载不到资源 → JavaScript 报错 → 白屏

## 解决方案：修改 Nginx 配置

### 步骤 1：登录宝塔面板

1. 访问宝塔面板地址（通常是 `http://服务器IP:8888`）
2. 点击左侧菜单 **网站**
3. 找到 `www.molitao.top`，点击 **设置**

### 步骤 2：修改配置文件

在网站设置页面，点击 **配置文件** 标签，找到以下位置：

```nginx
# 在 server 块内，在 location / 之前添加以下配置
```

**添加以下内容（复制整段）：**

```nginx
    # ========================================
    # 缓存控制配置 - 解决白屏问题
    # ========================================

    # index.html 禁止缓存（每次都从服务器获取最新版本）
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
        try_files $uri $uri/ /index.html;
    }

    # 带hash的JS/CSS文件可以长期缓存（文件名包含hash，内容变化文件名就变了）
    location ~* \.(?:css|js)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        access_log off;
    }

    # 图片等静态资源长期缓存
    location ~* \.(?:jpg|jpeg|gif|png|ico|svg|webp|woff|woff2|ttf|eot)$ {
        expires 30d;
        add_header Cache-Control "public";
        access_log off;
    }
```

### 步骤 3：完整配置示例

修改后，你的 Nginx 配置应该类似这样：

```nginx
upstream molitao_api {
    server 127.0.0.1:5000 weight=1 max_fails=3 fail_timeout=30s;
}

server {
    listen 80;
    listen 443 ssl http2;
    server_name www.molitao.top;
    index index.php index.html index.htm index.php default.htm default.html;
    root /www/wwwroot/www.molitao.top;

    # SSL配置...（保持不变）

    # ========================================
    # 缓存控制配置 - 解决白屏问题（新增部分开始）
    # ========================================

    # index.html 禁止缓存
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
        try_files $uri $uri/ /index.html;
    }

    # JS/CSS 长期缓存
    location ~* \.(?:css|js)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        access_log off;
    }

    # 静态资源长期缓存
    location ~* \.(?:jpg|jpeg|gif|png|ico|svg|webp|woff|woff2|ttf|eot)$ {
        expires 30d;
        add_header Cache-Control "public";
        access_log off;
    }

    # ========================================
    # 缓存控制配置结束
    # ========================================

    # 原有的配置保持不变...

    # Vue Router History 模式支持
    location / {
        try_files $uri $uri/ /index.html;
    }

    # API 代理...（保持不变）

    # WebSocket 配置...（保持不变）
}
```

### 步骤 4：保存并重载配置

1. 点击 **保存** 按钮
2. 宝塔会自动重载 Nginx 配置
3. 或者手动点击 **重载配置**

### 步骤 5：验证配置

在宝塔面板或 SSH 中执行：

```bash
# 测试 Nginx 配置语法
nginx -t

# 如果显示 "syntax is ok"，则重载配置
nginx -s reload
```

## 客户端处理方案

### 方案 A：让客户清理缓存（立即解决）

提供客户以下操作指引：

#### Chrome/Edge 浏览器：
1. 按 `Ctrl + Shift + Delete`（Mac: `Cmd + Shift + Delete`）
2. 时间范围选择：**所有时间**
3. 勾选：**缓存的图片和文件**
4. 点击 **清除数据**
5. 强制刷新页面：`Ctrl + F5`（Mac: `Cmd + Shift + R`）

#### 快捷方式：
- **Windows**: `Ctrl + F5` 强制刷新
- **Mac**: `Cmd + Shift + R` 强制刷新
- 或者打开浏览器**无痕模式**访问网站

### 方案 B：前端代码优化（长期方案）

#### 1. 在 index.html 中添加 meta 标签

编辑 `magic-tao/pc/index.html`：

```html
<head>
    <meta charset="UTF-8">
    <link rel="icon" type="image/svg+xml" href="/vite.svg" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />

    <!-- 禁止缓存 index.html -->
    <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="0" />

    <title></title>
</head>
```

#### 2. Vite 构建配置优化

编辑 `magic-tao/pc/vite.config.mts`，确保文件名带 hash：

```typescript
build: {
    rollupOptions: {
        output: {
            // 确保文件名包含 hash
            entryFileNames: 'assets/[name]-[hash].js',
            chunkFileNames: 'assets/[name]-[hash].js',
            assetFileNames: 'assets/[name]-[hash].[ext]'
        }
    }
}
```

## 验证修复效果

### 1. 检查响应头

```bash
# 检查 index.html 的响应头
curl -I https://www.molitao.top/index.html

# 应该看到：
# Cache-Control: no-cache, no-store, must-revalidate
# Pragma: no-cache
# Expires: 0

# 检查 JS 文件的响应头
curl -I https://www.molitao.top/assets/index-fad67373a.js

# 应该看到：
# Cache-Control: public, immutable
# Expires: （一年后的日期）
```

### 2. 浏览器测试

1. 打开 Chrome DevTools（F12）
2. 进入 Network 标签
3. 刷新页面
4. 查看 index.html 的响应头，应该有 `Cache-Control: no-cache`

## 部署检查清单

每次部署后，建议执行：

- [ ] Nginx 配置已添加缓存控制
- [ ] 检查 index.html 响应头包含 `no-cache`
- [ ] 检查静态资源响应头包含长期缓存
- [ ] 部署后访问网站验证无白屏
- [ ] 提供客户清理缓存指引（如需要）

## 常见问题

### Q: 修改后还有客户反馈白屏？

A: 让客户执行以下操作：
1. 清除浏览器缓存（见上方指引）
2. 使用无痕模式访问
3. 检查是否有 CDN 缓存（如果用了 Cloudflare 等 CDN，需要清除 CDN 缓存）

### Q: 为什么 JS 文件要长期缓存？

A: Vite 构建时会给文件名加上 hash（如 `index-fad67373a.js`），只要文件内容变化，hash 就会变化，文件名就会改变。所以可以安全地长期缓存。

### Q: 需要修改后端配置吗？

A: 不需要。这是纯前端缓存问题，只需要修改 Nginx 配置即可。