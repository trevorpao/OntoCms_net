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
- [x] `docker compose --env-file .env -f conf/docker/docker-compose.yml config` 驗證通過
- [x] `docker compose --env-file .env -f conf/docker/docker-compose.yml build web` 驗證通過
- [x] `document/sql/*.sql` 經關鍵字掃描後已無 `AUTO_INCREMENT`、`ENGINE=InnoDB`、`enum(...)`、反引號等 MySQL 殘留語法

## Open Items
- [ ] 將已轉好的 `document/sql/*.sql` 納入啟動時的 DB bootstrap 流程
- [ ] 將首頁從 MVC template 預設內容改為讀取 `tbl_option` 的系統名稱
- [ ] 執行 `docker compose --env-file .env -f conf/docker/docker-compose.yml up` 並驗證瀏覽器或 HTTP 請求能取得 SSR 首頁
- [ ] 補 Stage 0 正式驗收清單，區分已完成與未完成項
