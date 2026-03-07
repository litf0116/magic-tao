#!/usr/bin/env python3
"""
魔力宝贝装备数据收集脚本（简化版）
从魔力百科 (molibaike.com) 抓取装备数据
"""

import requests
from bs4 import BeautifulSoup
import json
import time
import os
from urllib.parse import urljoin
import re

# 配置
BASE_URL = "https://www.molibaike.com"
OUTPUT_DIR = "docs/装备查询系统/data"
DELAY = 2  # 请求间隔（秒）

# 创建输出目录
os.makedirs(OUTPUT_DIR, exist_ok=True)


def get_session():
    """创建请求会话"""
    session = requests.Session()
    session.headers.update(
        {
            "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36"
        }
    )
    return session


def fetch_page(session, url):
    """获取页面内容"""
    try:
        print(f"正在获取: {url}")
        response = session.get(url, timeout=15)
        response.raise_for_status()
        response.encoding = "utf-8"
        return response.text
    except Exception as e:
        print(f"获取失败 {url}: {e}")
        return None


def parse_equipment_page(html):
    """解析装备页面"""
    soup = BeautifulSoup(html, "html.parser")
    equipment_list = []

    # 查找所有装备项
    # 根据实际的HTML结构调整选择器
    items = soup.find_all("div", class_="col-md-3")
    if not items:
        items = soup.find_all("div", class_="item")
    if not items:
        items = soup.find_all("a", href=re.compile(r"/Item/"))

    print(f"找到 {len(items)} 个装备项")

    for item in items[:10]:  # 先只处理前10个测试
        try:
            # 提取链接
            link = item.find("a") if item.name != "a" else item
            if not link:
                continue

            href = link.get("href")
            if not href:
                continue

            # 提取ID
            id_match = re.search(r"/(\d+)/?\??.*$", href)
            if not id_match:
                continue
            equipment_id = id_match.group(1)

            # 提取名称
            name_elem = link.find("img") or item.find("span")
            if name_elem and name_elem.name == "img":
                name = name_elem.get("alt", "")
            elif name_elem:
                name = name_elem.get_text(strip=True)
            else:
                name = link.get("title", "")

            # 提取图片
            img = item.find("img")
            if img:
                image_url = img.get("src", "")
                if image_url and not image_url.startswith("http"):
                    image_url = urljoin(BASE_URL, image_url)
            else:
                image_url = ""

            # 提取类型和等级
            info_text = item.get_text(strip=True)

            # 简单匹配等级
            level_match = re.search(r"等级[:：]\s*(\d+)", info_text)
            level = int(level_match.group(1)) if level_match else 0

            equipment = {
                "id": equipment_id,
                "name": name,
                "level": level,
                "imageUrl": image_url,
                "detailUrl": urljoin(BASE_URL, href),
                "type": "待解析",
                "quality": "普通",
                "attributes": {},
            }

            equipment_list.append(equipment)
            print(f"  - {name} (ID: {equipment_id})")

        except Exception as e:
            print(f"  解析失败: {e}")
            continue

    return equipment_list


def get_equipment_detail(session, url):
    """获取装备详情"""
    html = fetch_page(session, url)
    if not html:
        return None

    soup = BeautifulSoup(html, "html.parser")

    # 提取名称
    name_elem = soup.find("h1") or soup.find("h2")
    name = name_elem.get_text(strip=True) if name_elem else ""

    # 提取描述
    desc_elem = soup.find("p") or soup.find("div", class_="description")
    description = desc_elem.get_text(strip=True) if desc_elem else ""

    # 提取属性（根据实际HTML结构调整）
    attributes = {}
    # 这里需要根据实际页面结构来解析

    return {"name": name, "description": description, "attributes": attributes}


def collect_sample_data():
    """收集示例数据"""
    session = get_session()

    print("=" * 60)
    print("魔力宝贝装备数据收集工具")
    print("=" * 60)

    # 获取装备列表页
    list_url = f"{BASE_URL}/item"
    html = fetch_page(session, list_url)

    if html:
        equipment_list = parse_equipment_page(html)

        # 保存列表
        if equipment_list:
            filename = f"{OUTPUT_DIR}/equipment_list_sample.json"
            with open(filename, "w", encoding="utf-8") as f:
                json.dump(equipment_list, f, ensure_ascii=False, indent=2)
            print(f"\n已保存 {len(equipment_list)} 件装备到 {filename}")

            # 获取前5件的详情
            print("\n获取装备详情...")
            detailed_list = []

            for idx, equip in enumerate(equipment_list[:5]):
                print(f"正在获取详情: {equip['name']}")
                detail = get_equipment_detail(session, equip["detailUrl"])
                if detail:
                    equip.update(detail)
                    detailed_list.append(equip)
                time.sleep(DELAY)

            # 保存详情
            if detailed_list:
                detail_filename = f"{OUTPUT_DIR}/equipment_detail_sample.json"
                with open(detail_filename, "w", encoding="utf-8") as f:
                    json.dump(detailed_list, f, ensure_ascii=False, indent=2)
                print(f"已保存 {len(detailed_list)} 件装备详情到 {detail_filename}")

    print("\n" + "=" * 60)
    print("数据收集完成！")
    print("=" * 60)


if __name__ == "__main__":
    collect_sample_data()
