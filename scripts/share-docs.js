#!/usr/bin/env node
/**
 * 批量给飞书用户授权文档
 * 将 docs/sync-mapping.json 中所有文档分享给指定用户
 */

const fs = require('fs');
const path = require('path');

const MAPPING_FILE = path.join(__dirname, '..', 'docs', 'sync-mapping.json');
const FEISHU_APP_ID = 'cli_a920700f1a211bd2';
const FEISHU_APP_SECRET = 'Ck2kfRvvWigdn4aK85ckoccIpLTVyIkx';
const USER_OPEN_ID = 'ou_6f87f86ef6a20debc5aa0ec865c7deaf';

async function getAccessToken() {
  const res = await fetch('https://open.feishu.cn/open-apis/auth/v3/tenant_access_token/internal', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ app_id: FEISHU_APP_ID, app_secret: FEISHU_APP_SECRET })
  });
  const data = await res.json();
  if (data.code !== 0) throw new Error(`获取 Token 失败: ${JSON.stringify(data)}`);
  console.log(`🔑 Token 获取成功`);
  return data.tenant_access_token;
}

async function addPermission(token, docId, docUrl) {
  // 尝试不同的 API 方式
  const res = await fetch(`https://open.feishu.cn/open-apis/drive/v1/permissions/${docId}/members?type=docx`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      member_type: 'openid',
      member_id: USER_OPEN_ID,
      perm: 'full_access',
      perm_type: 'container'
    })
  });
  const data = await res.json();
  return data;
}

async function main() {
  const mapping = JSON.parse(fs.readFileSync(MAPPING_FILE, 'utf-8'));
  
  // 每次重新获取 token（确保拿到最新权限）
  const token = await getAccessToken();
  
  const entries = Object.entries(mapping);
  console.log(`📋 将分享 ${entries.length} 个文档...\n`);
  
  let success = 0;
  let failed = 0;
  let skipped = 0;
  
  for (const [filename, info] of entries) {
    try {
      const result = await addPermission(token, info.doc_id, info.doc_url);
      if (result.code === 0) {
        console.log(`✅ ${filename}`);
        success++;
      } else if (result.code === 9904014) {
        console.log(`⏭️  ${filename} (已有权限)`);
        skipped++;
      } else {
        console.log(`❌ ${filename}: [${result.code}] ${result.msg}`);
        failed++;
      }
    } catch (err) {
      console.log(`❌ ${filename}: ${err.message}`);
      failed++;
    }
    
    // 添加延迟避免限流
    await new Promise(r => setTimeout(r, 200));
  }
  
  console.log(`\n========== 完成 ==========`);
  console.log(`✅ 成功: ${success}`);
  console.log(`⏭️  已有权限: ${skipped}`);
  if (failed > 0) console.log(`❌ 失败: ${failed}`);
  console.log(`\n请刷新飞书文档页面查看`);
}

main().catch(console.error);
