# OntoCms_net Check

## Current Stage
- `(done)`

## Check Basis
- 本檔目前承接 Stage 1.1 後的 owner-boundary 收斂與 Stage 1.2 前置校準。
- 驗證環境以 Docker 為準；主機缺少 `dotnet` CLI，因此 `.NET` 建置與 compile 驗證皆改由 Docker SDK / Docker Compose 路徑完成。

## Current Findings
- [x] 已建立 `conf/docker/docker-compose.yml`，包含 `web` 與 `db` 服務定義
- [x] 已建立 `.env` 契約，提供 compose 與 DB 連線的 source of truth
- [x] 已建立 `OntoCms_net.sln` 與 `src/public/` ASP.NET Core MVC skeleton
- [x] 已建立 `conf/docker/Dockerfile`，使 `web` 服務可透過 compose 建置
- [x] 已將 `document/sql/init.sql` 與 `document/sql/*.sql` 每日增量檔改寫為 MSSQL 版本，移除 MySQL 專屬語法並補上 `IDENTITY_INSERT` seed 包裝
- [x] 已提供顯式 DB bootstrap CLI，沿用 `DatabaseBootstrapper` 的 MSSQL ready retry 與 `tbl_option` 存在檢查，但不再於 web startup 自動執行
- [x] DB bootstrap 已切為獨立 CLI project：[src/cli/OntoCms.Cli.csproj](src/cli/OntoCms.Cli.csproj) 與 [src/cli/Bootstrap/DatabaseBootstrapper.cs](src/cli/Bootstrap/DatabaseBootstrapper.cs)；web project 不再編譯 `src/cli/**/*.cs`
- [x] 已讓 Docker image final stage 帶入 `document/sql/*.sql`，使容器內可讀到 bootstrap 腳本
- [x] 首頁 route 已由 `src/public` 收回 module owner，改由 `src/Modules/Option/outfit.cs` 承接並透過 theme view 渲染
- [x] 已新增最薄的 `api/option/get`，可回傳 `tbl_option.id = 1` 的 Gene Panel 風格 envelope 作為首頁 cross-check
- [x] `docker compose --env-file .env -f conf/docker/docker-compose.yml config` 驗證通過
- [x] `docker compose --env-file .env -f conf/docker/docker-compose.yml build web` 驗證通過
- [x] `document/sql/*.sql` 經關鍵字掃描後已無 `AUTO_INCREMENT`、`ENGINE=InnoDB`、`enum(...)`、反引號等 MySQL 殘留語法
- [x] 已以獨立 SQL Server 2022 容器重建乾淨資料庫並成功執行 `document/sql/init.sql` 與所有增量 SQL，確認目前 MSSQL 腳本可完整落庫
- [x] 已驗證 DB bootstrap 可由獨立 CLI 承接；web runtime 不再自動建立 / 初始化 DB
- [x] `http://localhost:8080` 已可 SSR 渲染首頁；目前 site title 仍讀取 `tbl_option.page/title = Demo`，但首頁主內容已改為讀取 `tbl_post.slug = about`
- [x] `http://localhost:8080/api/option/get?id=1` 已返回 `{"code":1,"data":{"id":1,"group":"page","name":"title","content":"Demo"},"csrf":""}`，確認首頁與 API cross-check 對齊同一筆資料
- [x] `http://localhost:8080/?lang=tw` 與 `http://localhost:8080/?lang=en` 已可透過 about Post 內容返回中英首頁
- [x] 首頁前台語言解析已收斂到 [src/conventions/HMVC/BaseOutfitController.cs](src/conventions/HMVC/BaseOutfitController.cs)，目前依 route → query → cookie → default fallback 決定前台語言，並支援 `user_lang` cookie 回寫
- [x] `http://localhost:8080/tw`、`http://localhost:8080/en`、`http://localhost:8080/tw/about`、`http://localhost:8080/en/about` 已成為首頁的語系前綴路由入口
- [x] 已以 curl 驗證首頁語言優先序：default `/`、route `/en/about`、cookie `user_lang=en + /about`、query `/about?lang=tw` 覆蓋 cookie、route `/tw/about?lang=en` 覆蓋 query
- [x] 語言決策規則已抽到 [src/conventions/HMVC/ForkLanguageResolver.cs](src/conventions/HMVC/ForkLanguageResolver.cs)；目前 `BaseOutfitController` 與 `BaseReactionController` 都只作為 HTTP adapter，不再承接 lang 規則本體
- [x] `https://loc.f3cms.com:4433/` 已可透過 `curl --resolve loc.f3cms.com:4433:127.0.0.1 -k` 成功返回首頁 HTML，且內容包含 `Demo`
- [x] `https://loc.f3cms.com:4433/api/option/get?id=1` 已可透過 `curl --resolve loc.f3cms.com:4433:127.0.0.1 -k` 成功返回 `Demo` 對應的 option payload
- [x] [bin/build.sh](bin/build.sh) 現在會先用 `mkcert` + `openssl` 在 [conf/iis](conf/iis) 生成開發用 `loc.f3cms.com.pfx`，再執行 Docker build
- [x] [conf/docker/docker-compose.yml](conf/docker/docker-compose.yml) 現在會把 [conf/iis](conf/iis) 掛載到 container 的 `/https`，供 Kestrel 讀取 `loc.f3cms.com.pfx`
- [x] [conf/dotnet/entrypoint.sh](conf/dotnet/entrypoint.sh) 已改為只檢查 `/https/loc.f3cms.com.pfx` 是否存在；若缺少憑證會明確要求先跑 [bin/build.sh](bin/build.sh)
- [x] root [.gitignore](.gitignore) 已排除 `conf/iis/*`，只保留 [conf/iis/.gitkeep](conf/iis/.gitkeep)，避免開發憑證進 git
- [x] 已在 `src/conventions` 補上 `IFeedRepository<TPayload>`、`MTBAttribute`、`MULTILANGAttribute` 的第一版 contract
- [x] 已讓 [src/public/OntoCms.Web.csproj](src/public/OntoCms.Web.csproj) 將 `src/conventions/**/*.cs` 納入編譯，避免 conventions 只停留在空目錄骨架
- [x] Stage 1.1 contract slice 經 `docker compose --env-file .env -f conf/docker/docker-compose.yml build web` 驗證可成功編譯
- [x] 已補上 `BaseFeedRepository<TPayload>` 最小骨架，能從 `MTB` / `MULTILANG` 解析 main/lang/meta table metadata 與 primary key helper
- [x] `BaseFeedRepository<TPayload>` 加入後，`docker compose --env-file .env -f conf/docker/docker-compose.yml build web` 仍可成功編譯
- [x] 已補上第一個 module-owned Feed 範例：`src/Modules/Option/feed.cs`、`reaction.cs`、`outfit.cs`、`kit.cs`、`smoke.cs`
- [x] 首頁 view 已移到 `src/theme/default/frontend/Home/Index.cshtml`，並由 Razor 自訂 view location 正常承接
- [x] Option 模組化與 theme view 搬移後，`docker compose --env-file .env -f conf/docker/docker-compose.yml build web` 仍可成功編譯
- [x] Option 模組化與首頁 route 搬移後，`http://localhost:8080/` 與 `http://localhost:8080/api/option/get?id=1` 仍可正常返回 `Demo`
- [x] FeedBase 目前位置已收斂在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs)
- [x] Stage 1 文件已明確對齊 Dapper-first 原則：FeedBase 採薄共通 CRUD / transaction，SQL 維持第一級公民，不引入 EF-style ownership 混淆
- [x] Stage 1 文件已明確決定 FeedBase 函式邊界：承接 metadata/table helper、save orchestration、`_handleColumn` 骨架、`SaveMetaAsync()` / `SaveLangAsync()`、thin read helper、status/delete helper
- [x] Stage 1 文件已明確排除 `Belong.php` 類 relation/bind/count/saveMany 行為，不讓它們污染 `BaseFeedRepository<T>`
- [x] 已在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上 Stage 1.2 的第一個 `_handleColumn` slice：payload 可先分流為主表欄位、`meta`、`lang`、`tags`，尚未接入 DB write 或 audit fields
- [x] `src/Modules/Option/feed.cs` 已改為以具體 `WriteModel` 觸發 `HandleColumns()`，作為第一個 module-owned caller
- [x] 已在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上 Stage 1.2 的第二個 slice：主表 insert/update SQL 生成與 audit fields 寫入骨架，先只承接主表
- [x] [src/Modules/Option/feed.cs](src/Modules/Option/feed.cs) 已改為第一個主表 save caller，可對 `tbl_option` 執行主表 insert/update
- [x] 已在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上 transaction wrapper 與 `SaveMetaAsync()`，讓主表與 `_meta` 可進入同一個 Dapper transaction
- [x] 已新增 [src/Modules/Post/feed.cs](src/Modules/Post/feed.cs) 作為第一個 `_meta` caller；因 `tbl_option` 無 `_meta` / `_lang` side table，本輪改由有現成 `tbl_post_meta` 的 Post 承接這個最小 slice
- [x] 已在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上 `SaveLangAsync()`，以 `MERGE INTO` 承接 `_lang` upsert，並先清掉同 parent 下不在本輪 payload 內的語系列
- [x] 已讓 [src/Modules/Post/feed.cs](src/Modules/Post/feed.cs) 成為第一個 `_lang` caller；`PostFeed.WriteModel` 現在可在同一個 transaction 內同時寫主表、`_meta`、`_lang`
- [x] 已在 [src/cli/Program.cs](src/cli/Program.cs) 補上 `smoke:post-save` 命令，並透過 [src/cli/Smoke/PostSaveSmoke.cs](src/cli/Smoke/PostSaveSmoke.cs) 驗證 `PostFeed` 的主表、`_meta`、`_lang` save path，完成後自動清理驗證資料
- [x] `docker compose --project-name ontocms_net --env-file .env -f conf/docker/docker-compose.yml run --rm --entrypoint dotnet web cli/OntoCms.Cli.dll smoke:post-save` 已驗證通過
- [x] 已在 [src/cli/Program.cs](src/cli/Program.cs) 補上 `smoke:post-save-rollback` 命令，透過 [src/cli/Smoke/PostSaveSmoke.cs](src/cli/Smoke/PostSaveSmoke.cs) 故意讓 `_lang.from_ai` 違反 constraint，驗證 transaction 失敗時 `tbl_post` 不會殘留資料
- [x] `docker compose --project-name ontocms_net --env-file .env -f conf/docker/docker-compose.yml run --rm --entrypoint dotnet web cli/OntoCms.Cli.dll smoke:post-save-rollback` 已驗證通過，且確認 `tbl_post`、`tbl_post_meta`、`tbl_post_lang` 都不會殘留 rollback 前的測試資料
- [x] 已新增 [src/Modules/Menu/feed.cs](src/Modules/Menu/feed.cs) 作為第二個 Feed save caller；這個 caller 只承接 `tbl_menu` 與 `tbl_menu_lang`，刻意不碰不存在的 `_meta`，用來驗證 `SaveLangAsync()` 能在 lang-only entity 上重用
- [x] 已在 [src/cli/Program.cs](src/cli/Program.cs) 補上 `smoke:menu-save` 命令，透過 [src/cli/Smoke/MenuSaveSmoke.cs](src/cli/Smoke/MenuSaveSmoke.cs) 驗證 `MenuFeed` 的主表與 `_lang` save path，完成後自動清理驗證資料
- [x] `docker compose --project-name ontocms_net --env-file .env -f conf/docker/docker-compose.yml run --rm --entrypoint dotnet web cli/OntoCms.Cli.dll smoke:menu-save` 已驗證通過，確認 `tbl_menu` 與 `tbl_menu_lang` 可由第二個 lang-only caller 正常寫入並清理
- [x] 已獨立建立 [src/conventions/HMVC/BaseRelationRepository.cs](src/conventions/HMVC/BaseRelationRepository.cs)，承接 relation table/key helper 與 `saveMany` payload shaping，避免 relation 邏輯回流到 FeedBase
- [x] 已建立 [src/conventions/HMVC/BaseOutfitController.cs](src/conventions/HMVC/BaseOutfitController.cs)，承接 route lifecycle hook、theme render helper、breadcrumb/formatter helper
- [x] `src/Modules/Option/outfit.cs` 已改為繼承 `BaseOutfitController`，module outfit 只保留 entity-owned query 與 page model 組裝
- [x] Stage 4 文件已明確決定 OutfitBase 函式邊界：承接 route lifecycle、theme render、breadcrumb/formatter helper；不承接 entity query、SEO 決策、或 static/XML/XLS runtime 輸出
- [x] 已建立 [src/conventions/HMVC/BaseReactionController.cs](src/conventions/HMVC/BaseReactionController.cs)，承接 reaction lifecycle hook、JSON envelope helper、`id/query` normalize helper、以及 `HandleRow()` / `HandleIteratee()` / `BeforeSaveAsync()` hook
- [x] `src/Modules/Option/reaction.cs` 已改為繼承 `BaseReactionController`，module reaction 只保留 entity-owned feed call 與 route declaration
- [x] Stage 2 文件已明確決定 ReactionBase 函式邊界：承接 API lifecycle、共通 envelope、request normalize、row/save hook；不承接 entity auth policy、dynamic rerouter、或 upload 流程
- [x] 已建立 [src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs)，承接 rule group selector、rule factory helper、與 `strWidth()` 對應的純字串 helper
- [x] `src/Modules/Option/kit.cs` 已改為繼承 `BaseKit`，module kit 只保留 entity-owned rule group wrapper
- [x] Stage 3 文件已明確決定 KitBase 函式邊界：承接 rule group/factory helper 與純 utility；不承接 account/login/mail/session/DB side effect
- [x] 已建立 [src/conventions/HMVC/BaseSmoke.cs](src/conventions/HMVC/BaseSmoke.cs) 與 [src/conventions/HMVC/SmokeContractException.cs](src/conventions/HMVC/SmokeContractException.cs)，承接 smoke contract dispatch、method map helper、與共通 contract exception
- [x] `src/Modules/Option/smoke.cs` 已改為繼承 `BaseSmoke`，module smoke 只保留 entity-owned surface/contract mapping 與未來 smoke scenario method
- [x] 文件已明確決定 SmokeBase 函式邊界：承接 contract dispatch / map / exception；不承接 module-specific DB bootstrap、assertion、cleanup

