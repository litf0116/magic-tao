#!/usr/bin/env python3
"""
魔力宝贝装备数据收集脚本
从魔力百科 (molibaike.com) 抓取装备数据
"""

import requests
from bs4 import BeautifulSoup
import json
import time
import os
from typing import Dict, List, Optional
from urllib.parse import urljoin
import re

# 配置
BASE_URL = "https://www.molibaike.com"
OUTPUT_DIR = "docs/装备查询系统/data"
DELAY = 1  # 请求间隔（秒）

# 创建输出目录
os.makedirs(OUTPUT_DIR, exist_ok=True)

# 装备类型映射
EQUIPMENT_TYPES = {
    "0": "武器",
    "1": "防具",
    "2": "首饰",
    "3": "料理",
    "4": "血瓶",
    "5": "宝石",
    "6": "属性水晶",
    "7": "伐木",
    "8": "狩猎",
    "9": "布料",
    "10": "挖掘",
    "11": "其它",
}

# 武器子类型
WEAPON_SUBTYPES = ["剑类", "杖类", "枪类", "斧类", "弓类", "回力镖", "小刀"]

# 防具子类型
ARMOR_SUBTYPES = ["长袍", "铠甲", "靴子", "头盔", "鞋子", "衣服", "盾牌", "帽子"]

# 首饰子类型
ACCESSORY_SUBTYPES = ["乐器", "项链", "头饰", "戒指", "耳环", "头带", "护身符", "手环"]

# 属性水晶子类型
CRYSTAL_SUBTYPES = ["聚能", "强袭", "希望", "普通", "怨念", "元素"]


def get_session():
    """创建请求会话"""
    session = requests.Session()
    session.headers.update(
        {
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
        }
    )
    return session


def fetch_page(session: requests.Session, url: str) -> Optional[str]:
    """获取页面内容"""
    try:
        print(f"正在抓取: {url}")
        response = session.get(url, timeout=10)
        response.raise_for_status()
        response.encoding = response.apparent_encoding
        return response.text
    except Exception as e:
        print(f"抓取失败 {url}: {e}")
        return None


def parse_equipment_list(html: str) -> List[Dict]:
    """解析装备列表页"""
    soup = BeautifulSoup(html, "html.parser")
    equipment_list = []

    # 查找所有装备卡片
    equipment_cards = soup.find_all("div", class_="equipment-item")

    for card in equipment_cards:
        try:
            # 提取装备ID
            link = card.find("a")
            if not link:
                continue

            href = link.get("href", "")
            equipment_id = re.search(r"/(\d+)", href)
            if not equipment_id:
                continue

            equipment_id = equipment_id.group(1)

            # 提取装备名称
            name_elem = card.find("h3") or card.find("h4") or card.find("h5")
            name = name_elem.text.strip() if name_elem else ""

            # 提取类型和等级
            type_elem = card.find(class_="equipment-type")
            type_info = type_elem.text.strip() if type_elem else ""

            # 提取图片
            img_elem = card.find("img")
            image_url = img_elem.get("src", "") if img_elem else ""
            if image_url and not image_url.startswith("http"):
                image_url = urljoin(BASE_URL, image_url)

            # 提取属性
            attributes = {}
            attr_elem = card.find(class_="equipment-attributes")
            if attr_elem:
                for attr in attr_elem.find_all("span"):
                    attr_text = attr.text.strip()
                    if "⚔️" in attr_text:
                        attributes["attack"] = (
                            re.search(r"\d+", attr_text).group()
                            if re.search(r"\d+", attr_text)
                            else 0
                        )
                    elif "🛡️" in attr_text:
                        attributes["defense"] = (
                            re.search(r"\d+", attr_text).group()
                            if re.search(r"\d+", attr_text)
                            else 0
                        )
                    elif "💨" in attr_text:
                        attributes["agility"] = (
                            re.search(r"\d+", attr_text).group()
                            if re.search(r"\d+", attr_text)
                            else 0
                        )

            # 提取等级
            level_match = re.search(r"LV(\d+)", type_info)
            level = int(level_match.group(1)) if level_match else 0

            # 提取品质
            quality = "普通"
            if "优秀" in type_info:
                quality = "优秀"
            elif "精良" in type_info:
                quality = "精良"
            elif "传说" in type_info:
                quality = "传说"
            elif "史诗" in type_info:
                quality = "史诗"

            equipment_list.append(
                {
                    "id": equipment_id,
                    "name": name,
                    "type": "",  # 待解析
                    "subType": "",
                    "level": level,
                    "quality": quality,
                    "imageUrl": image_url,
                    "attributes": attributes,
                    "detailUrl": urljoin(BASE_URL, href),
                }
            )
        except Exception as e:
            print(f"解析装备失败: {e}")
            continue

    return equipment_list


