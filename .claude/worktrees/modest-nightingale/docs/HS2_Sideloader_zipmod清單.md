# HS2 Sideloader zipmod 清單（快照）

以下為 Sideloader 已載入 zipmod 一覽，欄位：**啟用**、**名稱**、**版本**、**作者**、**GUID**、**zipmod 檔名**、**來源**。  
格式對齊 [HS2_BepInEx_插件清單.md](HS2_BepInEx_插件清單.md)。

## 如何產生此表

1. 從遊戲內或 Sideloader Modpack Studio 匯出 mod 清單（TSV：Tab 分隔，7 欄）。
2. 在專案根目錄執行：
   ```bash
   python scripts/zipmod_list_to_md.py 你的匯出.tsv >> docs/HS2_Sideloader_zipmod清單.md
   ```
   或從 stdin 導向：
   ```bash
   python scripts/zipmod_list_to_md.py < 你的匯出.tsv >> docs/HS2_Sideloader_zipmod清單.md
   ```
3. 若不想把來源轉成可點連結，可加參數：`python scripts/zipmod_list_to_md.py --no-link 你的匯出.tsv`。

---

| 啟用 | 名稱 | 版本 | 作者 | GUID | zipmod 檔名 | 來源 |
|:---:|------|------|------|------|-------------|------|
| ✓ | bp.sac_innie_v2 | 6.1 | Animal42069 based on original by SAC | bp.sac_innie_v2 | [Female][HS2][BPV5]SAC_Innie.zipmod | [Patreon](https://www.patreon.com/Animal42069) |
| ✓ | SAC_Meaty | 6.1 | Animal42069 based on original by SAC | bp.sac_meaty_v2 | [Female][HS2][BPV5]SAC_Meaty.zipmod | [Patreon](https://www.patreon.com/Animal42069) |
| ✓ | TD78.OpenPussy | 0.1.1 | SAC+TD78 | TD78.OpenPussy | [Female][HS2][TD78]OpenPussy.zipmod | totaldecay78.blogspot.com |

*（以上為範例；完整清單請將 Sideloader 匯出的 TSV 以 `scripts/zipmod_list_to_md.py` 轉成表格後貼上或併入本檔。）*
