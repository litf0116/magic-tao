#!/usr/bin/env python3
import asyncio
import json
import random
from playwright.async_api import async_playwright

INPUT_FILE = "monsters_data.json"
REQUEST_INTERVAL = 5

async def check_and_fetch(page, monster_info):
    url = monster_info.get('detailUrl', '')
    name = monster_info.get('name', '')

    try:
        await page.set_extra_http_headers({
            "Accept-Language": "zh-CN,zh;q=0.9,en;q=0.8",
        })
        await page.goto(url, timeout=20000, wait_until="domcontentloaded")
        await page.wait_for_timeout(1500)
        text = await page.inner_text('body')

        if '总档位' not in text:
            return None, False

        detail = await page.evaluate('''() => {
            const result = {};
            const bodyText = document.body.innerText;
            const rankMatch = bodyText.match(/总档位[：:]\s*(\d+)/);
            result.totalRank = rankMatch ? rankMatch[1] : '';
            const lines = bodyText.split('\n');
            const attrLabels = ['体质', '力量', '防御', '敏捷', '魔法'];
            result.attributes = {};
            attrLabels.forEach(label => {
                const idx = lines.indexOf(label);
                if (idx !== -1 && idx + 1 < lines.length) {
                    const val = lines[idx + 1].trim();
                    if (/^\d+$/.test(val)) result.attributes[label] = val;
                }
            });
            const resistLabels = ['地', '水', '火', '风'];
            result.resistance = {};
            resistLabels.forEach(label => {
                const idx = lines.indexOf(label);
                if (idx !== -1 && idx + 1 < lines.length) {
                    const val = lines[idx + 1].trim();
                    if (/^\d+$/.test(val)) result.resistance[label] = val;
                }
            });
            result.details = {};
            const table = document.querySelector('table');
            if (table) {
                const rows = table.querySelectorAll('tr');
                rows.forEach(row => {
                    const cells = row.querySelectorAll('td');
                    if (cells.length >= 2) {
                        const key = cells[0].textContent.trim().replace(/\s+/g, '');
                        let value = '';
                        for (let i = 1; i < cells.length; i++) {
                            const cellText = cells[i].textContent.trim().replace(/\s+/g, ' ');
                            if (cellText) value += cellText + ' ';
                        }
                        result.details[key] = value.trim();
                    }
                });
            }
            return result;
        }''')

        has_data = detail.get('attributes') and any(detail['attributes'].values())
        return {**monster_info, **detail} if has_data else None, True

    except Exception as e:
        return None, False

async def main():
    with open(INPUT_FILE, 'r', encoding='utf-8') as f:
        monsters = json.load(f)

    needs_fetch = []
    for m in monsters:
        attrs = m.get('attributes', {})
        if not attrs or not any(attrs.values()) if attrs else True:
            needs_fetch.append(m)

    print(f"检测有效URL: {len(needs_fetch)} 个")

    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        page = await browser.new_page()

        success = 0

        for i, monster in enumerate(needs_fetch):
            name = monster.get('name', '')
            result, is_valid = await check_and_fetch(page, monster)

            if result:
                monsters[monsters.index(monster)] = result
                success += 1
                print(f"[{i+1}/{len(needs_fetch)}] ✓ {name}")
            else:
                print(f"[{i+1}/{len(needs_fetch)}] ✗ {name}")

            await asyncio.sleep(REQUEST_INTERVAL + random.uniform(0, 2))

            if (i + 1) % 20 == 0:
                with open(INPUT_FILE, 'w', encoding='utf-8') as f:
                    json.dump(monsters, f, ensure_ascii=False, indent=2)
                print(f"  -> 进度: {success} 成功")

        with open(INPUT_FILE, 'w', encoding='utf-8') as f:
            json.dump(monsters, f, ensure_ascii=False, indent=2)

        print(f"\n完成! 采集成功: {success}")
        await browser.close()

if __name__ == "__main__":
    asyncio.run(main())