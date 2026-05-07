#!/usr/bin/env python3
"""
更新魔物索引脚本
"""

import json
import os

DATA_FILE = "monsters_data.json"
KB_ROOT = os.path.expanduser("~/Documents/Obsidian/magic-tao-kb/crossgate-knowledge/pets")

def main():
    with open(DATA_FILE, 'r', encoding='utf-8') as f:
        monsters = json.load(f)

    complete = [m for m in monsters if m.get('attributes') and any(m['attributes'].values())]

    by_race = {}
    for m in complete:
        race = m.get('type', '未知')
        if race not in by_race:
            by_race[race] = []
        by_race[race].append(m)

    index_content = ["# 魔物总索引\n", f"共有 {len(complete)} 个魔物已收录完整数据\n\n"]

    for race, monsters_list in sorted(by_race.items()):
        race_dir = {
            '野兽系': 'beasts', '飞行系': 'flying', '龙系': 'dragon',
            '不死系': 'undead', '植物系': 'plants', '昆虫系': 'insect',
            '特殊系': 'special', '金属系': 'metal', '人形系': 'humanoid', '邪魔系': 'demon'
        }.get(race, 'unknown')

        index_content.append(f"## {race} ({len(monsters_list)}个)\n\n")

        for m in sorted(monsters_list, key=lambda x: x.get('name', '')):
            name = m.get('name', '未知')
            attrs = m.get('attributes', {})
            total = sum(int(v) for v in attrs.values()) if attrs else 0
            index_content.append(f"- [[{name}]] - 总档:{total}\n")

        index_content.append("\n")

    with open(os.path.join(KB_ROOT, "_index/index.md"), 'w', encoding='utf-8') as f:
        f.writelines(index_content)

    print(f"索引已更新: {len(complete)} 个魔物")

if __name__ == "__main__":
    main()