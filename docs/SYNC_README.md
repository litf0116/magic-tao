# 飞书文档同步说明

## 同步机制

```
本地 docs/*.md 变更
    ↓
git commit 触发 post-commit hook
    ↓
feishu-doc-sync.js 读取变更文件
    ↓
读取 sync-mapping.json 查找已有文档
    ↓
创建或更新飞书文档
    ↓
更新 sync-mapping.json
```

## 文件说明

- `sync-mapping.json` — 本地文件 ↔ 飞书文档 ID 映射（自动维护）
- `scripts/feishu-doc-sync.js` — 同步脚本

## 使用方式

### 自动同步（推荐）
```bash
# 修改 docs/ 下的 .md 文件后，commit 即可自动同步
git add .
git commit -m "更新文档"
# → 自动触发 post-commit hook，同步到飞书
```

### 手动同步
```bash
# 同步上次提交变更的文件
node scripts/feishu-doc-sync.js

# 同步 docs/ 下所有文件
node scripts/feishu-doc-sync.js --all

# 同步指定文件
node scripts/feishu-doc-sync.js docs/xxx.md
```

### 环境变量配置

需要设置飞书应用凭证：

```bash
export FEISHU_APP_ID="cli_a920700f1a211bd2"
export FEISHU_APP_SECRET="你的AppSecret"
```

建议写入 `~/.zshrc` 或项目 `.env` 文件。

## 已在飞书创建的文档

| 本地文件 | 飞书文档 |
|---------|---------|
| APP开发规格说明书.md | [打开](https://www.feishu.cn/docx/BzphdlOdOoOjhnxazL8cpxOPncc) |
| Docker-README.md | [打开](https://www.feishu.cn/docx/NTejd6bhyoQnjJxWuGqcDtN3n6e) |
| ai-code-review-guide.md | [打开](https://www.feishu.cn/docx/AOQxd4KYdobde3xIPAZcFZiFnIh) |

## 注意事项

1. **首次同步**：新文件会创建新文档，已在 mapping 中的文件会更新已有文档
2. **冲突处理**：如果飞书端有手动编辑，同步会覆盖（以本地为准）
3. **不要手动编辑 mapping**：由脚本自动维护
4. **手动触发同步后需要 commit mapping 变更**
