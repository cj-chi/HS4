# -*- coding: utf-8 -*-
"""
Contour preview window: show 2D face contour (and optional photo contour) in a window.
Flow: user closes window = confirmed; then caller can write card and run HS2CharEdit --validate.

照片與圖形對齊：座標系與臉對齊，照片顯示方向修正後，藍/紅輪廓會自然疊在臉上。
若未來支援「旋轉過的照片」：會先偵測臉部朝向，再一併旋轉照片與輪廓，圖形會跟著旋轉以保持對齊。
"""
from pathlib import Path
from datetime import datetime
import numpy as np


def _smooth_contour_closed(contour_xy, num_points=96):
    """
    Smooth a closed contour via periodic spline and resample to num_points.
    Falls back to weighted-average smoothing + resample when scipy is not available.
    contour_xy: (N, 2) array; returns (num_points, 2) in same coord system, or original if failed.
    """
    if contour_xy is None or len(contour_xy) < 4:
        return contour_xy
    pts = np.asarray(contour_xy, dtype=np.float64)
    n = len(pts)
    try:
        from scipy.interpolate import splprep, splev
        pts_closed = np.vstack([pts, pts[0:1]])
        u = np.linspace(0, 1, n + 1)
        tck, _ = splprep([pts_closed[:, 0], pts_closed[:, 1]], u=u, s=0, per=1)
        u_new = np.linspace(0, 1, num_points, endpoint=False)
        return np.column_stack(splev(u_new, tck))
    except Exception:
        pass
    sm = np.zeros_like(pts)
    for i in range(n):
        prev_i = (i - 1) % n
        next_i = (i + 1) % n
        sm[i] = 0.25 * pts[prev_i] + 0.5 * pts[i] + 0.25 * pts[next_i]
    pts = sm
    ts = np.linspace(0, 1, num_points, endpoint=False)
    out = np.zeros((num_points, 2), dtype=np.float64)
    for idx, t in enumerate(ts):
        seg = t * n
        i0 = int(seg) % n
        i1 = (i0 + 1) % n
        frac = seg - int(seg)
        out[idx] = (1 - frac) * pts[i0] + frac * pts[i1]
    return out


