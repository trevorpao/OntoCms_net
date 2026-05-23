# OntoCms_net History

## 2026-05-23

### Stage
- `(done)`

### Summary
- `idea.md` 與 `plan.md` 已從原始 `NetRefactor` spec 搬入 `OntoCms_net`。
- 原始 spec 不含 `history.md` 與 `check.md`，因此本檔為新專案初始化的第一版 handoff。
- 已完成 Stage 0 bootstrap artifact 第一輪落地：新增 `.env`、`conf/docker/docker-compose.yml`、`OntoCms_net.sln`、`src/public/` MVC 專案骨架，以及 `conf/docker/Dockerfile`。
- 主機未安裝 `dotnet` CLI，因此 `.NET` skeleton 以 Docker SDK 容器建立；這次驗證路徑也以 Docker 為準。
- 已完成兩個最小驗證：`docker compose --env-file .env -f conf/docker/docker-compose.yml config` 可正確展開 compose 契約，`docker compose --env-file .env -f conf/docker/docker-compose.yml build web` 可成功建置目前的 ASP.NET Core skeleton。
- 已將 `document/sql/init.sql` 與每日增量 SQL 由 MySQL/MariaDB 語法改寫為 MSSQL 版本，包含 `NVARCHAR` / `DATETIME2` / `IDENTITY`、`IF OBJECT_ID ... CREATE TABLE`、索引拆出，以及需要保留固定主鍵值的 `IDENTITY_INSERT` seed 包裝。
- 已以獨立 compose project `ontocms-sqlcheck` 啟動 SQL Server 2022 容器，重建乾淨的 `OntoCms` 資料庫後，成功依序執行 `document/sql/init.sql`、`260324.sql`、`260412.sql`、`260425.sql`、`260501.sql`、`260520.sql`。
- 已在 `src/public/Program.cs` 接上啟動時 DB bootstrap，並在 Docker image final stage 帶入 `document/sql/*.sql`，使容器內可直接執行 SQL 初始化。
- 已以預設 compose 環境重建乾淨的 `db` volume，確認 `web` 啟動時會等待 MSSQL ready、依序執行 `init.sql` 與增量 SQL，且 `tbl_option`、`tbl_conversation`、`tbl_member`、`tbl_press_seen`、`tbl_doorman_blacklist`、`tbl_campaign_log` 等代表資料表都已存在。

### Drift
- 原始文件的 bootstrap 前提漂移已完成第一輪對齊，目前已不再是假設 Docker / `.NET` artifact 存在，而是已落地為實際 repo 結構。
- 仍有一段未完成的 Stage 0 drift：雖然 SQL 腳本已轉為 MSSQL、也已接上 `Program.cs` 啟動 bootstrap，但首頁仍未讀取 `tbl_option`，因此不能把 Stage 0 驗收點視為已完成。

### Next Step
- 進入 Stage 0 的下一個最小步驟：把首頁改為讀取一筆 `tbl_option` 系統名稱，完成第一個真正的 Docker-based Walking Skeleton，然後以 `docker compose --env-file .env -f conf/docker/docker-compose.yml up` 驗證 SSR 首頁可用。
