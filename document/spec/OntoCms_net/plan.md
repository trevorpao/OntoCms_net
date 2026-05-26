
# OntoCMS (.NET 版) 核心框架 - plan.md (v1 實踐計畫)

### Purpose
*   將 `idea.md` (v3) 中確認的「實體優先 (Entity-First)」與「FORK 架構 .NET 現代化」決策，拆解為可執行的階段 (Stages) 與子任務。
*   控制第一版重構範圍，確保系統從最小可運作骨架 (Walking Skeleton) 穩步生長，避免陷入「一次性重寫整個框架卻無法編譯」的風險。
*   為後續的 `(done)` 與 `check.md` 驗收提供明確依據。

### Plan Basis
本計畫嚴格遵守 `idea.md` (v3) 的以下決議：
*   **定位**：單一 CMS 核心框架，不處理多站共構 (PetiteCMS) 邏輯。
*   **架構**：ASP.NET Core 8/9 + MSSQL，採用 Server-Side Rendering (SSR) Razor Pages。
*   **資料存取**：使用 Dapper，嚴格實作主表、`_lang`、`_meta` 的分段寫入，禁用全自動 ORM Tracking 魔術。
*   **初始資料**：Schema 轉換 + `init.sql` 核心配置植入 (Seeding)，無歷史業務資料搬遷。
*   **認證授權**：沿用 `tbl_staff` 與 `tbl_role` 的 Claim-based Authorization。

### High-Risk Areas
*   **Feed 分段寫入的交易邊界 (Transaction)**：若主表寫入成功但 `_lang` 寫入失敗，會導致實體破碎。必須在 `BaseFeedRepository` 中嚴格控制 Dapper 的 `SqlTransaction`。
*   **DTO 與 Schema 欄位映射**：C# 為強型別，若 Request JSON 解析時未能與動態的 `_meta` 或 `_lang` 欄位乾淨脫鉤，容易造成 `Reaction` 層充滿髒 code。
*   **權限攔截的生命週期**：若 Authorization Middleware 讀取資料庫的頻率過高，將成為效能瓶頸，必須妥善設計 Claims 快取 (Cache) 策略。

### Priority-1 Feature Baseline
目前 spec 不把「完整 OntoCMS 所有功能」一次攤平成同一波實作，而是先收斂第一優先序功能，作為當前可執行的核心閉環：
*   **P1-A 內容發布核心**：單頁 / 文章 / 新聞 / 公告 / 專案的共通內容發布基線，以及多語系、封面、版型、上線時間、狀態控管。
*   **P1-B 導覽整理核心**：分類、標籤、選單階層。
*   **P1-C 素材與附屬資料核心**：媒體 / 圖片素材、meta 與內容發布直接需要的附屬資料。
*   **P1-D 後台治理核心**：staff、role、登入 / 憑證、系統選項與共用設定。
*   **Deferred from P1**：搜尋、作者 / 書籍 / 術語等獨立內容關聯對象、聯絡表單、訂閱資料、廣告管理、追蹤紀錄與其他營運輔助資料，不作為目前第一優先序的前置阻塞項。

---

### Stage Plan (實作階段拆解)

#### Stage 0: 基礎設施與 Walking Skeleton (環境建置)
**目標**：建立本機開發環境與空框架，打通「環境 ➔ DB ➔ 路由 ➔ Hello World 畫面」的最短路徑。
*   **Task 0.1: Docker 環境搭建**
    *   撰寫 `conf/docker/docker-compose.yml`，包含 `.NET 8/9 Web` 容器與 `MSSQL` 容器。
    *   設定本機 Port 映射與 Volume 掛載。
*   **Task 0.2: 專案結構初始化**
    *   建立 ASP.NET Core MVC 專案，依照 F3CMS 規範建立 `Libs/`、`Modules/`、`Theme/` 資料夾。
    *   安裝核心 NuGet 套件：`Dapper`、`Microsoft.Data.SqlClient`。
