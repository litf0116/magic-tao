#!/usr/bin/env python3
import asyncio
import json
import random
from playwright.async_api import async_playwright

OUTPUT_FILE = "monsters_data.json"
REQUEST_INTERVAL = 5
BATCH_SIZE = 50

USER_AGENTS = [
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Safari/605.1.15",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
]

async def fetch_detail_with_retry(page, monster_info, max_retries=3):
    for attempt in range(max_retries):
        try:
            await page.set_extra_http_headers({
                "Accept-Language": "zh-CN,zh;q=0.9,en;q=0.8",
                "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            })
            await page.goto(monster_info['detailUrl'], wait_until="domcontentloaded", timeout=30000)
            await page.wait_for_timeout(1000 + random.randint(0, 500))

            detail = await page.evaluate('''() => {
                const result = {};
                const bodyText = document.body.innerText;

                const heading = document.querySelector('h3');
                result.name = heading ? heading.firstChild.textContent.trim() : '';

                const rankMatch = bodyText.match(/总档位[：:]\s*(\d+)/);
                result.totalRank = rankMatch ? rankMatch[1] : '';

                const lines = bodyText.split('\n');
                const attrLabels = ['体质', '力量', '防御', '敏捷', '魔法'];
                result.attributes = {};
                attrLabels.forEach(label => {
                    const idx = lines.indexOf(label);
                    if (idx !== -1 && idx + 1 < lines.length) {
                        const val = lines[idx + 1].trim();
                        if (/^\d+$/.test(val)) {
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
                        if (/^\d+$/.test(val)) {
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

            return {**monster_info, **detail}
        except Exception as e:
            if attempt < max_retries - 1:
                wait_time = (attempt + 1) * 3 + random.randint(0, 2)
                await asyncio.sleep(wait_time)
                continue
            else:
                return monster_info

async def main():
    with open(OUTPUT_FILE, 'r', encoding='utf-8') as f:
        monsters = json.load(f)

    needs_fetch = []
    for m in monsters:
        attrs = m.get('attributes', {})
        if not attrs or not any(attrs.values()) if attrs else True:
            needs_fetch.append(m)

    print(f"需要补采: {len(needs_fetch)} 个魔物")
    print(f"请求间隔: {REQUEST_INTERVAL}秒")

    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        context = await browser.new_context(
            user_agent=random.choice(USER_AGENTS),
            viewport={"width": 1920, "height": 1080},
        )
        page = await context.new_page()

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

            if (i + 1) % BATCH_SIZE == 0:
                with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
                    json.dump(monsters, f, ensure_ascii=False, indent=2)
                print(f"\n=== 批次完成: {success} 成功, {failed} 失败 ===")
                wait_time = 30
                print(f"等待 {wait_time} 秒后继续...")
                await asyncio.sleep(wait_time)

            interval = REQUEST_INTERVAL + random.uniform(-0.5, 1.5)
            await asyncio.sleep(interval)

        with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
            json.dump(monsters, f, ensure_ascii=False, indent=2)

        print(f"\n补采完成! 成功: {success}, 失败: {failed}")
        await browser.close()

if __name__ == "__main__":
    asyncio.run(main())