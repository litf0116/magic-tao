#!/usr/bin/env python3
"""
魔力百科详情页补采脚本
使用单线程+重试机制避免限流
"""

import asyncio
import json
import time
from playwright.async_api import async_playwright

OUTPUT_FILE = "monsters_data.json"

async def fetch_detail_with_retry(page, monster_info, max_retries=3):
    """带重试的详情页获取"""
    for attempt in range(max_retries):
        try:
            await page.goto(monster_info['detailUrl'], wait_until="domcontentloaded", timeout=30000)
            await page.wait_for_timeout(800)

            detail = await page.evaluate('''() => {
                const result = {};
                const bodyText = document.body.innerText;

                const heading = document.querySelector('h3');
                result.name = heading ? heading.firstChild.textContent.trim() : '';

                const rankMatch = bodyText.match(/总档位[：:]\s*(\d+)/);
                result.totalRank = rankMatch ? rankMatch[1] : '';

                const lines = bodyText.split('\\n');
                const attrLabels = ['体质', '力量', '防御', '敏捷', '魔法'];
                result.attributes = {};
                attrLabels.forEach(label => {
                    const idx = lines.indexOf(label);
                    if (idx !== -1 && idx + 1 < lines.length) {
                        const val = lines[idx + 1].trim();
                        if (/^\\d+$/.test(val)) {
                            result.attributes[label] = val;
                        }
                    }
                });

                const resistLabels = ['地', '水', '火', '风'];
                result.resistance = {};
                resistLabels.forEach(label => {
                    const idx = lines.indexOf(label);
                    if (idx !== -1 && idx + 1 < lines.length) {
                        const val = lines[idx + 1].trim();
                        if (/^\\d+$/.test(val)) {
                            result.resistance[label] = val;
                        }
                    }
                });

                result.details = {};
                const table = document.querySelector('table');
                if (table) {
                    const rows = table.querySelectorAll('tr');
                    rows.forEach(row => {
                        const cells = row.querySelectorAll('td');
                        if (cells.length >= 2) {
                            const key = cells[0].textContent.trim().replace(/\\s+/g, '');
                            let value = '';
                            for (let i = 1; i < cells.length; i++) {
                                const cellText = cells[i].textContent.trim().replace(/\\s+/g, ' ');
                                if (cellText) value += cellText + ' ';
                            }
                            result.details[key] = value.trim();
                        }
                    });
                }

                return result;
            }''')

            return {**monster_info, **detail}
        except Exception as e:
            if attempt < max_retries - 1:
                await asyncio.sleep(2)
                continue
            else:
                print(f"  失败: {monster_info.get('name', 'unknown')} - {e}")
                return monster_info

async def main():
    with open(OUTPUT_FILE, 'r', encoding='utf-8') as f:
        monsters = json.load(f)

    # 找出缺少详情数据的魔物
    needs_fetch = []
    for m in monsters:
        attrs = m.get('attributes', {})
        if not attrs or not any(attrs.values()) if attrs else True:
            needs_fetch.append(m)

    print(f"需要补采: {len(needs_fetch)} 个魔物")

    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        page = await browser.new_page()

        success = 0
        failed = 0

        for i, monster in enumerate(needs_fetch):
            print(f"[{i+1}/{len(needs_fetch)}] 采集: {monster.get('name', 'unknown')}", end=" ")
            result = await fetch_detail_with_retry(page, monster)

            attrs = result.get('attributes', {})
            if attrs and any(attrs.values()) if attrs else False:
                monsters[monsters.index(monster)] = result
                success += 1
                print("✓")
            else:
                failed += 1
                print("✗")

            if (i + 1) % 50 == 0:
                with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
                    json.dump(monsters, f, ensure_ascii=False, indent=2)
                print(f"\n进度保存: {success} 成功, {failed} 失败")

            await asyncio.sleep(0.3)

        with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
            json.dump(monsters, f, ensure_ascii=False, indent=2)

        print(f"\n补采完成! 成功: {success}, 失败: {failed}")
        await browser.close()

if __name__ == "__main__":
    asyncio.run(main())