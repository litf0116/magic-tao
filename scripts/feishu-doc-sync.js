#!/usr/bin/env node
/**
 * 魔力淘文档 → 飞书云文档 同步脚本
 * 
 * 用法：
 *   node feishu-doc-sync.js              # 同步上次提交变更的文件
 *   node feishu-doc-sync.js --all        # 同步 docs/ 下所有文件
 *   node feishu-doc-sync.js <file1.md>  # 同步指定文件
 */

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// ============ 配置区 ============
const DOCS_DIR = path.join(__dirname, '..', 'docs');
const MAPPING_FILE = path.join(DOCS_DIR, 'sync-mapping.json');
const PROJECT_NAME = 'magic-tao';

// 飞书 API 配置（从环境变量读取）
const FEISHU_APP_ID = process.env.FEISHU_APP_ID || '';
const FEISHU_APP_SECRET = process.env.FEISHU_APP_SECRET || '';

// ============ 工具函数 ============

/** 获取 Feishu Access Token */
async function getAccessToken() {
  const res = await fetch('https://open.feishu.cn/open-apis/auth/v3/tenant_access_token/internal', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ app_id: FEISHU_APP_ID, app_secret: FEISHU_APP_SECRET })
  });
  const data = await res.json();
  if (data.code !== 0) throw new Error(`获取 Token 失败: ${data.msg}`);
  return data.tenant_access_token;
}

/** 读取本地 Markdown 文件 */
function readMarkdownFile(filePath) {
  return fs.readFileSync(filePath, 'utf-8');
}

/** 获取 git 上次提交的变更文件 */
function getChangedFiles() {
  try {
    const output = execSync('git diff --name-only HEAD~1 HEAD', { cwd: path.join(__dirname, '..') })
      .toString().trim().split('\n').filter(f => f.endsWith('.md'));
    return output;
  } catch {
    // 首次提交没有 HEAD~1，返回所有 md 文件
    return fs.readdirSync(DOCS_DIR).filter(f => f.endsWith('.md'));
  }
}

/** 加载/保存映射文件 */
function loadMapping() {
  if (!fs.existsSync(MAPPING_FILE)) return {};
  return JSON.parse(fs.readFileSync(MAPPING_FILE, 'utf-8'));
}

function saveMapping(mapping) {
  fs.writeFileSync(MAPPING_FILE, JSON.stringify(mapping, null, 2), 'utf-8');
}

/** 简单 MD → 飞书文档格式转换（保留代码块） */
function markdownToFeishuBlocks(mdContent) {
  const lines = mdContent.split('\n');
  const blocks = [];
  let inCodeBlock = false;
  let codeLines = [];

  for (let line of lines) {
    if (line.startsWith('```')) {
      if (!inCodeBlock) {
        inCodeBlock = true;
        codeLines = [];
      } else {
        // 代码块结束
        blocks.push({
          block_type: 2,
          code: {
            language: 1,
            content: codeLines.join('\n')
          }
        });
        inCodeBlock = false;
        codeLines = [];
      }
    } else if (inCodeBlock) {
      codeLines.push(line);
    } else if (line.trim() === '') {
      // 空行转 paragraph
      blocks.push({ block_type: 2, text: { elements: [{ text_run: { content: ' ' } }], style: {} } });
    } else {
      // 普通文本行
      blocks.push({
        block_type: 2,
        text: {
          elements: [{ text_run: { content: line } }],
          style: {}
        }
      });
    }
  }
  return blocks;
}

/** 创建飞书文档 */
async function createFeishuDoc(token, title, mdContent) {
  // 先创建空白文档
  const createRes = await fetch('https://open.feishu.cn/open-apis/docx/v1/documents', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ title })
  });
  const createData = await createRes.json();
  if (createData.code !== 0) throw new Error(`创建文档失败: ${createData.msg}`);
  
  const docId = createData.data.document.document_id;
  
  // 写入内容
  const blocks = markdownToFeishuBlocks(mdContent);
  await fetch(`https://open.feishu.cn/open-apis/docx/v1/documents/${docId}/blocks`, {
    method: 'PATCH',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ updates: blocks })
  });
  
  return docId;
}

