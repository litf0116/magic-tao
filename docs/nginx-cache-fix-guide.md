# Nginx 缓存配置修复指南

> **目标**：解决部分客户访问网站时出现白屏的问题  
> **时间**：2026-03-26  
> **影响范围**：www.molitao.top（PC 前端）

---

## 一、问题原因

### 现象
- 部分客户访问网站时出现白屏
- 原来能正常访问，突然进不去了

### 根本原因
```
浏览器缓存了旧版 index.html
    ↓
旧 index.html 引用已删除的 JS 文件（如 index-abc123.js）
    ↓
新部署后该 JS 文件不存在（已替换为 index-xyz789.js）
    ↓
JS 加载失败 → 页面白屏
```

### 为什么只有部分客户受影响？
- 不同浏览器缓存策略不同
- 访问频率高的用户更容易缓存旧版本
- 部分浏览器缓存时间较长

---

## 二、修复方案

### 方案概述
在 Nginx 中配置：
- **index.html 禁止缓存**：每次都从服务器获取最新版本
- **JS/CSS 长期缓存**：文件名包含 hash，内容变化文件名就变

---

## 三、修改步骤（宝塔面板）

### 步骤 1：备份当前配置

```bash
# SSH 登录服务器后执行
cp /www/server/panel/vhost/nginx/www.molitao.top.conf /www/server/panel/vhost/nginx/www.molitao.top.conf.bak.20260326
```

或通过宝塔面板：
1. 网站 → www.molitao.top → 设置 → 配置文件
2. 全选复制当前配置，保存到本地文件作为备份

---

### 步骤 2：修改 Nginx 配置

**位置**：宝塔面板 → 网站 → www.molitao.top → 设置 → 配置文件

**找到以下代码位置**：

```nginx
    #禁止在证书验证目录放入敏感文件
    if ( $uri ~ "^/\.well-known/.*\.(php|jsp|py|js|css|lua|ts|go|zip|tar\.gz|rar|7z|sql|bak)$" ) {
        return 403;
    }

    # SPA 路由支持 - Vue Router History 模式
    location / {
        try_files $uri $uri/ /index.html;
    }
```

**在 `# SPA 路由支持` 之前插入以下代码**：

```nginx
    #===========================================
    # 缓存控制配置 - 解决白屏问题
    #===========================================

    # index.html 禁止缓存（每次都从服务器获取最新版本）
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }

    #===========================================
```

---

### 步骤 3：完整配置示例

修改后应该类似这样：

```nginx
upstream molitao_api {
    server 127.0.0.1:5000 weight=1 max_fails=0 fail_timeout=10s;
}

server
{
    listen 80;
    listen 443 ssl http2;
    server_name www.molitao.top;
    index index.php index.html index.htm default.php default.htm default.html;
    root /www/wwwroot/www.molitao.top;

    # ... SSL 配置保持不变 ...

    #禁止在证书验证目录放入敏感文件
    if ( $uri ~ "^/\.well-known/.*\.(php|jsp|py|js|css|lua|ts|go|zip|tar\.gz|rar|7z|sql|bak)$" ) {
        return 403;
    }

    #===========================================
    # 缓存控制配置 - 解决白屏问题
    #===========================================

    # index.html 禁止缓存（每次都从服务器获取最新版本）
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }

    #===========================================

    # SPA 路由支持 - Vue Router History 模式
    location / {
        try_files $uri $uri/ /index.html;
    }

    # ... 其他配置保持不变 ...
}
```

---

### 步骤 4：保存并重载

1. 点击 **保存** 按钮
2. 宝塔面板会自动检测语法并重载 Nginx
3. 如果提示语法错误，检查是否复制完整

---

## 四、验证配置是否生效

### 方法 1：检查响应头（推荐）

```bash
curl -I https://www.molitao.top/index.html 2>/dev/null | grep -iE "cache|pragma|expires"
```