def show_contour_preview(contour_xy, photo_contour_xy=None, image_path=None, photo_measurement_segments=None, photo_edge_landmarks_xy=None, photo_all_landmarks_468_xy=None, photo_precise_landmarks_xy=None, photo_facial_feature_edges=None, title="Face contour (frontal)", show=True):
    """
    Open a window and draw the 2D contour. Optionally overlay photo contour, green segments, yellow 50, cyan 468（每點標編號）, 紫色五官邊緣（眼眉唇）.
    If show=False, save snapshot and close figure without plt.show() (for headless log capture).
    """
    try:
        import matplotlib
        # Force GUI backend so plt.show() opens a visible window (e.g. when MPLBACKEND was Agg or running from IDE)
        for backend in ("TkAgg", "Qt5Agg", "WXAgg", "Qt4Agg"):
            try:
                matplotlib.use(backend)
                break
            except Exception:
                continue
        import matplotlib.pyplot as plt
    except ImportError:
        _fallback_tkinter(contour_xy, photo_contour_xy, title)
        return

    fig, ax = plt.subplots(1, 1, figsize=(12, 12))
    ax.set_aspect("equal")
    ax.set_title(title)
    # 先反轉 Y 軸（小 y = 畫面上方），再畫圖，確保影像與輪廓都跟隨同一座標系
    ax.invert_yaxis()

    # Simulated contour is math coords (y-up); convert to image coords (y-down) so it matches photo
    if contour_xy is not None and len(contour_xy):
        c = np.asarray(contour_xy, dtype=np.float64)
        if c.ndim == 2 and c.shape[1] >= 2:
            c_plot = c.copy()
            c_plot[:, 1] = 1.0 - c[:, 1]
        else:
            c_plot = None
    else:
        c_plot = None

    # Normalize photo contour to same center/scale as simulated for overlay; keep sc_* and pc_* for aligning original image
    sc_xmin = sc_xmax = sc_ymin = sc_ymax = None
    pc_xmin = pc_xmax = pc_ymin = pc_ymax = None
    if c_plot is not None and photo_contour_xy is not None and len(photo_contour_xy):
        p = np.asarray(photo_contour_xy, dtype=np.float64)
        if p.ndim == 2 and p.shape[1] >= 2:
            sc_xmin, sc_xmax = c_plot[:, 0].min(), c_plot[:, 0].max()
            sc_ymin, sc_ymax = c_plot[:, 1].min(), c_plot[:, 1].max()
            sc_cx = (sc_xmin + sc_xmax) / 2
            sc_cy = (sc_ymin + sc_ymax) / 2
            sc_w = (sc_xmax - sc_xmin) or 1.0
            sc_h = (sc_ymax - sc_ymin) or 1.0
            pc_xmin, pc_xmax = p[:, 0].min(), p[:, 0].max()
            pc_ymin, pc_ymax = p[:, 1].min(), p[:, 1].max()
            pc_cx = (pc_xmin + pc_xmax) / 2
            pc_cy = (pc_ymin + pc_ymax) / 2
            pc_w = (pc_xmax - pc_xmin) or 1.0
            pc_h = (pc_ymax - pc_ymin) or 1.0
            p_plot = np.zeros_like(p)
            p_plot[:, 0] = sc_cx + (p[:, 0] - pc_cx) / pc_w * sc_w
            p_plot[:, 1] = sc_cy + (p[:, 1] - pc_cy) / pc_h * sc_h
        else:
            p_plot = None
    else:
        p_plot = (np.asarray(photo_contour_xy, dtype=np.float64) if photo_contour_xy is not None and len(photo_contour_xy) else None)
        if p_plot is not None and (p_plot.ndim != 2 or p_plot.shape[1] < 2):
            p_plot = None
    if c_plot is not None and sc_xmin is None:
        sc_xmin, sc_xmax = c_plot[:, 0].min(), c_plot[:, 0].max()
        sc_ymin, sc_ymax = c_plot[:, 1].min(), c_plot[:, 1].max()

    # #region agent log
    try:
        import json
        _logpath = Path(__file__).resolve().parent / "debug-ce42d5.log"
        _log = open(_logpath, "a", encoding="utf-8")
        _log.write(json.dumps({"sessionId": "ce42d5", "hypothesisId": "H1,H2", "location": "contour_preview.show_contour_preview", "message": "alignment bounds", "data": {"sc_xmin": sc_xmin, "sc_xmax": sc_xmax, "sc_ymin": sc_ymin, "sc_ymax": sc_ymax, "pc_xmin": pc_xmin, "pc_xmax": pc_xmax, "pc_ymin": pc_ymin, "pc_ymax": pc_ymax}, "timestamp": __import__("time").time() * 1000}, ensure_ascii=False) + "\n")
        _log.close()
    except Exception:
        pass
    # #endregion

    # Overlay original image with same center/scale as drawn face (so contours sit on top of the photo)
    image_path = Path(image_path) if image_path else None
    if image_path and image_path.exists() and sc_xmin is not None and sc_xmax is not None and sc_ymin is not None and sc_ymax is not None:
        try:
            used_pil = False
            try:
                from PIL import Image
                img = np.array(Image.open(str(image_path)).convert("RGB"))
                used_pil = True
            except Exception:
                import matplotlib.image as mimg
                img = mimg.imread(str(image_path))
            if img.ndim >= 2:
                H, W = img.shape[0], img.shape[1]
                if pc_xmin is not None and pc_xmax is not None and pc_ymin is not None and pc_ymax is not None:
                    # 裁切範圍須與 landmark 對齊用 bbox 一致，否則疊圖會與紫/綠/青錯位（log 證實：pad 會讓臉在 crop 中央，extent 卻對齊全 crop → 五官跑掉）
                    pad = 0.0
                    x0 = max(0, int((pc_xmin - pad) * W))
                    x1 = min(W, int((pc_xmax + pad) * W) + 1)
                    y0 = max(0, int((pc_ymin - pad) * H))
                    y1 = min(H, int((pc_ymax + pad) * H) + 1)
                    if x1 > x0 and y1 > y0:
                        crop = np.asarray(img[y0:y1, x0:x1])
                        # 已先 invert_yaxis：小 y = 畫面上方。PIL row 0=臉頂 => 用 origin='upper' + extent top=sc_ymin，讓 row 0 在 y=sc_ymin => 臉頂在畫面上方。
                        if not used_pil:
                            crop = np.flipud(crop)
                        # #region agent log
                        try:
                            import json
                            _logpath = Path(__file__).resolve().parent / "debug-ce42d5.log"
                            _f = open(_logpath, "a", encoding="utf-8")
                            _f.write(json.dumps({"sessionId": "ce42d5", "hypothesisId": "H2,H3,H4", "location": "contour_preview.imshow_crop", "message": "image extent and PIL", "data": {"used_pil": used_pil, "extent": [sc_xmin, sc_xmax, sc_ymax, sc_ymin], "extent_bottom_top": [sc_ymax, sc_ymin]}, "timestamp": __import__("time").time() * 1000}, ensure_ascii=False) + "\n")
                            _f.close()
                        except Exception:
                            pass
                        # #endregion
                        ax.imshow(crop, extent=(sc_xmin, sc_xmax, sc_ymax, sc_ymin), aspect="auto", origin="upper", zorder=0, alpha=0.85)
                else:
                    # No photo contour: show full image in [0,1]x[1,0] (image y-down)
                    ax.imshow(img, extent=(0, 1, 1, 0), aspect="auto", zorder=0, alpha=0.6)
        except Exception:
            pass

    if c_plot is not None:
        # 藍線 = 由 shapeValueFace[0..18] 算出的 2D 輪廓（目前為 forward stub：橢圓+下巴；之後會接 list/customshape 真實模型）
        ax.plot(c_plot[:, 0], c_plot[:, 1], "b-", linewidth=2, label="Simulated (from params 0..18)", zorder=2)
        ax.fill(c_plot[:, 0], c_plot[:, 1], alpha=0.15, color="blue", zorder=1)
        for i in range(len(c_plot)):
            ax.annotate(str(i), (c_plot[i, 0], c_plot[i, 1]), fontsize=6, color="darkblue", xytext=(2, 2), textcoords="offset points", zorder=4, alpha=0.9)

    if p_plot is not None and getattr(p_plot, "ndim", 0) == 2 and len(p_plot) > 0:
        # Close contour for display (first point = last for FACE_OVAL)
        if len(p_plot) > 2:
            p_closed = np.vstack([p_plot, p_plot[0:1]])
            ax.plot(p_closed[:, 0], p_closed[:, 1], "r--", linewidth=1.5, label="Photo", zorder=2)
        else:
            ax.plot(p_plot[:, 0], p_plot[:, 1], "r--", linewidth=1.5, label="Photo", zorder=2)
        ax.scatter(p_plot[:, 0], p_plot[:, 1], c="red", s=8, alpha=0.7, zorder=2)
        # Only show 8-point labels when using legacy 8-point contour; skip for FACE_OVAL (36 pts)
        if len(p_plot) <= 8:
            photo_point_labels = ["L_face", "R_face", "Forehead", "Chin", "L_cheek", "R_cheek", "L_mouth", "R_mouth"]
            for i in range(min(len(p_plot), len(photo_point_labels))):
                ax.annotate(photo_point_labels[i], (p_plot[i, 0], p_plot[i, 1]), fontsize=7, color="darkred", xytext=(4, 4), textcoords="offset points", zorder=3)

    # Green lines: other measurement segments (eye span, mouth width, nose, etc.) in same scale as photo
    if photo_measurement_segments and len(photo_measurement_segments) > 0 and sc_xmin is not None and pc_xmin is not None:
        sc_cx = (sc_xmin + sc_xmax) / 2
        sc_cy = (sc_ymin + sc_ymax) / 2
        sc_w = (sc_xmax - sc_xmin) or 1.0
        sc_h = (sc_ymax - sc_ymin) or 1.0
        pc_cx = (pc_xmin + pc_xmax) / 2
        pc_cy = (pc_ymin + pc_ymax) / 2
        pc_w = (pc_xmax - pc_xmin) or 1.0
        pc_h = (pc_ymax - pc_ymin) or 1.0
        for label, pt1, pt2 in photo_measurement_segments:
            x1, y1 = pt1[0], pt1[1]
            x2, y2 = pt2[0], pt2[1]
            gx1 = sc_cx + (x1 - pc_cx) / pc_w * sc_w
            gy1 = sc_cy + (y1 - pc_cy) / pc_h * sc_h
            gx2 = sc_cx + (x2 - pc_cx) / pc_w * sc_w
            gy2 = sc_cy + (y2 - pc_cy) / pc_h * sc_h
            ax.plot([gx1, gx2], [gy1, gy2], "g-", linewidth=1.2, zorder=2)
            mid_x = (gx1 + gx2) / 2
            mid_y = (gy1 + gy2) / 2
            ax.annotate(
                label,
                (mid_x, mid_y),
                fontsize=9,
                color="darkgreen",
                xytext=(4, 4),
                textcoords="offset points",
                zorder=5,
                bbox=dict(boxstyle="round,pad=0.25", facecolor="white", alpha=0.9, edgecolor="darkgreen", linewidth=0.8),
            )
        # 綠線名稱一覽（所有綠點/量測的名字）
        green_names = ", ".join(lbl for lbl, _, _ in photo_measurement_segments)
        ax.text(0.02, 0.98, "Measurements: " + green_names, transform=ax.transAxes, fontsize=8, color="darkgreen",
                verticalalignment="top", bbox=dict(boxstyle="round,pad=0.35", facecolor="white", alpha=0.85, edgecolor="darkgreen"),
                zorder=5)
        ax.plot([], [], "g-", linewidth=1.2, label="Measurements", zorder=2)

    # 紫色：五官邊緣（眼、眉、唇）連線
    if photo_facial_feature_edges and len(photo_facial_feature_edges) > 0 and sc_xmin is not None and pc_xmin is not None:
        sc_cx = (sc_xmin + sc_xmax) / 2
        sc_cy = (sc_ymin + sc_ymax) / 2
        sc_w = (sc_xmax - sc_xmin) or 1.0
        sc_h = (sc_ymax - sc_ymin) or 1.0
        pc_cx = (pc_xmin + pc_xmax) / 2
        pc_cy = (pc_ymin + pc_ymax) / 2
        pc_w = (pc_xmax - pc_xmin) or 1.0
        pc_h = (pc_ymax - pc_ymin) or 1.0
        # #region agent log
        try:
            import json
            _seg = photo_facial_feature_edges[0]
            (x1, y1), (x2, y2) = _seg
            px1 = sc_cx + (x1 - pc_cx) / pc_w * sc_w
            py1 = sc_cy + (y1 - pc_cy) / pc_h * sc_h
            px2 = sc_cx + (x2 - pc_cx) / pc_w * sc_w
            py2 = sc_cy + (y2 - pc_cy) / pc_h * sc_h
            _logpath = Path(__file__).resolve().parent / "debug-ce42d5.log"
            _f = open(_logpath, "a", encoding="utf-8")
            _f.write(json.dumps({"sessionId": "ce42d5", "hypothesisId": "H2,H4,H5", "location": "contour_preview.purple_edges", "message": "first purple segment raw and plot", "data": {"raw_pt1": [x1, y1], "raw_pt2": [x2, y2], "plot_pt1": [px1, py1], "plot_pt2": [px2, py2], "pc_cx": pc_cx, "pc_cy": pc_cy, "pc_w": pc_w, "pc_h": pc_h}, "timestamp": __import__("time").time() * 1000}, ensure_ascii=False) + "\n")
            _f.close()
        except Exception:
            pass
        # #endregion
        for (x1, y1), (x2, y2) in photo_facial_feature_edges:
            px1 = sc_cx + (x1 - pc_cx) / pc_w * sc_w
            py1 = sc_cy + (y1 - pc_cy) / pc_h * sc_h
            px2 = sc_cx + (x2 - pc_cx) / pc_w * sc_w
            py2 = sc_cy + (y2 - pc_cy) / pc_h * sc_h
            ax.plot([px1, px2], [py1, py2], color="purple", linewidth=1.8, zorder=2.5)
        ax.plot([], [], color="purple", linewidth=1.8, label="Face features (edges)", zorder=2)

    # Yellow: edge-related landmarks (50 pts) the algorithm sees
    if photo_edge_landmarks_xy is not None and len(photo_edge_landmarks_xy) > 0 and sc_xmin is not None and pc_xmin is not None:
        sc_cx = (sc_xmin + sc_xmax) / 2
        sc_cy = (sc_ymin + sc_ymax) / 2
        sc_w = (sc_xmax - sc_xmin) or 1.0
        sc_h = (sc_ymax - sc_ymin) or 1.0
        pc_cx = (pc_xmin + pc_xmax) / 2
        pc_cy = (pc_ymin + pc_ymax) / 2
        pc_w = (pc_xmax - pc_xmin) or 1.0
        pc_h = (pc_ymax - pc_ymin) or 1.0
        pts = np.asarray(photo_edge_landmarks_xy, dtype=np.float64)
        if pts.ndim == 2 and pts.shape[1] >= 2:
            yx = sc_cx + (pts[:, 0] - pc_cx) / pc_w * sc_w
            yy = sc_cy + (pts[:, 1] - pc_cy) / pc_h * sc_h
            ax.scatter(yx, yy, c="yellow", s=12, alpha=0.9, zorder=3, label="Landmarks (50)", edgecolors="darkgoldenrod", linewidths=0.5)

    # Cyan: all 468 MediaPipe face landmarks
    if photo_all_landmarks_468_xy is not None and len(photo_all_landmarks_468_xy) > 0 and sc_xmin is not None and pc_xmin is not None:
        sc_cx = (sc_xmin + sc_xmax) / 2
        sc_cy = (sc_ymin + sc_ymax) / 2
        sc_w = (sc_xmax - sc_xmin) or 1.0
        sc_h = (sc_ymax - sc_ymin) or 1.0
        pc_cx = (pc_xmin + pc_xmax) / 2
        pc_cy = (pc_ymin + pc_ymax) / 2
        pc_w = (pc_xmax - pc_xmin) or 1.0
        pc_h = (pc_ymax - pc_ymin) or 1.0
        pts = np.asarray(photo_all_landmarks_468_xy, dtype=np.float64)
        if pts.ndim == 2 and pts.shape[1] >= 2:
            cx = sc_cx + (pts[:, 0] - pc_cx) / pc_w * sc_w
            cy = sc_cy + (pts[:, 1] - pc_cy) / pc_h * sc_h
            ax.scatter(cx, cy, c="cyan", s=12, alpha=0.85, zorder=3, label="Landmarks (468)", edgecolors="none")
            # 每個 landmark 標出編號，方便指出哪些地標是對的
            for i in range(len(cx)):
                ax.annotate(str(i), (cx[i], cy[i]), fontsize=6, color="darkblue", xytext=(2, 2), textcoords="offset points", zorder=4, alpha=0.9)

    ax.legend(loc="upper right")
    plt.tight_layout()
    # 每次跑完存檔，檔名加時間戳，方便日後分析
    out_dir = Path(__file__).resolve().parent / "preview_snapshots"
    out_dir.mkdir(parents=True, exist_ok=True)
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    save_path = out_dir / f"contour_{timestamp}.png"
    fig.savefig(save_path, dpi=150, bbox_inches="tight")
    if show:
        plt.show()
    else:
        plt.close(fig)