def parse_equipment_detail(html: str, equipment_id: str) -> Optional[Dict]:
    """解析装备详情页"""
    soup = BeautifulSoup(html, "html.parser")

    try:
        # 提取装备名称
        name_elem = soup.find("h1") or soup.find("h2", class_="title")
        name = name_elem.text.strip() if name_elem else ""

        # 提取类型和子类型
        type_elem = soup.find(class_="equipment-type")
        type_info = type_elem.text.strip() if type_elem else ""

        # 提取等级
        level_match = re.search(r"等级[：:]\s*(\d+)", html)
        level = int(level_match.group(1)) if level_match else 0

        # 提取品质
        quality_elem = soup.find(class_="equipment-quality")
        quality = quality_elem.text.strip() if quality_elem else "普通"

        # 提取图片
        img_elem = soup.find(class_="equipment-image") or soup.find(
            "img", class_="main-image"
        )
        image_url = img_elem.get("src", "") if img_elem else ""
        if image_url and not image_url.startswith("http"):
            image_url = urljoin(BASE_URL, image_url)

        # 提取属性
        attributes = {}
        attr_table = soup.find("table", class_="attributes-table")
        if attr_table:
            rows = attr_table.find_all("tr")
            for row in rows:
                cells = row.find_all("td")
                if len(cells) >= 2:
                    attr_name = cells[0].text.strip()
                    attr_value = cells[1].text.strip()

                    if "攻击力" in attr_name or "攻击" in attr_name:
                        attributes["attack"] = (
                            re.search(r"\d+", attr_value).group()
                            if re.search(r"\d+", attr_value)
                            else 0
                        )
                    elif "防御力" in attr_name or "防御" in attr_name:
                        attributes["defense"] = (
                            re.search(r"\d+", attr_value).group()
                            if re.search(r"\d+", attr_value)
                            else 0
                        )
                    elif "魔法攻击" in attr_name:
                        attributes["magicAttack"] = (
                            re.search(r"\d+", attr_value).group()
                            if re.search(r"\d+", attr_value)
                            else 0
                        )
                    elif "魔法防御" in attr_name:
                        attributes["magicDefense"] = (
                            re.search(r"\d+", attr_value).group()
                            if re.search(r"\d+", attr_value)
                            else 0
                        )
                    elif "敏捷" in attr_name:
                        attributes["agility"] = (
                            re.search(r"\d+", attr_value).group()
                            if re.search(r"\d+", attr_value)
                            else 0
                        )
                    elif "命中" in attr_name:
                        attributes["hitRate"] = (
                            re.search(r"\d+", attr_value).group()
                            if re.search(r"\d+", attr_value)
                            else 0
                        )
                    elif "闪避" in attr_name:
                        attributes["dodgeRate"] = (
                            re.search(r"\d+", attr_value).group()
                            if re.search(r"\d+", attr_value)
                            else 0
                        )

        # 提取描述
        desc_elem = soup.find(class_="equipment-description")
        description = desc_elem.text.strip() if desc_elem else ""

        # 提取特殊效果
        special_effect = ""
        special_elem = soup.find(class_="special-effect")
        if special_elem:
            special_effect = special_elem.text.strip()

        # 提取适用职业
        requirements = {}
        class_elem = soup.find(class_="requirements")
        if class_elem:
            class_list = class_elem.find_all("span", class_="class-tag")
            if class_list:
                requirements["classes"] = [c.text.strip() for c in class_list]

            level_req_elem = class_elem.find(class_="level-requirement")
            if level_req_elem:
                level_match = re.search(r"等级[：:]\s*(\d+)", level_req_elem.text)
                if level_match:
                    requirements["level"] = int(level_match.group(1))

        # 提取掉落位置
        drop_locations = []
        drop_elem = soup.find(class_="drop-locations")
        if drop_elem:
            locations = drop_elem.find_all("li")
            drop_locations = [loc.text.strip() for loc in locations]

        # 提取合成配方
        synthesis = None
        synthesis_elem = soup.find(class_="synthesis")
        if synthesis_elem:
            materials = []
            material_list = synthesis_elem.find_all("li", class_="material")
            for mat in material_list:
                mat_name = (
                    mat.find(class_="material-name").text.strip()
                    if mat.find(class_="material-name")
                    else ""
                )
                mat_count = re.search(r"(\d+)", mat.text)
                mat_count = int(mat_count.group(1)) if mat_count else 0
                if mat_name and mat_count:
                    materials.append({"name": mat_name, "count": mat_count})

            if materials:
                synthesis = {
                    "materials": materials,
                    "goldCost": 0,  # 待提取
                }

        return {
            "id": equipment_id,
            "name": name,
            "type": "",  # 从type_info解析
            "subType": "",
            "level": level,
            "quality": quality,
            "attributes": attributes,
            "specialEffect": special_effect,
            "description": description,
            "imageUrl": image_url,
            "requirements": requirements,
            "synthesis": synthesis,
            "dropLocations": drop_locations,
            "source": "molibaike.com",
        }

    except Exception as e:
        print(f"解析装备详情失败 {equipment_id}: {e}")
        return None