*   **Task 0.3: 資料庫 Schema 轉換與 Seeding**
    *   將 `document/sql/init.sql` 與每日增量 SQL 改寫為 MSSQL 版本 (如 `DATETIME2`, `NVARCHAR(MAX)`, `IDENTITY`)。
    *   保留 `init.sql` 內核心配置與預設資料，並確保增量 SQL 可依序落庫。
    *   提供獨立 CLI project 與指令執行 `document/sql/*.sql` bootstrap，避免每次 web startup 自動建立或重建資料庫，也避免 production web build 承接 CLI source。
*   **Task 0.4: 第一個 Outfit 路由驗證**
    *   建立基礎 `HomeController` (Outfit 層) 與對應的 Razor View。
    *   從 MSSQL 讀取一筆 `tbl_option` 中 `group = page`、`name = title` 的系統名稱並渲染於網頁上，驗證連線與 SSR 正常。
    *   驗證入口固定為首頁 `https://loc.f3cms.com:4433/`，並以 `https://loc.f3cms.com:4433/api/option/get?id=1` 作為同一筆 `tbl_option` 的 API cross-check。
*   **[驗收點]**：先以獨立 CLI 完成 DB bootstrap，再執行 `docker compose --env-file .env -f conf/docker/docker-compose.yml up`；之後開啟 `https://loc.f3cms.com:4433/` 能正確渲染帶有 `tbl_option.page/title` site title 與 `tbl_post.slug = about` 首頁主內容的 SSR 首頁，且 `https://loc.f3cms.com:4433/api/option/get?id=1` 可返回對應 Option 資料，無編譯或連線錯誤。
*   **補充規則**：前台多語系頁面應優先承接 F3CMS 的 route-first 規則，至少同時支援無前綴與 `/{lang}/...` 前綴兩套路由；目前最小 helper 已先收斂 route → query → cookie → default fallback。

#### Stage 1: 實體資料生命週期 (Feed 層核心實作)
**目標**：以 Dapper 作為第一選擇，實作 OntoCMS 靈魂的 `BaseFeedRepository<T>`，維持 SQL 為第一級公民，避免 ORM magic 模糊 owner boundary。

**BaseFeedRepository<T> 預定承接函式群**：
*   **Metadata / Table Helper**：承接 `MTB`、`MULTILANG`、主表 / `_lang` / `_meta` 的 table name helper、primary key helper。
*   **Save Orchestration**：承接 `SaveAsync()` 主流程骨架、insert/update 分流、共通 transaction 持有，以及 `AfterSaveAsync()` hook。
*   **Payload Normalization**：承接 `_handleColumn` 對應的欄位分流骨架，將主表欄位、`meta`、`lang` 與其他附屬資料拆開，但不在 BaseFeed 內替 entity 決定最終寫入順序。
*   **Meta / Lang Persistence**：承接 `SaveMetaAsync()`、`SaveLangAsync()` 這類可重用的 `_meta` / `_lang` 寫入函式。
*   **Thin Read Helpers**：承接 `OneAsync()`、`LotsAsync()`、`LimitRowsAsync()` 這類薄查詢 helper；`SqlKata` 的 compile / execute 細節應集中在這些 owner-side 或 base-level 入口，複雜查詢維持直接 SQL。
*   **Status / Delete Helpers**：承接 `PublishAsync()`、`ChangeStatusAsync()`、`DeleteRowAsync()` 這類單表共通操作。

**不應放進 BaseFeedRepository<T> 的函式群**：
*   `Belong.php` 類型的 `bind`、`saveMany`、`setCnt`、`lotsSub`、`byTag` 等 relation / belong 行為，不應直接上推到 FeedBase。
*   這類函式涉及 entity-specific relation、counter、sub-table owner 規則，應留在 entity feed、module-owned helper，或未來獨立的 relation base，而不是混進共通 FeedBase。
*   目前 repo 已新增獨立的 `BaseRelationRepository` 作為這個分流起點；後續 relation/belong 共通能力應優先往 relation base 收斂，而不是回流到 FeedBase。
*   **Task 1.1: 介面與常數定義**
    *   定義 `IFeedRepository` 介面。
    *   實作 C# 版的 `MTB` (主表)、`MULTILANG` 屬性標記 (Attributes)。
    *   明確定義 `BaseFeedRepository<T>` 的定位：承接共通 CRUD、共通 transaction 持有、以及主表 / `_lang` / `_meta` table metadata helper。
