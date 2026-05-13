#!/usr/bin/env python3
"""
生成符合中国软件著作权申请规范的源代码PDF

规范要求：
- 页眉：软件名称 + 版本号（左对齐）
- 页脚：第X页（居中）
- 每页50行代码
- 行号左对齐，代码右对齐
- 等宽字体（Courier或宋体）
"""

import os
import re
from reportlab.lib.pagesizes import A4
from reportlab.lib.units import cm, mm
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

# PDF配置
PAGE_WIDTH, PAGE_HEIGHT = A4
MARGIN_LEFT = 2.5 * cm
MARGIN_RIGHT = 2.5 * cm
MARGIN_TOP = 2.5 * cm
MARGIN_BOTTOM = 2.5 * cm

# 代码区域
CODE_LEFT = MARGIN_LEFT
CODE_RIGHT = PAGE_WIDTH - MARGIN_RIGHT
CODE_TOP = PAGE_HEIGHT - MARGIN_TOP - 1 * cm  # 页眉下方
CODE_BOTTOM = MARGIN_BOTTOM + 1 * cm  # 页脚上方
CODE_WIDTH = CODE_RIGHT - CODE_LEFT
CODE_HEIGHT = CODE_TOP - CODE_BOTTOM

# 字体配置
FONT_SIZE = 9  # 五号字约9pt
LINE_HEIGHT = FONT_SIZE + 4  # 行高
LINES_PER_PAGE = 50  # 每页50行

# 行号宽度
LINE_NUM_WIDTH = 1.5 * cm

# 软件信息
SOFTWARE_NAME = "魔力淘"
VERSION = "V1.0.0"
COPYRIGHT_HOLDER = "黑龙江省魔淡网络科技有限公司"


def draw_header(c, page_num):
    """绘制页眉"""
    # 页眉文字：软件名称 + 版本号
    header_text = f"{SOFTWARE_NAME} {VERSION}"
    c.setFont("Helvetica", 9)
    c.setFillColorRGB(0.3, 0.3, 0.3)
    c.drawString(MARGIN_LEFT, PAGE_HEIGHT - 1.5 * cm, header_text)
    
    # 页眉下划线
    c.setStrokeColorRGB(0.7, 0.7, 0.7)
    c.setLineWidth(0.5)
    c.line(MARGIN_LEFT, PAGE_HEIGHT - 1.8 * cm, 
           PAGE_WIDTH - MARGIN_RIGHT, PAGE_HEIGHT - 1.8 * cm)


def draw_footer(c, page_num, total_pages):
    """绘制页脚"""
    # 页脚文字：第X页
    footer_text = f"第{page_num}页"
    c.setFont("Helvetica", 9)
    c.setFillColorRGB(0.3, 0.3, 0.3)
    
    # 居中
    text_width = c.stringWidth(footer_text, "Helvetica", 9)
    c.drawString((PAGE_WIDTH - text_width) / 2, 1 * cm, footer_text)
    
    # 页脚上划线
    c.setStrokeColorRGB(0.7, 0.7, 0.7)
    c.setLineWidth(0.5)
    c.line(MARGIN_LEFT, 1.5 * cm, PAGE_WIDTH - MARGIN_RIGHT, 1.5 * cm)


def draw_code_line(c, line_num, code_text, y_pos):
    """绘制单行代码（带行号）"""
    # 行号
    c.setFont("Courier", FONT_SIZE)
    c.setFillColorRGB(0.5, 0.5, 0.5)
    line_num_str = f"{line_num:5d}"
    c.drawString(CODE_LEFT, y_pos, line_num_str)
    
    # 代码文本（限制长度，避免超出页面）
    max_chars = int((CODE_WIDTH - LINE_NUM_WIDTH) / (FONT_SIZE * 0.6))  # Courier约0.6em宽
    if len(code_text) > max_chars:
        code_text = code_text[:max_chars-3] + "..."
    
    c.setFillColorRGB(0, 0, 0)
    c.drawString(CODE_LEFT + LINE_NUM_WIDTH, y_pos, code_text)


def extract_code_from_md(md_content):
    """从markdown文件提取纯代码行"""
    lines = []
    for line in md_content.split('\n'):
        # 跳过markdown标记行
        if line.startswith('# ') or line.startswith('> ') or line.startswith('---'):
            continue
        if '第' in line and '页结束' in line:
            continue
        # 保留代码行（去掉行号前缀）
        # 格式如 "10: class AppConstants {"
        if ': ' in line:
            # 尝试提取冒号后的内容
            parts = line.split(': ', 1)
            if len(parts) == 2:
                code = parts[1]
                lines.append(code)
            else:
                lines.append(line)
        else:
            lines.append(line)
    return lines


def generate_pdf(front_file, back_file, output_file):
    """生成源代码PDF"""
    # 读取源代码
    with open(front_file, 'r', encoding='utf-8') as f:
        front_content = f.read()
    with open(back_file, 'r', encoding='utf-8') as f:
        back_content = f.read()
    
    # 提取代码行
    front_lines = extract_code_from_md(front_content)
    back_lines = extract_code_from_md(back_content)
    
    # 合并所有代码行
    all_lines = front_lines + back_lines
    
    # 计算总页数
    total_pages = (len(all_lines) + LINES_PER_PAGE - 1) // LINES_PER_PAGE
    
    # 创建PDF
    c = canvas.Canvas(output_file, pagesize=A4)
    
    # 逐页绘制
    for page_idx in range(total_pages):
        page_num = page_idx + 1
        
        # 绘制页眉
        draw_header(c, page_num)
        
        # 绘制页脚
        draw_footer(c, page_num, total_pages)
        
        # 计算当前页的代码行范围
        start_line = page_idx * LINES_PER_PAGE
        end_line = min(start_line + LINES_PER_PAGE, len(all_lines))
        
        # 绘制代码行
        y_pos = CODE_TOP - LINE_HEIGHT
        for i in range(start_line, end_line):
            line_num = i + 1  # 行号从1开始
            code_text = all_lines[i]
            draw_code_line(c, line_num, code_text, y_pos)
            y_pos -= LINE_HEIGHT
        
        # 换页
        c.showPage()
    
    # 保存PDF
    c.save()
    print(f"✅ 已生成源代码PDF: {output_file}")
    print(f"   总行数: {len(all_lines)}")
    print(f"   总页数: {total_pages}")


if __name__ == "__main__":
    # 文件路径
    base_dir = os.path.dirname(os.path.abspath(__file__))
    front_file = os.path.join(base_dir, "源代码前30页.md")
    back_file = os.path.join(base_dir, "源代码后30页.md")
    output_file = os.path.join(base_dir, "源代码.pdf")
    
    # 生成PDF
    generate_pdf(front_file, back_file, output_file)
