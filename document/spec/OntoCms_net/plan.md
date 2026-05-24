
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
*   **Thin Read Helpers**：承接 `OneAsync()`、`LotsAsync()`、`TotalAsync()`、`PaginateAsync()` 這類薄查詢 helper；複雜查詢維持直接 SQL。
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
*   **Task 1.3: 多語系與中繼資料關聯寫入 (_afterSave)**
    *   第一個 slice 先補共通 transaction 與第一個 `_meta` caller：BaseFeedRepository 先承接 transaction wrapper 與 `SaveMetaAsync()`；第一個 caller 改落在有現成 `tbl_post_meta` 的 PostFeed，不在沒有 side table 的 OptionFeed 上硬做假 caller。
    *   第二個 slice 再補 `saveLang()`：利用 MSSQL 的 `MERGE INTO` 處理 `_lang` 表的 Upsert；第一個 caller 同樣落在有現成 `tbl_post_lang` 的 PostFeed，避免為了示範而在 OptionFeed 上製造不存在的 `_lang` 前提。
    *   驗證優先以顯式 CLI smoke command 承接，例如 `smoke:post-save`；不要為了驗證 save path 而先把 module save route 或 web API 擴張進 production request path。
    *   rollback 類 smoke command 已驗證完成：透過故意讓 `_lang` 欄位違反 constraint，確認主表、`_meta`、`_lang` 都不會部分殘留。
    *   下一個最小 slice 應補第二個 caller，優先挑現成 schema 但較窄的 lang-only module，例如 `tbl_menu` + `tbl_menu_lang`，驗證 `SaveLangAsync()` 能重用於沒有 `_meta` 的 entity。
    *   實作 `saveMeta()`：處理 `_meta` 表的鍵值對寫入。
    *   `BaseFeedRepository<T>` 只承接共通 CRUD / transaction；各 entity feed 仍必須自行決定主表、`_lang`、`_meta` 的寫入順序與 owner-side orchestration。
    *   確保 Task 1.2 與 1.3 包裝在同一個 `DbTransaction` 內。
    *   `Belong.php` 類的 relation 寫入（如 `saveMany` / `bind`）不納入本階段 FeedBase；若未來需要，另立 relation helper / base 再承接。
*   **[驗收點]**：撰寫整合測試 (Integration Test)，傳入一份包含 title (`_lang`) 與 seo_desc (`_meta`) 的 Payload，驗證 Dapper 能正確分流並寫入三張表；同時確認 `BaseFeedRepository<T>` 只提供薄共通能力，不會把 Feed 推向 Entity Framework 式 ownership 混淆。

#### Stage 2: 互動與權限治理 (Reaction & Auth 層)
**目標**：基於舊表建立 C# 權限攔截網，並實作標準的 API 回應層。
*   **ReactionBase 預定承接函式群**：
    *   API lifecycle hook：`BeforeReactionAsync()`、`AfterReactionAsync()`、共通 `ExecuteReactionAsync()` wrapper。
    *   JSON envelope helper：成功 / 缺欄 / wrong-data / unverified 的共通 envelope 與 response helper。
    *   Request normalization helper：`id`、`query` 這類 backend 常用輸入的最薄 normalize / guard helper。
    *   Row / Save hook：`HandleRow()`、`HandleIteratee()`、`BeforeSaveAsync()` 這類可覆寫 hook，讓 entity reaction 只保留 owner-side rule。
*   **不應放進 ReactionBase 的函式群**：
    *   entity-specific auth policy、feed method 選擇、validation rule 定義、module reroute 規則。
    *   `Reaction.php` 裡動態反射 module/method dispatch 的 `do_rerouter()`，不直接搬進 ASP.NET Core；路由解析維持由 framework routing 與 module controller 明確宣告。
    *   upload / upload_file 這類檔案處理流程，先維持 module-owned 或後續獨立 upload helper，不提前 generic 化進 ReactionBase。
*   **Task 2.1: Claim-based Auth 中介軟體**
    *   實作自訂的 `AuthenticationHandler`。
    *   讀取登入者的 `tbl_staff` 狀態與 `tbl_role` 權限代碼，轉化為 `.NET Claims`。
*   **Task 2.2: 權限常數與攔截器 (Action Filters)**
    *   實作 `[OntoAuthorize(PV_U)]` 等屬性標記，能在進入 Controller 前依據 Claims 拒絕越權存取 (回傳 403)。
*   **Task 2.3: Reaction API 控制器骨架**
    *   建立標準的 JSON 回應格式 (`{ status, data, error }`)。
    *   實作如 `rPost` 的 Controller，負責接收 Request、呼叫 Task 1 完成的 `Feed`，並回傳結果。
*   **[驗收點]**：未帶憑證或權限不足的請求呼叫 `Reaction` API 時，會被 Middleware 正確攔截；合法請求能正確觸發 `Feed` 寫入。

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