*   **Task 1.2: _handleColumn 與主表 CRUD**
    *   第一個 slice 先落 `_handleColumn`：先把 payload 分流為主表欄位、`meta`、`lang`、`tags`，維持純記憶體 normalization，不提前接 transaction 或 DB write。
    *   第二個 slice 再補 Dapper 對主表 (Main Table) 的動態 Insert/Update 語法生成與 Audit Fields (`insert_ts`, `last_ts`, `insert_user`, `last_user`) 寫入，先只承接主表，不提前展開 `_lang` / `_meta`。
    *   若需要 QueryBuilder，只允許很薄的 where / order / paging 組裝；不可把整個 Feed 推向 ORM-style query abstraction。
    *   複雜查詢維持直接寫 SQL；若 Dapper 之外需要第二選項，僅考慮 `SqlKata` 作為薄 query builder，而不是改採重型 ORM。
    *   `SqlKata` 的落點應對齊 PHP `Feed.php::exec` 的薄執行思路：只負責 compile 查詢與 bindings，實際執行、transaction 與 ownership orchestration 仍由 Dapper + Feed owner 承接。
*   **Task 1.3: 多語系與中繼資料關聯寫入 (_afterSave)**
    *   第一個 slice 先補共通 transaction 與第一個 `_meta` caller：BaseFeedRepository 先承接 transaction wrapper 與 `SaveMetaAsync()`；第一個 caller 改落在有現成 `tbl_post_meta` 的 PostFeed，不在沒有 side table 的 OptionFeed 上硬做假 caller。
    *   第二個 slice 再補 `saveLang()`：利用 MSSQL 的 `MERGE INTO` 處理 `_lang` 表的 Upsert；第一個 caller 同樣落在有現成 `tbl_post_lang` 的 PostFeed，避免為了示範而在 OptionFeed 上製造不存在的 `_lang` 前提。
    *   驗證優先以顯式 CLI smoke command 承接，例如 `smoke:post-save`；不要為了驗證 save path 而先把 module save route 或 web API 擴張進 production request path。
    *   rollback 類 smoke command 已驗證完成：透過故意讓 `_lang` 欄位違反 constraint，確認主表、`_meta`、`_lang` 都不會部分殘留。
    *   第二個 lang-only caller 已由 `tbl_menu` + `tbl_menu_lang` 關閉，確認 `SaveLangAsync()` 可重用於沒有 `_meta` 的 entity。
    *   relation base 的第一個 caller 已由 `PostFeed` 內的 owner-side private `PostRelationRepository` 承接 `tbl_post_tag` 關閉；既有 `smoke:post-save` / `smoke:post-save-rollback` 也已擴充驗證 relation row 的成功寫入與 rollback。
    *   另一個 `_meta` caller 已由 `AdvFeed` 承接 `tbl_adv` + `tbl_adv_meta` + `tbl_adv_lang` 關閉，並由 `smoke:adv-save` 驗證成功。
    *   relation base 的第二個單一行為已由 `PostFeed.GetIdsByTagAsync()` 承接 `tbl_post_tag` 的 `byTag` 查詢關閉，並由 `smoke:post-bytag` 驗證單 tag 與多 tag 交集行為成功。
    *   relation read 的相鄰最小 slice 已由 `PostFeed.GetTagIdsAsync()` 關閉，對應 `lotsSub` 的 owner → relation ids 查詢，並由 `smoke:post-tagids` 驗證成功。
    *   `SqlKata` 已先由 `OptionFeed.GetSiteTitleAsync()` 與 Feed/Relation base 的 read-side 小介面驗證成功；若要繼續擴充，應優先落在 list / paginate 類 read path，而不是改寫 save path。
    *   實作 `saveMeta()`：處理 `_meta` 表的鍵值對寫入。
    *   `BaseFeedRepository<T>` 只承接共通 CRUD / transaction；各 entity feed 仍必須自行決定主表、`_lang`、`_meta` 的寫入順序與 owner-side orchestration。
    *   確保 Task 1.2 與 1.3 包裝在同一個 `DbTransaction` 內。
    *   `Belong.php` 類的 relation 寫入（如 `saveMany` / `bind`）不納入本階段 FeedBase；若未來需要，另立 relation helper / base 再承接。
    *   目前 `counter` 類 caller 仍缺少 schema-backed 落點；下一個 relation slice 若要維持單一行為，應先找到或引入真正具備 `<relation>_cnt` 欄位的 caller，再補 `counter`，而不是把 `Belong.php` 的 relation 能力一次 generic 化。
