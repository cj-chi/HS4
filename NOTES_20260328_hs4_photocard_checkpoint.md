# HS4 還原點筆記 — 2026-03-28（PhotoToCard／MediaPipe／骨骼除錯）

## 分支與標籤

- **分支**：`wip/all-20260318-2012`（此還原點時，領先 `origin` 的 commit 數請以 `git status` 為準）
- **標籤（還原點）**：`checkpoint/hs4-photocard-20260328`  
  - 建立方式見下方「還原方式」；建立後可用 `git show checkpoint/hs4-photocard-20260328` 查看指向的 commit。

## 此還原點涵蓋的 Commit（由舊到新）

| Commit | 說明 |
|--------|------|
| `5eaf49a` | Checkpoint：PhotoToCard 外掛與 csproj、MediaPipe 工具、`Landmark Mapping.txt`、`docs/MediaPipe_FACEMESH_index_reference.md` |
| `b6ec030` | 臉部骨骼除錯：點縮半、全選/全消、懸浮才顯示名稱、結構模式綠線（`Camera.onPostRender` + GL） |
| `1f39e6d` | 「僅留錨點子樹＋複製名稱」、葉節點點選改為父節點並展開樹狀 `Foldout` |
| （本筆記檔） | 文件：`NOTES_20260328_hs4_photocard_checkpoint.md` |

## 主要路徑對照

| 路徑 | 說明 |
|------|------|
| `BepInEx_HS2_PhotoToCard/HS2PhotoToCardPlugin.cs` | 載卡截圖、臉部骨骼除錯 UI／2D 點／結構綠線／剪貼簿隔離子樹 |
| `BepInEx_HS2_PhotoToCard/HS2.PhotoToCard.Plugin.csproj` | 專案與建置後複製至 `D:\HS2\BepInEx\Plugins`（依 csproj 設定） |
| `tools/generate_landmark_overlay_viewer.py` | 產生可縮放平移的 landmark 疊圖 HTML |
| `tools/generate_facemesh_index_reference.py`、`tools/mediapipe_face_mesh_connections.py` | FACEMESH 索引參考 |
| `docs/MediaPipe_FACEMESH_index_reference.md` | 由工具產生之索引說明 |
| `Landmark Mapping.txt` | MediaPipe 與 `cf_J_*` 對照備忘（手動＋表） |
| `output/landmark_overlay_viewer.html` | 須用腳本重產；底圖邏輯見腳本內註解 |

## 骨骼除錯操作速查（遊戲內）

- **熱鍵**：預設 `ScrollLock`（CharaCustom）
- **全選/全消**、**結構模式**（點選設錨點畫綠線）、**僅留錨點子樹＋複製名稱**、葉節點自動升到父節點並展開樹

## 還原方式

```text
# 列出還原點標籤
git tag -l 'checkpoint/hs4-*'

# 查看標籤對應的 commit
git show checkpoint/hs4-photocard-20260328

# 僅檢視舊版檔案（不動工作區）
git show checkpoint/hs4-photocard-20260328:BepInEx_HS2_PhotoToCard/HS2PhotoToCardPlugin.cs

# 回到該點（破壞性：會丟掉之後未提交的變更）
git reset --hard checkpoint/hs4-photocard-20260328

# 或從標籤開分支保留現況
git switch -c recover/photocard-20260328 checkpoint/hs4-photocard-20260328
```

若標籤尚未建立，在含本筆記的 commit 上執行：

```text
git tag -a checkpoint/hs4-photocard-20260328 -m "HS4: PhotoToCard bone debug, MediaPipe tools, landmark mapping"
```

## 與其他筆記

- 環視／興奮劑等另一段 WIP 見同目錄 `NOTES_20260328_wip_checkpoint.md`（若存在）。
