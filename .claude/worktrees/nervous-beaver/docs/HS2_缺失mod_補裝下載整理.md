# HS2 報錯缺失的 mod 補裝／下載整理

日誌出現 **Missing zipmod! - tAOb_Cloud_Gorgeous_Dress** 等時，可依下列來源嘗試補裝。

---

## 1. 已知缺失 ID（從日誌）

| 缺失 ID（GUID） | 說明 |
|----------------|------|
| **tAOb_Cloud_Gorgeous_Dress** | tAOb 作者的服裝 mod，名稱為「Cloud Gorgeous Dress」 |

若你的日誌還有其他 `Missing zipmod! - <ID>`，可一併記下，用同樣方式搜尋。

---

## 2. 已找到的下載來源（BetterRepack Sideloader）

- **tAOb 作者目錄**（HS2 Sideloader Modpack）：  
  **https://sideload.betterrepack.com/download/AISHS2/Sideloader%20Modpack/tAOb/**

該目錄內**沒有**檔名為「Cloud_Gorgeous_Dress」的 zipmod，但有同系列的 **Cloud_Poor_Dress**：
- **[[tAOb] Cloud_Poor_Dress.zipmod](https://sideload.betterrepack.com/download/AISHS2/Sideloader%20Modpack/tAOb/%5btAOb%5d%20Cloud_Poor_Dress.zipmod)**（約 23MB）

可能情況：
- **Cloud_Gorgeous_Dress** 是另一款／另一版，未收錄在 BetterRepack，需從 tAOb 的 Patreon、或其它站點（見下）找。
- 若卡片用的是「Gorgeous」版，補裝時請以 **Gorgeous** 為準；**Poor_Dress** 僅為同作者、名稱相近，不一定能消除該缺失報錯。

---

## 3. 補裝步驟建議

1. **從 BetterRepack 裝 tAOb 系列（至少先補齊同作者）**  
   - 開啟：<https://sideload.betterrepack.com/download/AISHS2/Sideloader%20Modpack/tAOb/>  
   - 下載需要的 zipmod（例如先下 **Cloud_Poor_Dress** 試用，或整包 tAOb 目錄）。  
   - 將 `.zipmod` 放到 **D:\HS2\mods**（或你遊戲的 mods 目錄），**不要解壓**。  
   - 關閉遊戲後放入，再開遊戲；若曾清過 BepInEx cache，第一次會重掃較久。

2. **若日誌仍是 Missing zipmod! - tAOb_Cloud_Gorgeous_Dress**  
   - 表示 **Gorgeous** 款不在 BetterRepack 這份清單，需另尋來源：  
     - **tAOb Patreon**（若作者有釋出 Gorgeous 版）  
     - **世界終わり (sekaiowari.com)**：有 [tAOb] Perona_Swimsuit 等，可搜站內是否還有其他 tAOb 服裝  
     - **ハニーセレクト２勝手にアップローダー (yuki-portal.com)**：HS2 服裝／座標上傳站，可搜「tAOb」「Cloud」「Gorgeous」  
     - **Roy12 Mods**、**Lowborn 3D** 等 HS2 服裝站，用「tAOb」「Cloud Gorgeous Dress」搜尋  

3. **其它缺失 ID**  
   - 用日誌裡的完整 ID（例如 `DALI_033MJG_QQ3344929957`）到：  
     - <https://sideload.betterrepack.com/download/AISHS2/> 各 Modpack 目錄搜檔名或資料夾  
     - 或專案腳本：  
       `.\scripts\check_hs2_missing_mods.ps1 -Hs2Root "D:\HS2" -MissingIds "tAOb_Cloud_Gorgeous_Dress","其他ID"`  
       會在 mods 底下搜檔名是否包含該 ID。

---

## 4. 快速連結

| 用途 | 連結 |
|------|------|
| HS2 Sideloader Modpack 根目錄 | https://sideload.betterrepack.com/download/AISHS2/ |
| tAOb 作者目錄（含 Cloud_Poor_Dress） | https://sideload.betterrepack.com/download/AISHS2/Sideloader%20Modpack/tAOb/ |
| Sideloader Modpack - Exclusive HS2 | https://sideload.betterrepack.com/download/AISHS2/Sideloader%20Modpack%20-%20Exclusive%20HS2/ |

---

若你之後從日誌又看到其他 `Missing zipmod! - <ID>`，可把 ID 補進本文件「已知缺失 ID」表，再用同一套方式搜 BetterRepack 或上述網站補裝。