*   **[驗收點]**：撰寫整合測試 (Integration Test)，傳入一份包含 title (`_lang`) 與 seo_desc (`_meta`) 的 Payload，驗證 Dapper 能正確分流並寫入三張表；同時確認 `BaseFeedRepository<T>` 只提供薄共通能力，不會把 Feed 推向 Entity Framework 式 ownership 混淆。

#### Stage 2: 互動與權限治理 (Reaction & Auth 層)
**目標**：基於舊表建立 C# 權限攔截網，並實作標準的 API 回應層。
**對應 P1 範圍**：P1-D 後台治理核心，並作為後續 P1-A 內容發布權限控管的前置基線。
*   **Task 2.0: Role Entity Baseline 先行**
    *   在進入登入 / `AuthenticationHandler` 之前，先參考 PHP 的 `docker-f3cms/www/f3cms/modules/Role` FORKS，建立 `.NET` 版的 `Role` entity baseline，而不是先把 login / session / claims 流程硬接上去。
    *   第一版重點應放在 `RoleFeed` / `RoleReaction` / `RoleOutfit` 的 owner boundary，以及 `priv` bitmask 對應的 parse / reverse / option / list 呈現 helper，不提前把 `Staff` login side effect 混進 `Role` entity。
    *   這個 baseline 還必須提供 staff login 可直接消費的最小角色權限映射，例如 authority name 清單、權限 option、`hasAuth` 類 helper；`AuthenticationHandler` 只負責讀取 staff + role 並轉成 claims，不在 handler 內重做 role 規則。
    *   這一層完成後，再讓 `AuthenticationHandler` 讀取 `tbl_staff` + `tbl_role`，避免「尚未落地 role entity，卻先發明 login / claims abstraction」的承接倒置。
*   **ReactionBase 預定承接函式群**：
    *   API lifecycle hook：`BeforeReactionAsync()`、`AfterReactionAsync()`、共通 `ExecuteReactionAsync()` wrapper。
    *   JSON envelope helper：成功 / 缺欄 / wrong-data / unverified 的共通 envelope 與 response helper。
    *   Request normalization helper：`id`、`query` 這類 backend 常用輸入的最薄 normalize / guard helper。
    *   Row / Save hook：`HandleRow()`、`HandleIteratee()`、`BeforeSaveAsync()` 這類可覆寫 hook，讓 entity reaction 只保留 owner-side rule。
    *   Reaction feed contract：建立 `IReactionGetFeed<T>`、`IReactionListFeed<T>`、`IReactionOptionsFeed<T>` 這類最薄 shared contract，讓 `ReactGetAsync()` / `ReactSaveAsync()` / `ReactListAsync()` / `ReactGetOptionsAsync()` 可直接吃 feed-side contract，而不是每個 entity reaction 都重複組 delegate。
    *   Contract 目標仍是薄接縫，不替 entity 決定 auth policy、validation rule 或 feed method 選擇；shared layer 只負責把後台 CRUD 慣例與共用 helper 收斂到 ASP.NET Core controller base。
    *   Staff / module API route contract 需額外對齊舊 F3CMS 的 `/api/{module}/{method}` 外部命名慣例；以 `login` 為例，對外應落在 `/api/staff/login`，且新增 method 時不應再修改一份集中 route 表才能生效。這一層由 module-owned rerouter magic 承接，而不是要求每個 method 額外宣告一條固定 route。
