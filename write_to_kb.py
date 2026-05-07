#!/usr/bin/env python3
"""
魔物数据写入Obsidian知识库脚本
"""

import json
import os
import re

DATA_FILE = "monsters_data.json"
KB_ROOT = os.path.expanduser("~/Documents/Obsidian/magic-tao-kb/crossgate-knowledge/pets")

RACE_MAP = {
    '野兽系': 'beasts',
    '飞行系': 'flying',
    '龙系': 'dragon',
    '不死系': 'undead',
    '植物系': 'plants',
    '昆虫系': 'insect',
    '特殊系': 'special',
    '金属系': 'metal',
    '人形系': 'humanoid',
    '邪魔系': 'demon'
}

def sanitize_filename(name):
    """清理文件名"""
    name = re.sub(r'[<>:"/\\|?*]', '', name)
    return name[:100]

def extract_number_from_name(name):
    """从名称提取编号"""
    match = re.search(r'(\d+)', name)
    return match.group(1) if match else ""

def monster_to_markdown(monster):
    """将魔物数据转换为Markdown格式"""
    name = monster.get('name', '未知魔物')
    race = monster.get('type', '未知')
    race_dir = RACE_MAP.get(race, 'unknown')

    attrs = monster.get('attributes', {})
    resist = monster.get('resistance', {})
    details = monster.get('details', {})

    lines = [
        "---",
        f"title: {name}",
        f"type: monster",
        f"race: {race}",
        f"race_code: {race_dir}",
    ]

    if monster.get('totalRank'):
        lines.append(f"total_rank: {monster['totalRank']}")

    lines.extend([
        "---",
        "",
        f"# {name}",
        "",
        "## 基本属性",
        "",
        "| 属性 | 值 |",
        "|------|-----|",
    ])

    attr_names = {'体质': 'HP', '力量': '攻击', '防御': '防御', '敏捷': '敏捷', '魔法': '魔法'}
    for attr_key, attr_label in attr_names.items():
        val = attrs.get(attr_key, '0')
        lines.append(f"| {attr_label} | {val} |")

    if resist:
        lines.extend([
            "",
            "## 属性抗性",
            "",
            "| 属性 | 抗性 |",
            "|------|------|",
            f"| 地 | {resist.get('地', '0')} |",
            f"| 水 | {resist.get('水', '0')} |",
            f"| 火 | {resist.get('火', '0')} |",
            f"| 风 | {resist.get('风', '0')} |",
        ])

    if details:
        lines.extend([
            "",
            "## 详细信息",
            "",
        ])
        for key, val in details.items():
            if key and val:
                lines.append(f"- **{key}**: {val}")

    lines.extend([
        "",
        "## 元数据",
        f"- 种族: [[{race}]]",
        f"- 数据来源: [[魔力百科]]",
        "",
        "---",
        "",
        f"![[{name}]]",
    ])

    return "\n".join(lines)

def main():
    with open(DATA_FILE, 'r', encoding='utf-8') as f:
        monsters = json.load(f)

    complete = [m for m in monsters if m.get('attributes') and any(m['attributes'].values())]
    print(f"准备写入 {len(complete)} 个魔物到知识库...")

    for i, monster in enumerate(complete):
        race = monster.get('type', '未知')
        race_dir = RACE_MAP.get(race, 'unknown')
        name = monster.get('name', 'unknown')

        dir_path = os.path.join(KB_ROOT, race_dir)
        os.makedirs(dir_path, exist_ok=True)

        filename = sanitize_filename(name) + ".md"
        filepath = os.path.join(dir_path, filename)

        content = monster_to_markdown(monster)

        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)

        if (i + 1) % 20 == 0:
            print(f"  已写入 {i+1}/{len(complete)}")

    print(f"完成! 共写入 {len(complete)} 个魔物文件")

if __name__ == "__main__":
    main()