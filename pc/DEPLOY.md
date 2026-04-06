# PC 前端部署指南

## 一键部署

```bash
cd /Users/mac/workspace/magic-tao/pc
./deploy.sh
```

## 部署流程

1. **代码检查** - 检查是否有未提交的更改
2. **安装依赖** - `npm install`
3. **构建项目** - `npm run build`
4. **备份旧版本** - 备份到 `/www/backups/pc/时间戳/`
5. **上传新版本** - rsync 或 scp 到服务器
6. **验证部署** - 检查 index.html 和静态资源

## 环境参数

| 环境 | 域名 | 部署目录 |
|------|------|----------|
| production | www.molitao.top | /www/wwwroot/www.molitao.top |
| beta | beta.molitao.top | /www/wwwroot/beta.molitao.top |

## 常用命令

```bash
# 部署生产环境
./deploy.sh production

# 部署测试环境
./deploy.sh beta

# 手动构建
npm run build

# 手动同步到服务器
rsync -avz --delete dist/ molitao:/www/wwwroot/www.molitao.top/
```

## 服务器文件位置

```
/www/wwwroot/www.molitao.top/
├── index.html
└── assets/
```

## 验证部署

```bash
# 检查首页
curl -s -o /dev/null -w "%{http_code}" https://www.molitao.top/

# 检查静态资源
curl -s -I https://www.molitao.top/assets/index-*.js | head -1
```

## 回滚操作

```bash
# 找到备份目录
ssh molitao "ls -la /www/backups/pc/"

# 恢复备份
ssh molitao "rm -rf /www/wwwroot/www.molitao.top && \
  cp -r /www/backups/pc/<时间戳> /www/wwwroot/www.molitao.top"
```
