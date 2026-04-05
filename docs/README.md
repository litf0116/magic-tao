# Beta 测试服务部署资料

## 📁 目录结构

```
beta-deployment/
├── ssl-certificates/      # SSL 证书文件
│   ├── beta.molitao.top.pem
│   └── beta.molitao.top.key
├── nginx-config/         # Nginx 配置文件
│   ├── beta.molitao.top.conf
│   └── deploy-beta-nginx.sh
└── docs/                # 部署文档
    └── nginx-deployment-guide.md
```

## 🚀 快速部署

```bash
cd /Users/mac/workspace/magic-tao/beta-deployment
sudo bash nginx-config/deploy-beta-nginx.sh
```

## 📋 证书信息

- **域名**: beta.molitao.top
- **有效期**: 2026-03-20 至 2026-06-17
- **颁发机构**: DigiCert

## 🔧 配置说明

- **后端服务**: 127.0.0.1:12580
- **端口**: 80 (HTTP), 443 (HTTPS)
- **WebSocket**: 支持 SignalR
- **文件上传**: 最大 100MB

## 📞 证书续期

证书将于 **2026-06-17** 到期，请提前 30 天续期。

---

**位置**: `/Users/mac/workspace/magic-tao/beta-deployment/`
**更新时间**: 2026-03-20
