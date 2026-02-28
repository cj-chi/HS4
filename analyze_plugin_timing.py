# -*- coding: utf-8 -*-
"""
解析 BepInEx 插件 debug-526b9a.log，依每筆請求計算階段耗時，用來驗證「逐漸變慢」的假設。

假設（Hypotheses）：
  H-A: LoadFileLimited（H0→H2）隨請求次數變慢 → 若 ms 隨 index 上升則成立
  H-B: ReloadAsync 本身變慢（H1→ReloadAsync_done）→ 需 log 有 "ReloadAsync done"
  H-C: ReloadAsync 後的固定等待+截圖前準備（ReloadAsync_done→H4）變慢 → 若此段 ms 隨 index 上升則成立
  H-D: 整體 H1→H4（ReloadAsync+等待）變慢 → 已由使用者手算確認，本腳本用數據再驗證

不改寫既有腳本，僅讀取 NDJSON log、輸出統計。
"""
import json
import sys
from pathlib import Path

def _out(s):
    try:
        print(s)
    except UnicodeEncodeError:
        sys.stdout.buffer.write((s + "\n").encode("utf-8", errors="replace"))
        sys.stdout.buffer.flush()


def main():
    log_path = Path(__file__).resolve().parent / "debug-526b9a.log"
    if not log_path.exists():
        _out("Log not found: %s" % log_path)
        return 1

    # 只取插件 location 的紀錄
    loc_prefix = "HS2PhotoToCardPlugin.LoadCardAndScreenshot"
    entries = []
    with open(log_path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            try:
                o = json.loads(line)
                if o.get("location", "").startswith(loc_prefix):
                    entries.append(o)
            except Exception:
                continue

    # 依請求分組：每組以 H0 "LoadCardAndScreenshot started" 開始，到下一筆 H0 前為止
    requests = []
    i = 0
    while i < len(entries):
        e = entries[i]
        if e.get("message") == "LoadCardAndScreenshot started" and e.get("hypothesisId") == "H0":
            group = []
            if i > 0 and entries[i - 1].get("hypothesisId") == "Hlst":
                group.append(entries[i - 1])
            while i < len(entries):
                group.append(entries[i])
                i += 1
                if i < len(entries) and entries[i].get("message") == "LoadCardAndScreenshot started" and entries[i].get("hypothesisId") == "H0":
                    break
            requests.append(group)
            continue
        i += 1

    if not requests:
        _out("No request groups found (no H0 LoadCardAndScreenshot started).")
        return 1

        # 每組取 H0, H2, H1(Yielding), ReloadAsync done, AfterFixedWait(若有), H4
    rows = []
    for idx, group in enumerate(requests):
        ts_h0 = ts_h2 = ts_h1_yield = ts_reload_done = ts_after_fixed = ts_h4 = None
        lst_count = None
        for e in group:
            ts = e.get("timestamp")
            msg = e.get("message", "")
            hid = e.get("hypothesisId", "")
            if hid == "Hlst" and "lstLoadAssetBundleInfo" in msg:
                lst_count = (e.get("data") or {}).get("lstCount")
            if msg == "LoadCardAndScreenshot started" and hid == "H0":
                ts_h0 = ts
            elif msg == "LoadFileLimited done" and hid == "H2":
                ts_h2 = ts
            elif msg == "Yielding ReloadAsync" and hid == "H1":
                ts_h1_yield = ts
            elif "ReloadAsync done" in msg and hid == "H1":
                ts_reload_done = ts
            elif msg == "AfterFixedWait" and hid == "H1":
                ts_after_fixed = ts
            elif msg == "Before CaptureScreenshot" and hid == "H4":
                ts_h4 = ts
        if ts_h0 is None:
            continue
        load_ms = (ts_h2 - ts_h0) if ts_h2 is not None else None
        h1_to_h4_ms = (ts_h4 - ts_h1_yield) if (ts_h1_yield is not None and ts_h4 is not None) else None
        reload_ms = (ts_reload_done - ts_h1_yield) if (ts_h1_yield is not None and ts_reload_done is not None) else None
        fixed_wait_ms = (ts_after_fixed - ts_reload_done) if (ts_reload_done is not None and ts_after_fixed is not None) else None
        capture_prep_ms = (ts_h4 - ts_after_fixed) if (ts_after_fixed is not None and ts_h4 is not None) else None
        after_reload_ms = (ts_h4 - ts_reload_done) if (ts_reload_done is not None and ts_h4 is not None) else None
        rows.append({
            "index": len(rows) + 1,
            "lst_count": lst_count,
            "load_ms": load_ms,
            "h1_to_h4_ms": h1_to_h4_ms,
            "reload_ms": reload_ms,
            "fixed_wait_ms": fixed_wait_ms,
            "capture_prep_ms": capture_prep_ms,
            "after_reload_ms": after_reload_ms,
        })

    n = len(rows)
    _out("=== Plugin timing analysis (debug-526b9a.log) ===")
    _out("Total requests: %d" % n)
    _out("")

    # Hlst: 每筆請求開始時 lstLoadAssetBundleInfo.Count（有 Begin/End 時應每輪為 0）
    lst_counts = [r["lst_count"] for r in rows if r.get("lst_count") is not None]
    if lst_counts:
        first5 = lst_counts[:5]
        last5 = lst_counts[-5:] if len(lst_counts) >= 5 else lst_counts
        _out("Hlst lstLoadAssetBundleInfo.Count (before Begin): first5 %s ... last5 %s" % (first5, last5))
        _out("  → 有 Begin/End 時應皆為 0；若隨 index 遞增則為未清空累積。")
    _out("")

    # H-A: LoadFileLimited 是否隨 index 變大
    load_vals = [r["load_ms"] for r in rows if r["load_ms"] is not None]
    if load_vals:
        first5 = load_vals[:5]
        last5 = load_vals[-5:] if len(load_vals) >= 5 else load_vals
        _out("H-A LoadFileLimited (H0→H2) ms: first5 %s ... last5 %s" % (first5, last5))
        _out("  min=%s max=%s → 若 min/max 都 <50ms 且無隨 index 上升趨勢則 H-A 不成立（載卡非瓶頸）" % (min(load_vals), max(load_vals)))
    _out("")

    # H-D: H1→H4 是否隨 index 變大
    h1h4_vals = [r["h1_to_h4_ms"] for r in rows if r["h1_to_h4_ms"] is not None]
    if h1h4_vals:
        first5 = h1h4_vals[:5]
        last5 = h1h4_vals[-5:] if len(h1h4_vals) >= 5 else h1h4_vals
        _out("H-D ReloadAsync+等待 (H1→H4) ms: first5 %s ... last5 %s" % (first5, last5))
        _out("  min=%s max=%s → 若 last5 明顯 > first5 則 H-D 成立（此階段逐漸變慢）" % (min(h1h4_vals), max(h1h4_vals)))
    _out("")

    # H-B / H-C: 若有 ReloadAsync done 與 AfterFixedWait
    if any(r["reload_ms"] is not None for r in rows):
        reload_vals = [r["reload_ms"] for r in rows if r["reload_ms"] is not None]
        after_vals = [r["after_reload_ms"] for r in rows if r["after_reload_ms"] is not None]
        _out("H-B ReloadAsync only (H1→ReloadAsync_done) ms: first5 %s ... last5 %s" % (reload_vals[:5], reload_vals[-5:] if len(reload_vals) >= 5 else reload_vals))
        _out("H-C After ReloadAsync (ReloadAsync_done→H4) ms: first5 %s ... last5 %s" % (after_vals[:5], after_vals[-5:] if len(after_vals) >= 5 else after_vals))
        if any(r["fixed_wait_ms"] is not None for r in rows):
            fw = [r["fixed_wait_ms"] for r in rows if r["fixed_wait_ms"] is not None]
            cp = [r["capture_prep_ms"] for r in rows if r["capture_prep_ms"] is not None]
            _out("  FixedWait (ReloadAsync_done→AfterFixedWait) ms: first5 %s ... last5 %s" % (fw[:5], fw[-5:] if len(fw) >= 5 else fw))
            _out("  CapturePrep (AfterFixedWait→H4) ms: first5 %s ... last5 %s" % (cp[:5], cp[-5:] if len(cp) >= 5 else cp))
    else:
        _out("H-B/H-C: log 中無 'ReloadAsync done'，無法細分。請重新編譯並部署插件後再跑一輪 5 分鐘測試，再解析新 log。")

    _out("")
    _out("--- 前 5 / 中 5 / 後 5 筆明細 ---")
    for label, indices in [("前5筆", list(range(0, min(5, n)))), ("中5筆", list(range(max(0, n//2 - 2), min(n, n//2 + 3)))), ("後5筆", list(range(max(0, n - 5), n)))]:
        _out(label + ":")
        for i in indices:
            if i >= len(rows):
                break
            r = rows[i]
            _out("  #%d lstCount=%s load=%s  H1→H4=%s  reload=%s  fixed_wait=%s  capture_prep=%s" % (r["index"], r.get("lst_count"), r["load_ms"], r["h1_to_h4_ms"], r["reload_ms"], r["fixed_wait_ms"], r["capture_prep_ms"]))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
