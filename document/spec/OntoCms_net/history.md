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
- 已補上最薄的首頁 `HomeController` 與 Razor View，並以 Docker 驗證 `http://localhost:8080` 已可 SSR 顯示來自 `tbl_option` 中 `group = page`、`name = title` 的值 (`Demo`)。
- 已補上最薄的 `api/option/get` endpoint，並以 Docker 驗證 `http://localhost:8080/api/option/get?id=1` 可返回 `id = 1` 的 option payload，與首頁顯示的 `Demo` 對齊。
- 已補齊 Kestrel HTTPS 入口與自簽憑證生成，並以 `curl --resolve loc.f3cms.com:4433:127.0.0.1 -k` 驗證 `https://loc.f3cms.com:4433/` 與 `https://loc.f3cms.com:4433/api/option/get?id=1` 均已可用，完成 Stage 0 的最終入口驗收。
- 開發 HTTPS 憑證初始化已從 container entrypoint 內的臨時 openssl 自生，改為 host-side 的 [bin/build.sh](bin/build.sh) 先用 `mkcert` + `openssl` 在 [conf/iis](conf/iis) 生成 `loc.f3cms.com.pfx`，再由 [conf/docker/docker-compose.yml](conf/docker/docker-compose.yml) 掛載進 `/https`；[conf/dotnet/entrypoint.sh](conf/dotnet/entrypoint.sh) 現在只檢查憑證存在，不再偷偷產證書。
- [conf/docker/Dockerfile](conf/docker/Dockerfile) 現在只 publish web，不再在每次 image build 時順手 publish CLI；[bin/build.sh](bin/build.sh) 也已改為走可快取的 `docker compose build web`。
- 已開始 Stage 1.1 的第一個 Feed contract slice：在 `src/conventions` 補上 `IFeedRepository<TPayload>`、`MTBAttribute`、`MULTILANGAttribute`，並讓 web `.csproj` 實際把 conventions 檔案納入編譯；Docker build 驗證通過。
- 已補上 `BaseFeedRepository<TPayload>` 最小骨架，先只承接 `MTB` / `MULTILANG` metadata 與 table helper（main/lang/meta、primary key），不提前展開 `_handleColumn`、`saveLang()` 或 transaction；Docker build 驗證通過。
- 已把首頁 route 從 `src/public/Controllers/HomeController.cs` 收回 `src/Modules/Option/outfit.cs`，並讓 `OptionOutfit` 重用 `OptionFeed` 讀取站台名稱；首頁 view 已移到 `src/theme/default/frontend/Home/Index.cshtml`。
- 已把 `api/option/get` 與其 envelope / read path 收斂為 `src/Modules/Option/reaction.cs`、`feed.cs`、`kit.cs`，並補上 `smoke.cs` / `outfit.cs` 檔位，使 Option 首次具備完整五個 FORKS 檔案。
- 已以 Docker 驗證模組化與 theme 搬移後 `build web` 通過，且 `http://localhost:8080/` 與 `http://localhost:8080/api/option/get?id=1` 均仍返回 `Demo`。
- 已將 FeedBase 承接原則回寫到 spec：`BaseFeedRepository<T>` 明確以 Dapper 為第一選擇，承接薄共通 CRUD / transaction；entity feed 自行決定主表、`_lang`、`_meta` 的寫入順序；複雜查詢維持直接 SQL，第二選項僅保留 `SqlKata`。
- 已對照 PHP 版 `Feed.php` / `Belong.php` 補完 FeedBase 函式邊界：BaseFeed 承接 metadata/table helper、save orchestration、`_handleColumn` 骨架、meta/lang persistence、thin read helper、status/delete helper；`Belong.php` 類 relation/bind/count/saveMany 行為明確排除，不直接混入 FeedBase。
- 已將 `Belong.php` 對應的 relation 基底獨立為 [src/conventions/HMVC/BaseRelationRepository.cs](src/conventions/HMVC/BaseRelationRepository.cs)，先承接 relation table/key helper 與 `saveMany` payload shaping，不讓 `{Entity}/feed.cs` 因 relation 初始化與 payload 組裝而變胖。
- 已對照 `Outfit.php` 補上 [src/conventions/HMVC/BaseOutfitController.cs](src/conventions/HMVC/BaseOutfitController.cs)，先承接 route lifecycle hook、theme render helper、breadcrumb/formatter helper；`OptionOutfit` 已改為繼承此 base，使 `{Entity}/outfit.cs` 只保留 entity-owned query 與 view model 組裝。
- 已對照 `Reaction.php` 補上 [src/conventions/HMVC/BaseReactionController.cs](src/conventions/HMVC/BaseReactionController.cs)，先承接 API lifecycle hook、共通 JSON envelope、`id/query` normalize helper，以及 `HandleRow()` / `HandleIteratee()` / `BeforeSaveAsync()` hook；`OptionReaction` 已改為繼承此 base，使 `{Entity}/reaction.cs` 只保留 route 與 entity-owned feed call。
- 已對照 `Kit.php` 與 `modules/Staff/kit.php` 補上 [src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs)，先承接 rule group selector、rule group factory helper、與 `strWidth()` 對應的純 utility；`OptionKit` 已改為繼承此 base，使 `{Entity}/kit.cs` 可維持只保留 entity-owned rule wrapper。
- 已對照 `Smoke.php` 與 `modules/Mobile/smoke.php` 補上 [src/conventions/HMVC/BaseSmoke.cs](src/conventions/HMVC/BaseSmoke.cs) 與 [src/conventions/HMVC/SmokeContractException.cs](src/conventions/HMVC/SmokeContractException.cs)，先承接 smoke contract dispatch、surface/contract map helper、與共通 contract exception；`OptionSmoke` 已改為繼承此 base，使 `{Entity}/smoke.cs` 可維持只保留 entity-owned smoke mapping 與 scenario method。
- 已決定 Stage 1.2 的第一個可驗證行為先做 `_handleColumn` 而不是 audit fields，並在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上最小 payload normalization：先把 payload 分流為主表欄位、`meta`、`lang`、`tags`；[src/Modules/Option/feed.cs](src/Modules/Option/feed.cs) 已改用具體 `WriteModel` 觸發這個第一個 caller。
- 已依最新運行規則把 DB bootstrap 從 web startup 拔除：`Program.cs` 不再在啟動時自動執行 bootstrap；改由明確 CLI 指令 `db:bootstrap` 觸發，並新增 [bin/bootstrap-db.sh](bin/bootstrap-db.sh) 作為 Docker 友善入口，讓使用者只在需要時自行決定是否建立 / 初始化資料庫。
- 已將 DB bootstrap 的承接位置再收斂為 [src/cli/Bootstrap/DatabaseBootstrapper.cs](src/cli/Bootstrap/DatabaseBootstrapper.cs)，避免把 CLI 專用實作放在 `public` 或 `conventions`；`public` host 只保留命令分流，CLI 實作本身改由獨立 `src/cli` 承接。
- 已進一步將 DB bootstrap 正式切為獨立 CLI project：[src/cli/OntoCms.Cli.csproj](src/cli/OntoCms.Cli.csproj) 與 [src/cli/Program.cs](src/cli/Program.cs) 承接命令入口；[src/public/OntoCms.Web.csproj](src/public/OntoCms.Web.csproj) 不再編譯 `src/cli/**/*.cs`，production web build 與 CLI build 邊界已拆開。
- CLI 的 Docker 執行路徑已進一步自 web final image 抽離：目前 [conf/docker/docker-compose.yml](conf/docker/docker-compose.yml) 已新增 SDK 型 `cli` service，先作為長駐 dev container 存活；`db:bootstrap` 與 smoke command 則透過 [bin/cli.sh](bin/cli.sh) 走 `docker compose exec`，不再為每次 CLI 命令重建 container。
- [src/cli/OntoCms.Cli.csproj](src/cli/OntoCms.Cli.csproj) 已進一步與 [src/public/OntoCms.Web.csproj](src/public/OntoCms.Web.csproj) 解耦：CLI 現在直接編譯 `src/conventions`、`src/Modules` 並自行帶入 `document/sql/*.sql`，不再因 `ProjectReference` 先把整個 web host 一起編掉。
- [conf/docker/docker-compose.yml](conf/docker/docker-compose.yml) 的 `cli` service 現在改由長駐 idle container + [bin/docker-cli-entrypoint.sh](bin/docker-cli-entrypoint.sh) 承接；`bin/cli.sh` 會優先 `exec` 進既有 container，只在來源檔比 `OntoCms.Cli.dll` 新時才增量 build，否則直接執行既有 DLL。Docker 驗證下 `smoke:post-save` 已壓到首次約 1.9 秒、熱路徑約 1.3 秒。
- 已完成 Stage 1.2 的第二個最小 slice：在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上主表 insert/update SQL 生成與 audit fields (`insert_ts` / `last_ts` / `insert_user` / `last_user`) 寫入骨架；[src/Modules/Option/feed.cs](src/Modules/Option/feed.cs) 現在可作為第一個主表 save caller，先只承接 `tbl_option` 主表，不提前展開 `_lang` / `_meta`。
- 已補上 Stage 1.3 的第一個最小 slice：在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上共通 transaction wrapper 與 `SaveMetaAsync()`；同時新增 [src/Modules/Post/feed.cs](src/Modules/Post/feed.cs) 作為第一個 `_meta` caller。由於 `tbl_option` 本身沒有 `_meta` / `_lang` side table，這一輪明確改由有現成 `tbl_post_meta` 的 Post 承接，避免在 Option 上做假 caller。
- 已補上 Stage 1.3 的第二個最小 slice：在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上 `SaveLangAsync()`，以 `MERGE INTO` 承接 `_lang` upsert 與缺席語系清理；[src/Modules/Post/feed.cs](src/Modules/Post/feed.cs) 現在可在同一個 transaction 內同時承接主表、`_meta`、`_lang` 三段寫入，成為第一個 `_lang` caller。
- 對照 PHP 版 `Feed.php::saveLang()` 後，已修正 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 的 `_lang` 語意：現在只 upsert payload 內提供的語系，不再因缺席語系或空 payload 刪除既有 `_lang` rows，與 PHP 版 `saveLang()` 的 owner-side承接更一致。
- 已補上最小 runtime 驗證路徑：在 [src/cli/Program.cs](src/cli/Program.cs) 新增 `smoke:post-save` 命令，透過 [src/cli/Smoke/PostSaveSmoke.cs](src/cli/Smoke/PostSaveSmoke.cs) 實際驗證 `PostFeed` 的主表、`_meta`、`_lang` transaction save path，並在驗證後清理測試資料；Docker 路徑已驗證通過。
- 已補上 rollback 類 smoke 驗證路徑：在 [src/cli/Program.cs](src/cli/Program.cs) 新增 `smoke:post-save-rollback` 命令，透過 [src/cli/Smoke/PostSaveSmoke.cs](src/cli/Smoke/PostSaveSmoke.cs) 故意讓 `_lang.from_ai` 違反 `CK_tbl_post_lang_from_ai`，並已用 Docker 驗證 `PostFeed.SaveAsync()` 失敗時 transaction 會 rollback，`tbl_post`、`tbl_post_meta`、`tbl_post_lang` 都不殘留 partial row。
- 已補上第二個 Feed save caller：新增 [src/Modules/Menu/feed.cs](src/Modules/Menu/feed.cs) 作為 lang-only entity 的最小 caller，讓 `tbl_menu` + `tbl_menu_lang` 在不經 `_meta` 的前提下也可重用 `BaseFeedRepository` 的主表 + `_lang` save path；同時新增 [src/cli/Smoke/MenuSaveSmoke.cs](src/cli/Smoke/MenuSaveSmoke.cs) 與 `smoke:menu-save` 做 Docker runtime 驗證。
- `smoke:menu-save` 已用 Docker 驗證通過，確認第二個 caller 不需要 `_meta` 也能正確完成主表與 `_lang` 寫入，並在 smoke 結束後清理測試資料。
- 已把首頁改為由 Post 承接內容來源：目前 [src/Modules/Option/outfit.cs](src/Modules/Option/outfit.cs) 會讀取 `slug = about` 的 `PostFeed` 發布內容作為首頁主文，並以 querystring `?lang=tw|en` 切換中英語系；site title 仍沿用 `tbl_option.page/title` 作為頁首與 document title 的輔助資訊。
- 已新增 [document/sql/260524.sql](document/sql/260524.sql) 作為 about 首頁內容的 idempotent 增量資料，並以 Docker 現場套用後驗證 `http://localhost:8080/?lang=tw` 與 `http://localhost:8080/?lang=en` 都可正確返回首頁內容。
- 已把前台語言解析收斂到 [src/conventions/HMVC/BaseOutfitController.cs](src/conventions/HMVC/BaseOutfitController.cs) 的共通 helper，依 F3CMS 最小優先序承接 route → query → cookie → default fallback，並在 route/query 明確指定語言時回寫 `user_lang` cookie。
- 已補上首頁的語系前綴路由：目前 [src/Modules/Option/outfit.cs](src/Modules/Option/outfit.cs) 同時支援 `/`、`/about`、`/tw`、`/en`、`/tw/about`、`/en/about`；首頁語言切換連結也已改用前綴路徑而非 querystring。
- 已用 Docker runtime + curl 驗證首頁語言規則：`/` 走 default fallback、`/en/about` 走 route、帶 `user_lang=en` 的 `/about` 走 cookie、`/about?lang=tw` 可覆蓋 cookie、`/tw/about?lang=en` 仍以 route 優先。
- 已將語言決策規則自 [src/conventions/HMVC/BaseOutfitController.cs](src/conventions/HMVC/BaseOutfitController.cs) 抽出並泛化為 [src/conventions/HMVC/ForkLanguageResolver.cs](src/conventions/HMVC/ForkLanguageResolver.cs)，避免 lang helper 被綁死在 Outfit fork；目前 `BaseOutfitController` 與 [src/conventions/HMVC/BaseReactionController.cs](src/conventions/HMVC/BaseReactionController.cs) 都只保留 HTTP request/cookie 的薄 adapter。
- 已為 [src/conventions/HMVC/BaseRelationRepository.cs](src/conventions/HMVC/BaseRelationRepository.cs) 補上最小 `saveMany` persistence：目前可在既有 transaction 內先刪除 owner 既有 relation rows，再依 payload 重建 relation rows，不把 relation save path 回流到 FeedBase。
- 已先用 Post 的 owner-side relation helper 關閉 relation base 的第一個 caller：目前 [src/Modules/Post/feed.cs](src/Modules/Post/feed.cs) 內的 private `PostRelationRepository` 會將 `tags` payload 寫入 [document/sql/init.sql](document/sql/init.sql#L1144) 對應的 `tbl_post_tag`，不再額外長出獨立 `relation.cs`。
- 已擴充 [src/cli/Smoke/PostSaveSmoke.cs](src/cli/Smoke/PostSaveSmoke.cs) 驗證 Post save path 現在會同時覆蓋主表、`_meta`、`_lang`、`tbl_post_tag` relation；rollback smoke 也已確認 `_lang` 失敗時 `tbl_post_tag` 不會殘留 partial row。
- 已新增 [src/Modules/Adv/feed.cs](src/Modules/Adv/feed.cs) 作為第二個 `_meta` caller，讓 `tbl_adv`、`tbl_adv_meta`、`tbl_adv_lang` 可在同一個 transaction 內重用 BaseFeed 的主表、`_meta`、`_lang` save path，且不必順手擴到 relation。
- 已新增 [src/cli/Smoke/AdvSaveSmoke.cs](src/cli/Smoke/AdvSaveSmoke.cs) 與 [src/cli/Program.cs](src/cli/Program.cs) 的 `smoke:adv-save` 命令，並已用 Docker `bin/cli.sh smoke:adv-save` 驗證主表、`_meta`、`_lang` 寫入與清理都成功。
- 已在 [src/conventions/HMVC/BaseRelationRepository.cs](src/conventions/HMVC/BaseRelationRepository.cs) 補上最小 `byTag` 對應讀取 helper：目前可依單一或多個 relation ids 取回 owner ids，且多 tag 會以交集語意收斂，不把 relation read path 推回 FeedBase。
- 已讓 [src/Modules/Post/feed.cs](src/Modules/Post/feed.cs) 內的 owner-side relation helper 承接 relation base 的第二個 caller，補上 `PostFeed.GetIdsByTagAsync()` 對應 `tbl_post_tag` 的 `byTag` 查詢。
- 已擴充 [src/cli/Smoke/PostSaveSmoke.cs](src/cli/Smoke/PostSaveSmoke.cs) 與 [src/cli/Program.cs](src/cli/Program.cs) 的 `smoke:post-bytag` 命令，並已用 Docker `bin/cli.sh smoke:post-bytag` 驗證單 tag 與多 tag 交集查詢都成功。
- 已在 [src/conventions/HMVC/BaseRelationRepository.cs](src/conventions/HMVC/BaseRelationRepository.cs) 補上 owner → relation ids 讀取 helper，讓 relation read 不只停在 `byTag`，也能承接 `lotsSub` 對應的最小 owner-side 查詢。
- 已讓 [src/Modules/Post/feed.cs](src/Modules/Post/feed.cs) 內的 owner-side relation helper 再補上一層 `PostFeed.GetTagIdsAsync()`，承接 `tbl_post_tag` 的 owner → tag ids 讀取。
- 已擴充 [src/cli/Smoke/PostSaveSmoke.cs](src/cli/Smoke/PostSaveSmoke.cs) 與 [src/cli/Program.cs](src/cli/Program.cs) 的 `smoke:post-tagids` 命令，並已用 Docker `bin/cli.sh smoke:post-tagids` 驗證 owner → tag ids 讀取成功。
- 已在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上最小 SqlKata compile helper，定位只收斂為「組出 SQL + bindings」，實際執行仍交給 Dapper，不把 FeedBase 推向重型 query abstraction。
- 已將 [src/Modules/Option/feed.cs](src/Modules/Option/feed.cs) 的 `GetSiteTitleAsync()` 改為第一個 read-only pilot：目前由 SqlKata 組查詢，再由 Dapper 執行；既有 save path 不受影響。
- 已在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 進一步補上 `OneAsync()`、`LotsAsync()`、`LimitRowsAsync()` 這類 read-side 小介面，讓 SqlKata 的 compile / execute 細節集中在 base-level 入口，不散落在 feed caller。
- 已在 [src/conventions/HMVC/BaseRelationRepository.cs](src/conventions/HMVC/BaseRelationRepository.cs) 補上 `NewReadQuery()`、`CompileReadCommand()`、`ReadManyAsync()`，讓 relation read-side 也走 SqlKata + Dapper 的小介面模式；write-side 的 `ReplaceSaveManyAsync()` 仍維持 Dapper + transaction。
- 已同步調整 [src/public/OntoCms.Web.csproj](src/public/OntoCms.Web.csproj)、[src/cli/OntoCms.Cli.csproj](src/cli/OntoCms.Cli.csproj) 與 [bin/docker-cli-entrypoint.sh](bin/docker-cli-entrypoint.sh)，並已用 Docker `build web` 與 `bin/cli.sh smoke:adv-save` 驗證 web/cli 兩條 build graph 都通過。

### Drift
- 原始文件的 bootstrap 前提漂移已完成第一輪對齊，目前已不再是假設 Docker / `.NET` artifact 存在，而是已落地為實際 repo 結構。
- Stage 0 原先假設「web startup 自動 bootstrap DB」，但這會把營運修復動作綁進正常啟動路徑；本輪已改回顯式 CLI 模型，關閉這段 runtime / operations drift。
- 文件曾將首頁來源寫成抽象的 `site_name`，但目前實際 seed 與實作都以 `tbl_option` 中 `group = page`、`name = title` 為準；本輪已完成文件對齊。
- 首頁主內容原先只承接 `tbl_option.page/title` 的 walking skeleton，與現在要用 Post 建立首頁的需求不一致；本輪已把首頁主內容切到 `slug = about` 的 Post，並保留 `tbl_option.page/title` 作為 site title，關閉這段首頁內容來源 drift。
- 首頁語言切換原先只靠 `?lang=tw|en`，與 F3CMS 既有的 `/{lang}/...` 前綴規則不一致；本輪已補上 route prefix 與共通語言 helper，關閉這段多語系 route drift。
- Gene Panel 參考文件中的 `option/get` 範例仍沿用舊的 `group = site`、`name = site_name` 假設；本輪已同步改回目前 seed 事實。
- 目前剩餘的不是 repo 內 runtime gap，而是本機若要在瀏覽器直接輸入 `loc.f3cms.com`，仍需外部 DNS 或 hosts 設定；repo 內已完成 HTTPS service、憑證與 4433 port mapping，本輪驗收以 `curl --resolve` 關閉此差距。
- `conf/iis` 現在作為開發 HTTPS 憑證落點，但實際 `.pem` / `.key` / `.pfx` 已改由 root `.gitignore` 排除，只保留 `.gitkeep`，避免憑證進入 git。
- `src/conventions` 原本只有目錄骨架，尚未被任何 `.csproj` 編譯；若直接在此落地 Feed contract，會形成「文件有分層、實際 build 卻不承接」的程式 / 文件漂移。本輪已先補齊這個編譯邊界。
- 首頁與 Option API 原先仍分散在 `src/public`，與 module owner / theme owner 結構不一致；本輪已把 route 與 view 移回 `src/Modules/Option` 與 `src/theme/default/frontend`，關閉這段 owner-boundary drift。
- 先前 Stage 1 文件雖然已選 Dapper，但尚未把 FeedBase 的責任邊界寫清楚；本輪已把「Dapper-first、SQL-first、薄 QueryBuilder、entity-owned write order、`SqlKata` 僅作第二選項」補成明確規則。
- 先前文件仍未明確回答 `Feed.php` 與 `Belong.php` 哪些函式該進 BaseFeed；本輪已把這個分界寫死，避免 `{Entity}/feed.cs` 因共通責任不清而重新長胖。
- `Belong.php` 雖已從 FeedBase 排除，但若沒有獨立 relation base，entity feed 仍會重新長出 relation table/key 初始化與 row payload 組裝；本輪已補上這個最小獨立基底。
- relation base 雖已存在，但先前還沒有任何實際 caller，導致文件雖主張 relation 應獨立承接，runtime 卻尚未證明這條路徑可用；本輪已用 Post/tag 的第一個 caller 與 Docker smoke 關閉這段 relation-base drift。
- 先前文件要求「下一步優先補另一個 `_meta` caller」，但 runtime 尚只有 Post 一個 `_meta` 實例；本輪已用 Adv 作為第二個 `_meta` caller 關閉這段 caller coverage drift。
- 先前文件雖已收斂 relation 第二步應做 `counter` 或 `byTag`，但 runtime 尚未有任何 relation read caller；本輪已用 Post/tag 的 `byTag` caller 與 Docker smoke 關閉這段 relation-read coverage drift。
- 先前文件把下一步直接收斂到 relation `counter`，但目前 repo schema 找不到任何現成 `<relation>_cnt` 欄位，且 PHP modules 端也沒有可直接對照的 `setCnt()` caller；本輪先把這段規劃漂移顯性化，改以同一條 relation 線上的 owner → relation ids 讀取作為最小替代 slice。
- 先前文件雖已把 `SqlKata` 定位為第二選項，但 repo 內還沒有任何實際 caller，導致規則存在於文件、runtime 卻沒有最小證明；本輪已用 `OptionFeed.GetSiteTitleAsync()` 的 read-only pilot 關閉這段 SqlKata adoption drift。
- 先前文件仍把 relation caller 寫成獨立 `src/Modules/Post/relation.cs`，與目前 owner-file-first 結構不一致；本輪已改回以 [src/Modules/Post/feed.cs](src/Modules/Post/feed.cs) 內的 private relation helper 為準，關閉這段 owner-boundary drift。
- 先前文件雖已寫明 `SqlKata` 僅限 read-side，但尚未明確反映 compile / execute 細節應收在小介面；本輪已以 `OneAsync()` / `LotsAsync()` / `LimitRowsAsync()` 與 relation base read helper 關閉這段 read-side interface drift。
- 先前 repo 沒有任何 OutfitBase，導致 `Outfit.php` 裡可共用的 route lifecycle / breadcrumb / formatter helper 沒有承接點；本輪已補上最小 `BaseOutfitController`，但仍刻意排除 static cache / XML/XLS 等 runtime-heavy 輸出。
- 先前 repo 沒有任何 ReactionBase，導致 `Reaction.php` 裡可共用的 envelope / lifecycle / request guard helper 沒有承接點；本輪已補上最小 `BaseReactionController`，但仍刻意排除 dynamic rerouter、upload、與 entity-specific auth / validation。
- 先前 repo 沒有任何 KitBase，導致 `Kit.php` 裡可共用的 rule selector / utility helper 沒有承接點；本輪已補上最小 `BaseKit`，但仍刻意排除 `Staff/kit.php` 類 account/login/mail/session side effect。
- 先前 repo 沒有任何 SmokeBase，導致 `Smoke.php` 裡可共用的 contract dispatch 與錯誤類型沒有承接點；本輪已補上最小 `BaseSmoke`，但仍刻意排除 `Mobile/smoke.php` 類 module-owned DB bootstrap / assertion / cleanup。

### Next Step
- 目前 relation base 的 read path 已由 Post/tag 的 `byTag` 與 owner → tag ids caller 關閉，且 Docker smoke 已驗證成功；若要繼續往 relation `counter` 推進，下一步應先找到或引入一個真正具備 `<relation>_cnt` 欄位的 schema-backed caller，而不是把 `counter` 硬做成 generic magic。
- `SqlKata` 目前已在 FeedBase / RelationBase 內有最小 read-side compile helper 與 owner-facing 小介面；若要繼續擴充，下一步應優先落在 list/paginate 類 read path，而不是把 save/upsert/transaction orchestration 改寫成 builder-first。
