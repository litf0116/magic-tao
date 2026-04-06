# 内容/广告模块测试规范

## 1. 测试概述

### 1.1 测试目标
验证公告、CMS文章、广告位等内容管理功能。

### 1.2 测试环境
```
后端服务: http://localhost:12580
数据库: MySQL 127.0.0.1:3306/www_molitao_top
测试用户: feifei (ID: 7509)
```

## 2. API接口

### 2.1 获取最新公告
```bash
GET /api/services/app/Announce/GetLatest
Authorization: Bearer {token}
```

### 2.2 获取公开公告列表
```bash
GET /api/services/app/Announce/GetAllPublic?MaxResultCount=5
Authorization: Bearer {token}
```

### 2.3 获取公开CMS文章
```bash
GET /api/services/app/CmsArticle/GetAllPublic?MaxResultCount=5
Authorization: Bearer {token}
```

### 2.4 获取广告列表
```bash
GET /api/AdvertisingSpace/GetList
Authorization: Bearer {token}
```

### 2.5 按类型获取广告
```bash
GET /api/AdvertisingSpace/GetTypeList/{type}
Authorization: Bearer {token}
```
**⚠️ 注意**: type参数必须是数字枚举值（如1），不能是字符串（如"home"）

## 3. 数据库表

### 3.1 t_announce (公告表)
- 总记录数: 11

### 3.2 t_cmsarticle (CMS文章表)
- 总记录数: 13

### 3.3 t_advertisingspace (广告位表)
- 总记录数: 6
- Type字段为int枚举（当前只有Type=1）

## 4. 测试用例

### 4.1 公告测试
- ❌ 获取最新公告返回null
- ✅ 获取公开公告列表 (返回3条)

### 4.2 CMS文章测试
- ✅ 获取公开CMS文章 (返回3条)

### 4.3 广告测试
- ✅ 获取广告列表 (返回2条)
- ❌ GetTypeList传字符串"home"返回验证错误
- ✅ GetTypeList传数字"1"返回1条

## 5. 测试结果

| 测试项 | 状态 | 备注 |
|-------|------|------|
| 获取最新公告 | ❌ 异常 | 返回null，数据库有11条 |
| 获取公开公告列表 | ✅ 通过 | 返回3条 |
| 获取公开CMS文章 | ✅ 通过 | 返回3条 |
| 获取广告列表 | ✅ 通过 | 返回2条 |
| 按类型获取广告(字符串) | ❌ 异常 | type参数不接受字符串 |
| 按类型获取广告(数字) | ✅ 通过 | 返回1条 |

## 6. 数据库验证

```sql
-- 验证公告
SELECT Id, Title, IsPublished FROM t_announce ORDER BY Id DESC LIMIT 5;

-- 验证CMS文章
SELECT Id, Title, IsPublished FROM t_cmsarticle ORDER BY Id DESC LIMIT 5;

-- 验证广告
SELECT Id, Type, Title, Status FROM t_advertisingspace;
```

---
**最后更新**: 2026-04-04