## Stage 0 Acceptance Checklist
- [x] Docker compose 可展開並啟動 `web` / `db`
- [x] `document/sql/*.sql` 已完成 MSSQL 化並可在 SQL Server 2022 完整落庫
- [x] DB bootstrap 已改為顯式 CLI，使用者可在需要時自行決定是否重建 / 初始化資料庫
- [x] 首頁 SSR 的 site title 仍讀取 `tbl_option` 的 `group = page`、`name = title`，首頁主內容則改為讀取 `tbl_post.slug = about`
- [x] `api/option/get?id=1` 已能返回同一筆 option，作為首頁 cross-check
- [x] 最終 HTTPS 驗收入口 `https://loc.f3cms.com:4433/` 與 `https://loc.f3cms.com:4433/api/option/get?id=1` 已驗證可用

## Open Items
- [x] Stage 1.2 的第一個可驗證行為已定為 `_handleColumn` 欄位分流，而非 audit fields 先行
- [ ] 若引入第一個 `_lang` 之後還要擴充 FeedBase，下一步應優先補最小 runtime 驗證或第二個 caller，而不是把主表、`_lang`、`_meta` 的寫入順序上推成 generic magic
- [ ] 若引入 QueryBuilder，需維持薄 where / order / paging 組裝；複雜查詢仍直接寫 SQL，第二選項僅考慮 `SqlKata`
- [ ] 決定 relation base 的第一個可驗證 caller，要先承接 `saveMany`、`lotsSub`、`byTag`、或 counter 類行為中的哪一個
- [ ] 若要繼續擴充 Feed save path，下一個最小延伸應轉向 relation base 的第一個 caller，或補另一個 `_meta` caller；第二個 lang-only caller 已先由 Menu 覆蓋
- [ ] 若要繼續擴充 OutfitBase，下一個最小 caller 應先落在 breadcrumb 或共通 theme render，不直接把 `Outfit.php` 的 static cache / XML/XLS 輸出整包搬入
- [ ] 若要繼續擴充 ReactionBase，下一個最小 caller 應先落在 list/get/save 三者之一的共通 wrapper，不直接把 `Reaction.php` 的 dynamic rerouter 或 upload 流程整包搬入
- [ ] 若要繼續擴充 KitBase，下一個最小 caller 應先落在 save/login 其中一組 validation rule wrapper，不直接把 `Staff/kit.php` 的 login/session/mail side effect 整包搬入
- [ ] 若要繼續擴充 SmokeBase，下一個最小 caller 應先挑單一 surface/contract scenario，不直接把 `Mobile/smoke.php` 的整包 DB bootstrap 與 assertion 一次搬入