*   **不應放進 ReactionBase 的函式群**：
    *   entity-specific auth policy、feed method 選擇、validation rule 定義、module reroute 規則。
    *   entity-specific middleware、login/session 相鄰 side effect 與非 CRUD method 決策，不上推到 generic `ReactionBase`；保留 rerouter magic 時，這些責任仍維持 module-owned。
    *   upload / upload_file 這類檔案處理流程，先維持 module-owned 或後續獨立 upload helper，不提前 generic 化進 ReactionBase。
*   **Task 2.1: Claim-based Auth 中介軟體**
    *   實作自訂的 `AuthenticationHandler`。
    *   讀取登入者的 `tbl_staff` 狀態與 `tbl_role` 權限代碼，並透過 `Role` entity 已提供的權限映射轉化為 `.NET Claims`。
*   **Task 2.2: 權限常數與攔截器 (Action Filters)**
    *   實作 `[OntoAuthorize(PV_U)]` 等屬性標記，能在進入 Controller 前依據 Claims 拒絕越權存取 (回傳 403)。
*   **Task 2.3: Reaction API 控制器骨架**
    *   建立標準的 JSON 回應格式 (`{ status, data, error }`)。
    *   實作如 `rPost` 的 Controller，負責接收 Request、呼叫 Task 1 完成的 `Feed`，並回傳結果。
    *   第一個 proof 可先用 `OptionReaction` 對 `OptionFeed` 落 `get` / `save`，驗證 entity reaction 只保留 route declaration 與 owner-side feed wiring；共用 flow 則由 `BaseReactionController` + reaction feed contract 承接。
    *   `Staff` login / logout / status 等 reaction API 的 route-contract resync 已先由 [src/Modules/Staff/reaction.cs](src/Modules/Staff/reaction.cs) 關閉：對外已對齊 `/api/staff/{method}`，並由 module-owned dispatcher 承接，而不是額外維護 `/authenticate` 這類脫離 module 命名的獨立路徑；下一步再回到 authority filter proof。
*   **[驗收點]**：未帶憑證或權限不足的請求呼叫 `Reaction` API 時，會被 Middleware 正確攔截；合法請求能正確觸發 `Feed` 寫入；staff 登入後所得到的 claims 應與 `Role` entity 提供的 authority mapping 一致，而不是另一套 handler 私有規則。

#### Stage 3: WorkflowEngine MVP (Kit 工具層)
**目標**：將狀態機與防禦邏輯移植為無狀態的 C# 領域服務 (Domain Service)。
*   **KitBase 預定承接函式群**：
    *   Rule group helper：`Rule()` 對應的 group selector、base rule fallback、以及空 rule group helper。
    *   Rule factory helper：建立 rule group / rule set 的薄資料結構 helper，讓 entity kit 只保留模組規則內容。
    *   Pure utility helper：`strWidth()` 這類不依賴 entity state 的純字串工具函式。
*   **不應放進 KitBase 的函式群**：
    *   entity-specific validation rule 定義、account uniqueness 檢查、寄信、token / login / session guard。
    *   `modules/Staff/kit.php` 裡的 `sendInvite()`、`_notExistAccount()`、`_accountRule()`、`_isLogin()`、`_chkLogin()` 這類模組擁有規則與 side effect，不上推到共通 KitBase。
    *   任何需要 DB、mail、session、或 module-owned workflow context 的 helper，都維持 entity kit 或後續專屬 service，不提前 generic 化。
*   **Task 3.1: JSON Definition 載入與驗證**
    *   於 `Libs/` 實作 `WorkflowEngine` 類別，能反序列化並驗證目標流程 JSON (如 `flow.json`)。
