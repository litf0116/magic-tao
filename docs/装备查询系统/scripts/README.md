# 装备数据收集指南

## 📋 概述

本目录包含从魔力宝贝相关网站收集装备数据的脚本和工具。

## 🚀 快速开始

### 前置要求

```bash
# 安装Python依赖
pip install requests beautifulsoup4 lxml

# 或使用项目虚拟环境
python -m venv venv
source venv/bin/activate  # Linux/Mac
venv\Scripts\activate     # Windows
pip install -r requirements.txt
```

### 运行脚本

```bash
# 收集所有装备数据
python scripts/collect_equipment_data.py

# 只收集特定类型（修改脚本中的collect_all函数）
# collect_equipment_by_type(session, "0")  # 武器
# collect_equipment_by_type(session, "1")  # 防具
# collect_equipment_by_type(session, "2")  # 首饰
# collect_equipment_by_type(session, "6")  # 属性水晶
```

## 📂 数据结构

### 输出文件

```
data/
├── equipments_武器.json       # 武器装备数据
├── equipments_武器.csv        # 武器装备CSV
├── equipments_防具.json       # 防具装备数据
├── equipments_防具.csv        # 防具装备CSV
├── equipments_首饰.json       # 首饰装备数据
├── equipments_首饰.csv        # 首饰装备CSV
├── equipments_属性水晶.json   # 属性水晶数据
└── equipments_属性水晶.csv    # 属性水晶CSV
```

### 数据格式

#### JSON格式

```json
{
  "id": "123",
  "name": "威力短弓",
  "type": "武器",
  "subType": "弓类",
  "level": 1,
  "quality": "普通",
  "attributes": {
    "attack": 15,
    "defense": 0,
    "agility": 5,
    "hitRate": 10,
    "dodgeRate": 0
  },
  "specialEffect": "",
  "description": "这是一把基础的短弓，适合新手使用。",
  "imageUrl": "https://molibaike.com/...",
  "requirements": {
    "level": 1,
    "classes": ["弓箭手", "忍者"]
  },
  "synthesis": {
    "materials": [
      {"name": "杉", "count": 10},
      {"name": "布料", "count": 5}
    ],
    "goldCost": 1000
  },
  "dropLocations": [
    "法兰城周边",
    "新手任务奖励"
  ],
  "source": "molibaike.com"
}
```

#### CSV格式

```
id,name,type,subType,level,quality,attack,defense,agility,hitRate,dodgeRate,specialEffect,description,imageUrl,requirements_level,requirements_classes,synthesis_materials,dropLocations,source
123,威力短弓,武器,弓类,1,普通,15,0,5,10,0,,这是一把基础的短弓,适合新手使用。,https://...,1,"弓箭手,忍者","[{""name"":""杉"",""count"":10}]","法兰城周边,新手任务奖励",molibaike.com
```

## 🔧 配置说明

### 修改请求间隔

在 `collect_equipment_data.py` 中修改：

```python
DELAY = 1  # 请求间隔（秒），默认1秒
```

### 添加新的装备类型

在脚本中添加新的类型映射：

```python
EQUIPMENT_TYPES = {
    "0": "武器",
    "1": "防具",
    "2": "首饰",
    "6": "属性水晶",
    "11": "其它"  # 新增
}
```

### 自定义输出目录

```python
OUTPUT_DIR = "docs/装备查询系统/data"  # 修改输出路径
```

## 📊 数据统计

收集完成后，可以使用以下命令查看统计信息：

```bash
# 统计装备数量
python scripts/collect_equipment_data.py --stats

# 或者使用jq（Linux/Mac）
cat data/*.json | jq '. | length' | awk '{sum+=$1} END {print sum}'
```

## ⚠️ 注意事项

1. **请求频率**：脚本已设置1秒间隔，避免请求过快被封IP
2. **数据准确性**：建议人工抽检部分数据的准确性
3. **网站结构变化**：如果网站改版，需要更新解析逻辑
4. **版权信息**：收集的数据仅供学习使用

## 🔄 更新数据

定期更新装备数据：

```bash
# 备份现有数据
cp -r data data.backup

# 重新收集
python scripts/collect_equipment_data.py

# 对比差异
diff data backup/data
```

## 📝 数据字段说明

| 字段 | 类型 | 说明 |
|------|------|------|
| id | string | 装备唯一ID |
| name | string | 装备名称 |
| type | string | 主类型（武器、防具、首饰、属性水晶） |
| subType | string | 子类型（剑类、弓类、长袍等） |
| level | integer | 装备等级（1-9） |
| quality | string | 品质（普通、优秀、精良、传说、史诗） |
| attributes | object | 属性对象 |
| attributes.attack | integer | 攻击力 |
| attributes.defense | integer | 防御力 |
| attributes.magicAttack | integer | 魔法攻击 |
| attributes.magicDefense | integer | 魔法防御 |
| attributes.agility | integer | 敏捷 |
| attributes.hitRate | integer | 命中率 |
| attributes.dodgeRate | integer | 闪避率 |
| specialEffect | string | 特殊效果描述 |
| description | string | 装备描述 |
| imageUrl | string | 装备图片URL |
| requirements.level | integer | 使用等级要求 |
| requirements.classes | array | 适用职业列表 |
| synthesis.materials | array | 合成材料列表 |
| synthesis.goldCost | integer | 合成金币消耗 |
| dropLocations | array | 掉落位置列表 |
| source | string | 数据来源 |

## 🐛 故障排除

### 问题1：连接超时

**解决方案**：增加超时时间
```python
response = session.get(url, timeout=30)  # 增加到30秒
```

### 问题2：编码问题

**解决方案**：确保使用正确的编码
```python
response.encoding = response.apparent_encoding
```

### 问题3：解析失败

**解决方案**：打印HTML内容进行分析
```python
print(html)  # 查看实际获取的HTML结构
```

## 📚 参考资料

- 魔力百科: https://www.molibaike.com/
- Requests文档: https://requests.readthedocs.io/
- BeautifulSoup文档: https://www.crummy.com/software/BeautifulSoup/bs4/doc/

---

**最后更新**: 2026-03-07