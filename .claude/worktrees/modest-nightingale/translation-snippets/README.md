# 翻譯片段（房間左上方等）

## 左上方日文修正（グループ2が設定中 → 繁中）

1. **確認遊戲的 BepInEx 翻譯資料夾存在**  
   例如：`D:\hs2\BepInEx\Translation\zh-TW\Text\`  
   （路徑依你的遊戲安裝目錄與 BepInEx 設定而定；HS4 則為對應的 HS4 遊戲目錄。）

2. **把對照寫進翻譯檔**
   - **方式 A**：把 `zh-TW_Text_RoomUI.txt` 的**全部內容**複製到該 `Text` 資料夾裡已有的某個 .txt（例如 `Manual.txt` 或 `Translation.txt`）的**最後**，存檔為 UTF-8。
   - **方式 B**：直接把 `zh-TW_Text_RoomUI.txt` 複製到 `Text` 資料夾內（檔名可保留或改成例如 `RoomUI.txt`）。

3. **重啟遊戲**（或依 AutoTranslator / 你使用的翻譯插件說明重新載入翻譯），左上方應會顯示「群組2設定中」等繁中。

若你使用的是 **XUnity.AutoTranslator**，格式為每行：`日文原文=繁中譯文`，本檔已符合。