*   **Task 3.2: 狀態防護與權限判定 (Guard)**
    *   實作 `canTransit(actionCode, runtimeContext)`，檢查當前使用者 Role 是否符合，及狀態轉換是否合法。
*   **Task 3.3: 模組專屬 Log (Module-owned Log) 交易封裝**
    *   在 `Reaction` 層實作呼叫範例：先由 Engine 判定合法，接著開啟 Transaction 寫入業務主表 (`tbl_post`) 與狀態軌跡表 (`tbl_post_log`)，最後 Commit。
*   **[驗收點]**：當嘗試執行 JSON 規範外的狀態切換 (如 Draft ➔ Offlined) 時，Engine 會拋出錯誤並中止資料庫 Transaction。

#### Stage 4: 頁面渲染與後台選單 (Outfit 層)
**目標**：結合 Seed Data 產出動態生成的後台/前台框架。
**對應 P1 範圍**：P1-A 內容發布核心與 P1-B 導覽整理核心。
*   **OutfitBase 預定承接函式群**：
    *   Route lifecycle hook：`BeforeRouteAsync()`、`AfterRouteAsync()`、共通 `ExecuteOutfitAsync()` wrapper。
    *   Theme render helper：theme view path 組裝與共通 `ThemeView()`。
    *   Breadcrumb / formatter helper：breadcrumb 組裝、breadcrumb 字串輸出、date/duration/slug/url/join 這類純格式函式。
*   **不應放進 OutfitBase 的函式群**：
    *   entity-specific query、SEO 內容決策、頁面資料組裝、module-owned reroute 規則。
    *   `Outfit.php` 裡偏 runtime / deployment 的 static cache、XML/XLS header、特殊輸出流程，不在目前最小 OutfitBase slice 內承接。
*   **Task 4.1: 動態選單渲染**
    *   在 `Outfit` (Razor View Component 或 ViewComponent) 讀取 `tbl_menu` 的資料，遞迴渲染出後台左側導覽列。
*   **Task 4.2: 多語系與系統選項載入**
    *   讀取 `tbl_option` 載入全站共用變數 (如網站名稱、Logo 路徑)。
*   **[驗收點]**：啟動站臺後，Razor SSR 畫面能正確顯示 `init.sql` 中定義的選單與系統名稱。

#### Stage 5: Smoke Contract 與最小驗證面
**目標**：建立最薄的 smoke contract dispatch，使 smoke scenario 能維持 module-owned，但不重複實作 surface/contract routing。
*   **SmokeBase 預定承接函式群**：
    *   Contract dispatch：`Run()` 對應 surface/contract 到實際 smoke method。
    *   Method map helper：建立 surface/contract map 的薄 helper，避免各 entity 重複組字典。
    *   Contract exception：`surface_not_found`、`contract_not_found`、`invalid_smoke_contract` 這類共通 smoke contract 錯誤。
*   **不應放進 SmokeBase 的函式群**：
    *   entity-specific DB bootstrap、seed、assertion、cleanup。
    *   `modules/Mobile/smoke.php` 裡的 `runRequestCreateOrEnsure()` 這類模組專屬 scenario、資料準備與結果驗證，不上推到共通 SmokeBase。
    *   任何需要 feed/reaction/module side effect 的 smoke 流程，都維持 entity smoke 擁有。

---

### Entry / Exit Criteria for (done)
*   **Entry Criteria (開始實作前提)**：
    *   開發者已詳閱本 `plan.md` 與對應的 `idea.md` (v3)。
    *   Docker Desktop 與 `docker compose` 已安裝就緒；若 host 缺少 `.NET SDK`，可改以 Docker SDK 容器作為建立與驗證路徑。
*   **Exit Criteria (宣告階段完成標準)**：
    *   該 Stage 的程式碼已提交 (Commit)，並通過上述的 **[驗收點]**。
    *   產生對應的 SQL 或架構變更時，必須維持 F3CMS 的小寫底線 (`tbl_`) 命名規範。
    *   若實作過程中發現架構或邊界漂移 (Drift)，已同步更新 `history.md`。