def _fallback_tkinter(contour_xy, photo_contour_xy, title):
    try:
        import tkinter as tk
    except ImportError:
        print("No matplotlib or tkinter; cannot show preview.")
        return

    w, h = 400, 500
    root = tk.Tk()
    root.title(title)
    canvas = tk.Canvas(root, width=w, height=h, bg="white")
    canvas.pack()

    def scale_pt(x, y):
        if contour_xy is None or not len(contour_xy):
            return None
        arr = np.asarray(contour_xy)
        mx, Mx = arr[:, 0].min(), arr[:, 0].max()
        my, My = arr[:, 1].min(), arr[:, 1].max()
        sx = (Mx - mx) or 1
        sy = (My - my) or 1
        px = (x - mx) / sx * (w - 40) + 20
        py = (y - my) / sy * (h - 40) + 20
        return (px, py)

    if contour_xy is not None and len(contour_xy) >= 2:
        c = np.asarray(contour_xy)
        pts = [scale_pt(c[i, 0], c[i, 1]) for i in range(len(c)) if scale_pt(c[i, 0], c[i, 1])]
        if len(pts) >= 2:
            flat = []
            for (a, b) in pts:
                flat.extend([a, b])
            canvas.create_line(flat, fill="blue", width=2)

    if photo_contour_xy is not None and len(photo_contour_xy):
        p = np.asarray(photo_contour_xy)
        for i in range(len(p)):
            pt = scale_pt(p[i, 0], p[i, 1])
            if pt:
                canvas.create_oval(pt[0] - 2, pt[1] - 2, pt[0] + 2, pt[1] + 2, fill="red", outline="red")

    root.mainloop()


