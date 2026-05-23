# OntoCms_net Check

## Current Stage
- `(done)`

## Check Basis
- 本檔目前承接 Stage 0 bootstrap artifact 的第一輪 implementation 檢查。
- 驗證環境以 Docker 為準；主機缺少 `dotnet` CLI，因此 `.NET` 建置與 compile 驗證皆改由 Docker SDK / Docker Compose 路徑完成。

## Current Findings
- [x] 已建立 `conf/docker/docker-compose.yml`，包含 `web` 與 `db` 服務定義
- [x] 已建立 `.env` 契約，提供 compose 與 DB 連線的 source of truth
- [x] 已建立 `OntoCms_net.sln` 與 `src/public/` ASP.NET Core MVC skeleton
- [x] 已建立 `conf/docker/Dockerfile`，使 `web` 服務可透過 compose 建置
- [x] 已將 `document/sql/init.sql` 與 `document/sql/*.sql` 每日增量檔改寫為 MSSQL 版本，移除 MySQL 專屬語法並補上 `IDENTITY_INSERT` seed 包裝
- [x] 已在 `src/public/Program.cs` 接上啟動時 DB bootstrap，並加入 MSSQL ready retry 與 `tbl_option` 存在檢查
- [x] 已讓 Docker image final stage 帶入 `document/sql/*.sql`，使容器內可讀到 bootstrap 腳本
- [x] `docker compose --env-file .env -f conf/docker/docker-compose.yml config` 驗證通過
- [x] `docker compose --env-file .env -f conf/docker/docker-compose.yml build web` 驗證通過
- [x] `document/sql/*.sql` 經關鍵字掃描後已無 `AUTO_INCREMENT`、`ENGINE=InnoDB`、`enum(...)`、反引號等 MySQL 殘留語法
- [x] 已以獨立 SQL Server 2022 容器重建乾淨資料庫並成功執行 `document/sql/init.sql` 與所有增量 SQL，確認目前 MSSQL 腳本可完整落庫
- [x] 已以預設 compose 環境重建乾淨 volume，確認 `web` 啟動時可自動建立 / 初始化 DB，且 `tbl_option` 與各增量 SQL 的代表資料表都已成功落庫

## Open Items
- [ ] 將首頁從 MVC template 預設內容改為讀取 `tbl_option` 的系統名稱
- [ ] 執行 `docker compose --env-file .env -f conf/docker/docker-compose.yml up` 並驗證瀏覽器或 HTTP 請求能取得 SSR 首頁
- [ ] 補 Stage 0 正式驗收清單，區分已完成與未完成項
