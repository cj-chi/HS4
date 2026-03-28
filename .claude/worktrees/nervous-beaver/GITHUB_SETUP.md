# 連到 GitHub

本專案已初始化 Git 並完成首次提交。要推到你的 GitHub：

## 1. 在 GitHub 建立新 repo

1. 登入 https://github.com
2. 右上角 **New repository**
3. 填 **Repository name**（例如 `HS4` 或 `hs2-face-ratio-mapping`）
4. 選 **Public** 或 **Private**
5. **不要**勾選 "Add a README"（本地已有）
6. 建立後會看到「…or push an existing repository from the command line」

## 2. 在本機加上 remote 並推送

在專案目錄 `d:\HS4` 執行（把 `你的帳號` 和 `repo名稱` 換成你的）：

```bash
git remote add origin https://github.com/你的帳號/repo名稱.git
git branch -M main
git push -u origin main
```

若已設定 SSH：

```bash
git remote add origin git@github.com:你的帳號/repo名稱.git
git push -u origin main
```

## 3. 之後若要改 Git 顯示名稱／信箱

```bash
git config --global user.name "你的名字"
git config --global user.email "你的GitHub信箱或 noreply 信箱"
```

目前此 repo 僅設了本機的 `user.name` / `user.email`（jason），要改成你的可執行：

```bash
cd d:\HS4
git config user.name "你的GitHub使用者名"
git config user.email "你的email"
```