def get_photo_contour_and_measurements(image_path):
    """
    Run MediaPipe once; return (contour_xy, segments, edge_landmarks_xy, all_landmarks_468_xy, precise_landmarks_xy).
    contour_xy: (K, 2) FACE_OVAL smoothed in [0,1]; segments: green measurement lines; edge: 50 (yellow);
    all_landmarks_468_xy: (468, 2) cyan; precise_landmarks_xy: (N, 2) FACE_SHAPE_INDICES (36 face contour pts) for purple.
    """
    try:
        from extract_face_ratios import (
            _get_model_path,
            get_xy,
            FACE_OVAL_INDICES,
            EDGE_RELATED_50_INDICES,
            FACE_FEATURE_EDGES,
            LEFT_EYE_INNER,
            RIGHT_EYE_INNER,
            LEFT_EYE_OUTER,
            RIGHT_EYE_OUTER,
            NOSE_TIP,
            NOSE_BRIDGE,
            MOUTH_LEFT,
            MOUTH_RIGHT,
            NOSE_LEFT,
            NOSE_RIGHT,
            UPPER_LIP_LEFT,
            UPPER_LIP_RIGHT,
            LOWER_LIP,
        )
        import mediapipe as mp
        from mediapipe.tasks.python import vision
    except ImportError:
        return None, [], None, None, None, []

    path = Path(image_path)
    if not path.exists():
        return None, [], None, None, None, []

    model_path = _get_model_path()
    base_options = mp.tasks.BaseOptions(model_asset_path=model_path)
    options = vision.FaceLandmarkerOptions(
        base_options=base_options,
        running_mode=vision.RunningMode.IMAGE,
        num_faces=1,
    )
    with vision.FaceLandmarker.create_from_options(options) as landmarker:
        mp_image = mp.Image.create_from_file(str(path))
        result = landmarker.detect(mp_image)
        if not result.face_landmarks:
            return None, [], None, None, None, []  # no face
        lm = result.face_landmarks[0]

        # All 468 landmarks (for full mesh preview)
        all_pts = []
        for i in range(len(lm)):
            x, y = get_xy(lm[i])
            all_pts.append([x, y])
        all_landmarks_468_xy = np.array(all_pts, dtype=np.float64) if all_pts else None

        # 輸出所有 landmark 索引與座標，供比對哪些點精準（[0,1] 圖座標，y 向下）
        out_dir = Path(__file__).resolve().parent
        out_file = out_dir / "landmarks_coords.txt"
        with open(out_file, "w", encoding="utf-8") as f:
            f.write("index,x,y  # MediaPipe 468, image coords [0,1] y-down\n")
            for i in range(len(lm)):
                x, y = get_xy(lm[i])
                f.write(f"{i},{x:.6f},{y:.6f}\n")
        precise_landmarks_xy = None  # 紫色已移除，不再計算

        # Edge-related 50 points (raw, for yellow dots); contour already included in these
        edge_pts = []
        for i in EDGE_RELATED_50_INDICES:
            if i < len(lm):
                x, y = get_xy(lm[i])
                edge_pts.append([x, y])
        edge_landmarks_xy = np.array(edge_pts, dtype=np.float64) if edge_pts else None

        # Contour: FACE_OVAL 36 points, then smooth via closed spline and resample (72–128 pts)
        pts = []
        for i in FACE_OVAL_INDICES:
            if i < len(lm):
                x, y = get_xy(lm[i])
                pts.append([x, y])
        contour_xy = np.array(pts, dtype=np.float64) if len(pts) >= 4 else None
        if contour_xy is not None:
            raw_bbox = {}
            # #region agent log
            try:
                raw_bbox = {"xmin": float(contour_xy[:, 0].min()), "xmax": float(contour_xy[:, 0].max()), "ymin": float(contour_xy[:, 1].min()), "ymax": float(contour_xy[:, 1].max())}
            except Exception:
                pass
            # #endregion
            contour_xy = _smooth_contour_closed(contour_xy, num_points=96)
            # #region agent log
            try:
                import json
                s = np.asarray(contour_xy, dtype=np.float64)
                smooth_bbox = {"xmin": float(s[:, 0].min()), "xmax": float(s[:, 0].max()), "ymin": float(s[:, 1].min()), "ymax": float(s[:, 1].max())}
                _logpath = Path(__file__).resolve().parent / "debug-ce42d5.log"
                _f = open(_logpath, "a", encoding="utf-8")
                _f.write(json.dumps({"sessionId": "ce42d5", "hypothesisId": "H1", "location": "contour_preview.get_photo_contour", "message": "raw36 vs smoothed96 bbox", "data": {"raw_36_bbox": raw_bbox, "smoothed_96_bbox": smooth_bbox}, "timestamp": __import__("time").time() * 1000}, ensure_ascii=False) + "\n")
                _f.close()
            except Exception:
                pass
            # #endregion

        # Measurement segments (same landmarks as extract_ratios)
        def pt(idx):
            if idx < len(lm):
                return get_xy(lm[idx])
            return (0.5, 0.5)

        segments = []
        try:
            segments.append(("eye_span", pt(LEFT_EYE_INNER), pt(RIGHT_EYE_INNER)))
            segments.append(("L_eye", pt(LEFT_EYE_INNER), pt(LEFT_EYE_OUTER)))
            segments.append(("R_eye", pt(RIGHT_EYE_INNER), pt(RIGHT_EYE_OUTER)))
            segments.append(("mouth_w", pt(MOUTH_LEFT), pt(MOUTH_RIGHT)))
            segments.append(("nose_w", pt(NOSE_LEFT), pt(NOSE_RIGHT)))
            segments.append(("nose_h", pt(NOSE_BRIDGE), pt(NOSE_TIP)))
            ux = (pt(UPPER_LIP_LEFT)[0] + pt(UPPER_LIP_RIGHT)[0]) / 2
            uy = (pt(UPPER_LIP_LEFT)[1] + pt(UPPER_LIP_RIGHT)[1]) / 2
            segments.append(("lip_h", (ux, uy), pt(LOWER_LIP)))
        except Exception:
            segments = []

        # 五官邊緣（眼、眉、唇）線段，用紫色畫
        facial_feature_edges = []
        try:
            for (i, j) in FACE_FEATURE_EDGES:
                if i < len(lm) and j < len(lm):
                    facial_feature_edges.append((pt(i), pt(j)))
        except Exception:
            facial_feature_edges = []

    return contour_xy, segments, edge_landmarks_xy, all_landmarks_468_xy, precise_landmarks_xy, facial_feature_edges


def get_photo_contour_xy(image_path):
    """
    Extract frontal face contour points (2D) from photo using MediaPipe FACE_OVAL.
    Returns (K, 2) array for preview overlay, or None if unavailable.
    Coordinates in [0,1], image y-down; closed contour order (no duplicate end point).
    """
    contour_xy, _, _, _, _, _ = get_photo_contour_and_measurements(image_path)
    if contour_xy is None:
        return None
    # #region agent log
    try:
        import json
        _logpath = Path(__file__).resolve().parent / "debug-ce42d5.log"
        _log = open(_logpath, "a", encoding="utf-8")
        _log.write(json.dumps({"sessionId": "ce42d5", "hypothesisId": "H1", "location": "contour_preview.get_photo_contour_xy", "message": "photo contour bounds", "data": {"xmin": float(contour_xy[:, 0].min()), "xmax": float(contour_xy[:, 0].max()), "ymin": float(contour_xy[:, 1].min()), "ymax": float(contour_xy[:, 1].max()), "n_pts": len(contour_xy)}, "timestamp": __import__("time").time() * 1000}, ensure_ascii=False) + "\n")
        _log.close()
    except Exception:
        pass
    # #endregion
    return contour_xy