def collect_equipment_by_type(
    session: requests.Session, type_id: str, subtypes: List[str] = None
):
    """按类型收集装备"""
    print(f"\n开始收集类型: {EQUIPMENT_TYPES.get(type_id, type_id)}")

    all_equipments = []

    # 收装备列表
    list_url = f"{BASE_URL}/Equipment?type={type_id}"
    html = fetch_page(session, list_url)
    if not html:
        return

    equipment_list = parse_equipment_list(html)
    print(f"找到 {len(equipment_list)} 件装备")

    # 获取详情
    for idx, equipment in enumerate(equipment_list):
        print(f"正在获取详情 {idx + 1}/{len(equipment_list)}: {equipment['name']}")

        detail_html = fetch_page(session, equipment["detailUrl"])
        if detail_html:
            detail_data = parse_equipment_detail(detail_html, equipment["id"])
            if detail_data:
                all_equipments.append(detail_data)

        time.sleep(DELAY)  # 避免请求过快

    # 保存数据
    if all_equipments:
        type_name = EQUIPMENT_TYPES.get(type_id, f"type_{type_id}")
        filename = f"{OUTPUT_DIR}/equipments_{type_name}.json"
        with open(filename, "w", encoding="utf-8") as f:
            json.dump(all_equipments, f, ensure_ascii=False, indent=2)
        print(f"已保存 {len(all_equipments)} 件装备到 {filename}")

        # 同时保存CSV格式
        csv_filename = f"{OUTPUT_DIR}/equipments_{type_name}.csv"
        save_to_csv(all_equipments, csv_filename)
    else:
        print(f"类型 {type_name} 没有找到装备")


def save_to_csv(equipments: List[Dict], filename: str):
    """保存为CSV格式"""
    import csv

    if not equipments:
        return

    # 获取所有可能的字段
    fieldnames = [
        "id",
        "name",
        "type",
        "subType",
        "level",
        "quality",
        "attack",
        "defense",
        "magicAttack",
        "magicDefense",
        "agility",
        "hitRate",
        "dodgeRate",
        "specialEffect",
        "description",
        "imageUrl",
        "requirements_level",
        "requirements_classes",
        "synthesis_materials",
        "dropLocations",
        "source",
    ]

    with open(filename, "w", encoding="utf-8-sig", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()

        for equip in equipments:
            row = {
                "id": equip.get("id"),
                "name": equip.get("name"),
                "type": equip.get("type"),
                "subType": equip.get("subType"),
                "level": equip.get("level"),
                "quality": equip.get("quality"),
                "attack": equip.get("attributes", {}).get("attack", ""),
                "defense": equip.get("attributes", {}).get("defense", ""),
                "magicAttack": equip.get("attributes", {}).get("magicAttack", ""),
                "magicDefense": equip.get("attributes", {}).get("magicDefense", ""),
                "agility": equip.get("attributes", {}).get("agility", ""),
                "hitRate": equip.get("attributes", {}).get("hitRate", ""),
                "dodgeRate": equip.get("attributes", {}).get("dodgeRate", ""),
                "specialEffect": equip.get("specialEffect", ""),
                "description": equip.get("description", ""),
                "imageUrl": equip.get("imageUrl", ""),
                "requirements_level": equip.get("requirements", {}).get("level", ""),
                "requirements_classes": ",".join(
                    equip.get("requirements", {}).get("classes", [])
                )
                if equip.get("requirements")
                else "",
                "synthesis_materials": json.dumps(
                    equip.get("synthesis", {}).get("materials", []), ensure_ascii=False
                )
                if equip.get("synthesis")
                else "",
                "dropLocations": ",".join(equip.get("dropLocations", []))
                if equip.get("dropLocations")
                else "",
                "source": equip.get("source", ""),
            }
            writer.writerow(row)

    print(f"已保存CSV到 {filename}")


def collect_all():
    """收集所有装备"""
    session = get_session()

    # 收集主要装备类型
    main_types = {
        "0": ("武器", WEAPON_SUBTYPES),
        "1": ("防具", ARMOR_SUBTYPES),
        "2": ("首饰", ACCESSORY_SUBTYPES),
        "6": ("属性水晶", CRYSTAL_SUBTYPES),
    }

    print("=" * 60)
    print("魔力宝贝装备数据收集工具")
    print("=" * 60)

    for type_id, (type_name, subtypes) in main_types.items():
        collect_equipment_by_type(session, type_id, subtypes)
        time.sleep(DELAY * 2)  # 类型间延迟

    print("\n" + "=" * 60)
    print("数据收集完成！")
    print(f"数据保存在: {OUTPUT_DIR}")
    print("=" * 60)


if __name__ == "__main__":
    collect_all()