/** 更新飞书文档内容 */
async function updateFeishuDoc(token, docId, mdContent) {
  // 获取现有 blocks
  const getRes = await fetch(
    `https://open.feishu.cn/open-apis/docx/v1/documents/${docId}/blocks?page_size=500`,
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  const getData = await getRes.json();
  if (getData.code !== 0) throw new Error(`获取文档块失败: ${getData.msg}`);
  
  const existingBlocks = getData.data.items || [];
  const blockIds = existingBlocks.map(b => b.block_id);
  
  // 删除所有现有块
  if (blockIds.length > 0) {
    await fetch(`https://open.feishu.cn/open-apis/docx/v1/documents/${docId}/blocks/batch_delete`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ start_index: 0, end_index: blockIds.length, block_ids: blockIds })
    });
  }
  
  // 写入新内容
  const blocks = markdownToFeishuBlocks(mdContent);
  await fetch(`https://open.feishu.cn/open-apis/docx/v1/documents/${docId}/blocks`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ children: blocks, index: 0 })
  });
}

// ============ 主流程 ============

async function main() {
  const args = process.argv.slice(2);
  const syncAll = args.includes('--all');
  
  // 确定要同步的文件
  let filesToSync;
  if (args.length > 0 && !args.includes('--all')) {
    // 指定文件
    filesToSync = args.filter(f => f.endsWith('.md'));
  } else if (syncAll) {
    filesToSync = fs.readdirSync(DOCS_DIR).filter(f => f.endsWith('.md') && f !== 'sync-mapping.json');
  } else {
    filesToSync = getChangedFiles().filter(f => f !== 'sync-mapping.json');
  }
  
  if (filesToSync.length === 0) {
    console.log('✅ 没有需要同步的文件');
    return;
  }
  
  if (!FEISHU_APP_ID || !FEISHU_APP_SECRET) {
    console.error('❌ 缺少飞书 API 凭证，请设置环境变量 FEISHU_APP_ID 和 FEISHU_APP_SECRET');
    process.exit(1);
  }
  
  console.log(`📋 将同步 ${filesToSync.length} 个文件`);
  
  const token = await getAccessToken();
  const mapping = loadMapping();
  const results = [];
  
  for (const file of filesToSync) {
    const filePath = path.join(DOCS_DIR, file);
    if (!fs.existsSync(filePath)) {
      console.warn(`⚠️ 文件不存在: ${file}`);
      continue;
    }
    
    const title = file.replace('.md', '');
    const content = readMarkdownFile(filePath);
    
    try {
      if (mapping[file]) {
        // 更新已有文档
        console.log(`🔄 更新文档: ${title}`);
        await updateFeishuDoc(token, mapping[file].doc_id, content);
        results.push({ file, status: 'updated', doc_id: mapping[file].doc_id });
      } else {
        // 创建新文档
        console.log(`🆕 创建文档: ${title}`);
        const docId = await createFeishuDoc(token, title, content);
        results.push({ file, status: 'created', doc_id: docId });
        mapping[file] = {
          doc_id: docId,
          synced_at: new Date().toISOString()
        };
      }
    } catch (err) {
      console.error(`❌ 同步失败 [${file}]: ${err.message}`);
      results.push({ file, status: 'error', error: err.message });
    }
  }
  
  // 保存新的映射
  saveMapping(mapping);
  
  // 输出汇总
  console.log('\n========== 同步结果 ==========');
  for (const r of results) {
    const icon = r.status === 'error' ? '❌' : r.status === 'created' ? '✅' : '🔄';
    console.log(`${icon} ${r.file} → ${r.doc_id || r.error}`);
  }
  console.log('\n💾 映射文件已更新: docs/sync-mapping.json');
}

main().catch(console.error);
