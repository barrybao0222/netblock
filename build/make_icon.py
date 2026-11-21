# -*- coding: utf-8 -*-
"""NetBlock 图标：蓝色圆角方块 + 白色断网标志"""
import os
from PIL import Image, ImageDraw

S = 1024
img = Image.new("RGB", (S, S), (37, 99, 235))
d = ImageDraw.Draw(img)
for y in range(S):
    t = y / (S - 1)
    c = (int(37 + (59 - 37) * t), int(99 + (130 - 99) * t), int(235 + (246 - 235) * t))
    d.line([(0, y), (S, y)], fill=c)

mask = Image.new("L", (S, S), 0)
ImageDraw.Draw(mask).rounded_rectangle([0, 0, S - 1, S - 1], radius=int(S * 0.23), fill=255)
base = Image.new("RGBA", (S, S), (0, 0, 0, 0))
base.paste(img, (0, 0), mask)
d = ImageDraw.Draw(base)

# 地球
cx, cy, gr = S // 2, S // 2, int(S * 0.28)
w = int(S * 0.045)
white = (255, 255, 255, 255)
d.ellipse([cx - gr, cy - gr, cx + gr, cy + gr], outline=white, width=w)
d.ellipse([cx - gr * 0.40, cy - gr, cx + gr * 0.40, cy + gr], outline=white, width=w)
d.line([cx - gr, cy, cx + gr, cy], fill=white, width=w)
d.arc([cx - gr, cy - gr * 0.55, cx + gr, cy + gr * 0.55], 0, 180, fill=white, width=w)
d.arc([cx - gr, cy - gr * 0.55, cx + gr, cy + gr * 0.55], 180, 360, fill=white, width=w)

# 斜杠：先描白底再画红色，形成切口
sw = int(S * 0.10)
p1 = (cx - gr * 1.25, cy + gr * 1.25)
p2 = (cx + gr * 1.25, cy - gr * 1.25)
d.line([p1, p2], fill=(255, 255, 255, 255), width=int(sw * 1.75))
d.line([p1, p2], fill=(239, 68, 68, 255), width=sw)

sizes = [16, 20, 24, 32, 40, 48, 64, 96, 128, 256]
imgs = [base.resize((n, n), Image.LANCZOS) for n in sizes]
here = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ROOT = here
imgs[-1].save(os.path.join(ROOT, "src", "app.ico"), format="ICO", sizes=[(n, n) for n in sizes])
base.resize((256, 256), Image.LANCZOS).save(os.path.join(ROOT, "docs", "icon-256.png"))
print("icon ok")
