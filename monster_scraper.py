#!/usr/bin/env python3
"""
魔力百科魔物数据采集脚本
采集所有33页的魔物列表和详情页数据
"""

import asyncio
import json
import re
import time
from playwright.async_api import async_playwright
from urllib.parse import urljoin

BASE_URL = "https://molibaike.com"
OUTPUT_FILE = "monsters_data.json"

async def fetch_list_page(page, page_num):
    """获取列表页"""
    url = f"{BASE_URL}/Monster?page={page_num}"
    print(f"正在获取第 {page_num}/33 页...")
    await page.goto(url, wait_until="domcontentloaded")
    await page.wait_for_timeout(500)  # 等待JS渲染

    # 提取魔物数据
    monsters = await page.evaluate('''() => {
        const monsterLinks = document.querySelectorAll('a[href*="/Monster/Detail"]');
        const monsters = [];

        for (let i = 0; i < monsterLinks.length; i += 2) {
            const nameLink = monsterLinks[i];
            const viewLink = monsterLinks[i + 1];
            if (nameLink && viewLink && nameLink.href.includes('/Monster/Detail')) {
                const card = nameLink.closest('.card') || nameLink.parentElement.parentElement;
                const allLinks = card ? card.querySelectorAll('a') : [];
                let type = '';
                for (let j = 0; j < allLinks.length; j++) {
                    const href = allLinks[j].href || '';
                    if (href.includes('type=') && !href.includes('/Monster/Detail')) {
                        type = allLinks[j].textContent.trim();
                        break;
                    }
                }

                monsters.push({
                    name: nameLink.textContent.trim(),
                    detailUrl: nameLink.href.split('?')[0],
                    type: type || '未知'
                });
            }
        }
        return monsters;
    }''')

    return monsters

async def fetch_detail_page(page, monster_info):
    """获取详情页"""
    try:
        await page.goto(monster_info['detailUrl'], wait_until="domcontentloaded")
        await page.wait_for_timeout(300)

        detail = await page.evaluate('''() => {
            const result = {};
            const bodyText = document.body.innerText;

            // 名称
            const heading = document.querySelector('h3');
            result.name = heading ? heading.firstChild.textContent.trim() : '';

            // 总档位
            const rankMatch = bodyText.match(/总档位[：:]\s*(\d+)/);
            result.totalRank = rankMatch ? rankMatch[1] : '';

            // 属性值
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

            // 抗性
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

            // 详细信息表格
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
        print(f"  获取详情失败: {monster_info['name']} - {e}")
        return monster_info

async def main():
    all_monsters = []

    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        page = await browser.new_page()

        # 第一步：采集所有列表页
        print("=" * 50)
        print("第一步：采集33页列表...")
        print("=" * 50)

        for page_num in range(1, 34):
            monsters = await fetch_list_page(page, page_num)
            all_monsters.extend(monsters)
            print(f"  第{page_num}页: 获取 {len(monsters)} 个魔物, 累计: {len(all_monsters)}")

        print(f"\\n列表采集完成! 共获取 {len(all_monsters)} 个魔物")

        # 保存中间结果
        with open("monsters_list.json", "w", encoding="utf-8") as f:
            json.dump(all_monsters, f, ensure_ascii=False, indent=2)
        print("已保存到 monsters_list.json")

        # 第二步：采集详情页（分批处理）
        print("\\n" + "=" * 50)
        print("第二步：采集详情页...")
        print("=" * 50)

        detailed_monsters = []
        batch_size = 20
        total = len(all_monsters)

        for i in range(0, total, batch_size):
            batch = all_monsters[i:i+batch_size]
            print(f"\\n处理批次 {i//batch_size + 1}/{(total + batch_size - 1)//batch_size} ({i+1}-{min(i+batch_size, total)}/{total})")

            tasks = [fetch_detail_page(page, m) for m in batch]
            results = await asyncio.gather(*tasks)

            for r in results:
                detailed_monsters.append(r)
                print(f"  ✓ {r.get('name', 'Unknown')} 完成")

            # 每批次保存一次
            with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
                json.dump(detailed_monsters, f, ensure_ascii=False, indent=2)

        print(f"\\n详情采集完成! 共 {len(detailed_monsters)} 个魔物")
        print(f"已保存到 {OUTPUT_FILE}")

        await browser.close()

if __name__ == "__main__":
    asyncio.run(main())