**预期结果**：
```
Cache-Control: no-cache, no-store, must-revalidate
Pragma: no-cache
Expires: 0
```

### 方法 2：浏览器开发者工具

1. 打开 Chrome DevTools（F12）
2. 进入 **Network** 标签
3. 勾选 **Disable cache**（暂时禁用缓存）
4. 刷新页面
5. 点击 `index.html` 请求
6. 查看 **Response Headers**，应该有：
   ```
   Cache-Control: no-cache, no-store, must-revalidate
   ```

### 方法 3：测试新用户访问

用**无痕模式**打开网站：
- Chrome: `Ctrl + Shift + N`
- Edge: `Ctrl + Shift + N`
- Firefox: `Ctrl + Shift + P`

如果无痕模式能正常访问，说明配置生效。

---

## 五、回滚方案

如果修改后出现问题，执行以下命令回滚：

```bash
# 通过 SSH 回滚
cp /www/server/panel/vhost/nginx/www.molitao.top.conf.bak.20260326 /www/server/panel/vhost/nginx/www.molitao.top.conf
nginx -s reload
```

或通过宝塔面板：
1. 网站 → www.molitao.top → 设置 → 配置文件
2. 粘贴之前备份的配置
3. 保存

---

## 六、客户清理缓存指南

配置修改后，**已缓存的用户**仍需要清理一次缓存。

### 发给客户的指引

```
【网站访问异常解决方案】

如遇到网站白屏或无法访问，请按以下步骤清理浏览器缓存：

方法一：强制刷新（最快）
- Windows: 按 Ctrl + F5
- Mac: 按 Cmd + Shift + R

方法二：清除缓存
1. 按 Ctrl + Shift + Delete（Mac: Cmd + Shift + Delete）
2. 时间范围选择"所有时间"
3. 勾选"缓存的图片和文件"
4. 点击"清除数据"
5. 刷新页面

方法三：使用无痕模式
- Chrome/Edge: Ctrl + Shift + N
- 在无痕窗口中访问网站

清理后即可正常访问。
```

---

## 七、检查清单

修改完成后，确认以下事项：

- [ ] Nginx 配置语法正确（`nginx -t` 通过）
- [ ] index.html 响应头包含 `Cache-Control: no-cache`
- [ ] 无痕模式能正常访问网站
- [ ] 已通知客户清理缓存（如需要）

---

## 八、技术说明

### 为什么 JS/CSS 不需要改？

当前配置已有：
```nginx
location ~ .*\.(js|css)?$
{
    expires 12h;
}
```

**原因**：Vite 构建时会给文件名加 hash（如 `index-fd69573a.js`），内容变化 → hash 变化 → 文件名变化。所以即使缓存 12 小时，也不会加载到旧文件。

### 为什么只改 index.html？

`index.html` 是入口文件，它引用具体的 JS/CSS 文件。如果它被缓存，就会引用旧的 JS 路径，导致 404。

---

## 九、常见问题

### Q: 修改后还是有客户白屏？

A: 客户浏览器里已经有旧缓存，需要客户手动清理一次。之后就不会再出现这个问题。

### Q: 会不会影响性能？

A: 不会。index.html 通常只有几 KB，每次请求的开销可以忽略不计。JS/CSS 等大文件仍然会被缓存。

### Q: 需要修改后端配置吗？

A: 不需要。这是纯前端缓存问题。

---

## 十、相关文件

| 文件 | 说明 |
|-----|------|
| `/www/server/panel/vhost/nginx/www.molitao.top.conf` | 当前 Nginx 配置 |
| `/www/server/panel/vhost/nginx/www.molitao.top.conf.bak.20260326` | 备份配置 |
| `magic-tao/docs/nginx-config-after-fix.conf` | 修改后的完整配置参考 |

---

**修改时间**：2026-03-26 后半夜  
**修改人**：运维团队  
**审核人**：___________