from reportlab.lib.pagesizes import A4
from reportlab.lib.units import mm
from reportlab.pdfgen import canvas
import os

# 页面尺寸 A4
PAGE_WIDTH, PAGE_HEIGHT = A4

# 字体配置 - 缩小字体确保50行能放下
FONT_SIZE = 8.5       # 源代码字体大小
LINE_HEIGHT = 12      # 行高
MARGIN_LEFT = 30*mm   # 左边距（给行号留空间）
MARGIN_RIGHT = 15*mm
MARGIN_TOP = 18*mm
MARGIN_BOTTOM = 15*mm

# 内容区域宽度
CONTENT_WIDTH = PAGE_WIDTH - MARGIN_LEFT - MARGIN_RIGHT

# 每页行数 - 软著要求最少50行
LINES_PER_PAGE = 50

def generate_pdf(source_file, output_file):
    """生成软著源代码PDF"""
    
    # 读取源代码
    with open(source_file, 'r', encoding='utf-8', errors='ignore') as f:
        lines = f.readlines()
    
    # 创建PDF
    c = canvas.Canvas(output_file, pagesize=A4)
    
    page_num = 1
    line_num = 1
    i = 0
    
    while i < len(lines):
        # 添加页眉
        c.setFont("Helvetica", 8)
        c.drawString(MARGIN_LEFT, PAGE_HEIGHT - 12*mm, "魔力淘 V1.0.0")
        c.drawRightString(PAGE_WIDTH - MARGIN_RIGHT, PAGE_HEIGHT - 12*mm, f"第{page_num}页")
        
        # 添加页脚
        c.drawString(MARGIN_LEFT, MARGIN_BOTTOM - 8*mm, "黑龙江省魔淡网络科技有限公司")
        
        # 绘制代码行
        y = PAGE_HEIGHT - MARGIN_TOP
        lines_on_page = 0
        
        while lines_on_page < LINES_PER_PAGE and i < len(lines):
            line = lines[i].rstrip('\n\r')
            i += 1
            lines_on_page += 1
            
            # 绘制行号
            c.setFont("Courier", 7)
            c.drawString(MARGIN_LEFT - 12*mm, y, f"{line_num:5d}")
            line_num += 1
            
            # 绘制代码内容 - 不换行，直接截断超长部分
            c.setFont("Courier", FONT_SIZE)
            max_chars = int(CONTENT_WIDTH / (FONT_SIZE * 0.45))
            if len(line) > max_chars:
                line = line[:max_chars]
            c.drawString(MARGIN_LEFT, y, line)
            y -= LINE_HEIGHT
        
        # 新增页面
        c.showPage()
        page_num += 1
    
    c.save()
    
    total_lines = line_num - 1
    print(f"✅ 生成完成: {output_file}")
    print(f"总页数: {page_num - 1}")
    print(f"总行数: {total_lines}")
    print(f"每页行数: {LINES_PER_PAGE} 行")

if __name__ == "__main__":
    generate_pdf("源代码_100页.txt", "源代码_100页.pdf")
