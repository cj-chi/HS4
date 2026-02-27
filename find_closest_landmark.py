# Find landmark(s) closest to target (e.g. 0.715, 0.6505)
import sys
target = (0.715, 0.6505)
if len(sys.argv) >= 3:
    target = (float(sys.argv[1]), float(sys.argv[2]))

lines = open("landmarks_coords.txt", encoding="utf-8").readlines()
pts = []
for line in lines[1:]:
    line = line.strip()
    if not line:
        continue
    parts = line.split(",")
    if len(parts) >= 3:
        idx = int(parts[0])
        x, y = float(parts[1]), float(parts[2])
        d = (x - target[0]) ** 2 + (y - target[1]) ** 2
        pts.append((idx, x, y, d))

pts.sort(key=lambda t: t[3])
print("Closest 15 landmarks to", target, "(raw image coords [0,1]):")
print("index      x        y      distance")
for t in pts[:15]:
    print(f"{t[0]:4d}  {t[1]:.6f}  {t[2]:.6f}  {t[3]**0.5:.6f}")

# Right-side face (x > 0.4, 0.5 < y < 0.75)
right = [(t[0], t[1], t[2]) for t in pts if t[1] > 0.4 and 0.5 < t[2] < 0.75]
right.sort(key=lambda t: -t[1])
print("\nRight-side face (x>0.4, 0.5<y<0.75), by x desc:")
for t in right[:12]:
    print(f"  index {t[0]:3d}  x={t[1]:.4f}  y={t[2]:.4f}")
