# WIP 筆記 — 2026-03-28（Git 還原點對照）

## 分支與用途

- 分支：`wip/all-20260318-2012`（與 `origin` 同步基準依當時 `git status`）
- 本筆記對應一次 **checkpoint commit + tag**，必要時可 `git checkout <tag>` 或 `git reset --hard <commit>` 還原至此狀態。

## 本還原點包含的變更摘要

| 路徑 | 說明 |
|------|------|
| `.../OrbitController.cs` | 環視焦點改為需 **Ctrl** 的 Q/W/E（含雙女 Ctrl+Shift）；新增 **`ApplyFinishHotkeys`**（P/O/I/U/Y/T，依 `modeCtrl` 過濾）設 `ctrlFlag.click` |
| `.../ExciterTriggerPatches.cs` | 感度滿後「立即觸發」改為 **Ctrl + 左鍵**（避免與場景點選衝突） |
| `CODE_REVIEW_Claude_20260328_HS2OrbitAndExciter_SkeletonToggle.md` | 審查／設計原則與待修項目之文字更新 |
| `.claude/.../settings.local.json` | Claude 本機權限 allow 列表（Bash/dotnet 等） |

## 與舊版行為的差異（方便日後對照）

- **已移除**環視用滾輪覆寫相機縮放（`ApplyOrbitScrollZoom` 類邏輯已不在目前 worktree 程式路徑中搜尋到）。
- **尚未處理**（審查文件仍標 P0/P1）：`OrbitBypassWheelPatches` 注入 `wheel=0.10f` 可能影響動畫速度；Exciter transpiler 僅替換第一處 `feel_f`/1f 匹配；`_orbitActiveForPatches` 非 `volatile` 等。

## 文件與程式可能不一致處

- `HS2OrbitAndExciter/CHANGELOG.md`（若未納入本次 commit）仍可能描述舊的 QWE／滾輪行為，合併主線時建議再對照 `OrbitController` 與 patches 更新 CHANGELOG。

## 還原方式

```text
git tag -l 'checkpoint/*'
git show checkpoint/hs2-orbit-wip-20260328
git checkout checkpoint/hs2-orbit-wip-20260328   # 或建立分支：git switch -c recover/... <tag>
```
