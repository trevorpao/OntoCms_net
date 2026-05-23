
# OntoCMS (.NET 版) 核心框架

OntoCMS 是一套基於 **「以實體為優先 (Entity-First)」** 與 **「Hierarchical FORKS 分層架構」** 理念所建構的 .NET 企業級單站內容管理系統 (CMS) 基礎框架。

本專案源自於 F3CMS 的核心精神，在保持優良資料庫正規化與表分解原則（主表、`_lang`、`_meta`）的前提下，將應用層全面轉移至 ASP.NET Core (C#)。我們拒絕「以頁面為中心」的義大利麵條式代碼 (Spaghetti code) 與過度肥重的全自動 ORM 魔術，致力於打造一套符合企業級資安、強型別約束與極致效能的現代化 CMS 架構。

## 🌟 核心架構：Hierarchical FORKS

OntoCMS 嚴格遵守「一個實體對應一個模組」的規範，並將系統邊界優雅地折射為五個層級（FORKS）：

*   **[F]eed (資料生命週期層)**：實體資料的唯一操作根基。基於 **Dapper (Micro-ORM)** 實作，負責精準攔截並拆解對主表、多語系表與中繼資料表的分段 CRUD 寫入，確保交易一致性。
*   **[O]utfit (畫面與渲染層)**：專責伺服器端渲染 (SSR)、SEO 設定與頁面路由，嚴禁在此層處理資料庫寫入。
*   **[R]eaction (互動與 API 層)**：基於 ASP.NET Web API，處理前端 JSON/AJAX 請求。負責權限攔截 (Claim-based Auth)、表單驗證 orchestration，並呼叫 Feed 層。
*   **[K]it (共用工具層)**：放置無狀態的共用領域邏輯與模組專屬規則。
*   **[S]moke (冒煙測試防護層)**：作為第一級架構公民的測試防護網。以最低成本確保系統在重構與迭代時，主幹道 (Happy Path) 絕對暢通。

## 📂 專案目錄結構與責任邊界 (Directory Structure)

專案結構嚴格定義了技術、慣例與業務邏輯的防腐邊界，禁止隨意打破分層：

```text
OntoCMS/
├── src/
│   ├── public/                    # 1. 最薄的網站入口
│   │   ├── appsettings.json
│   │   ├── Program.cs             # 僅負責 host startup
│   │   └── wwwroot/               # 對外靜態資源
│   │
│   ├── conventions/               # 2. 全 Repo 開發慣例與共用骨架
│   │   ├── BaseFeed.cs            # HMVC / FORKS 的共用合約
│   │   ├── BaseReaction.cs
│   │   └── Attributes/            # 共用的 Route, Response, Auth 標記
│   │
│   ├── Modules/                   # 3. 實體擁有者 (Entity Owners)
│   │   └── {Entity}/              # 依業務實體命名 (如 Post, Press)
│   │       ├── feed.cs            # 最小必備：實體生命週期
│   │       ├── reaction.cs        # 視需求補：API 控制器
│   │       ├── outfit.cs          # 視需求補：頁面路由
│   │       ├── kit.cs             # 視需求補：領域規則
│   │       ├── dto.cs
│   │       └── model.cs
│   │
│   ├── infra/                     # 4. 技術基礎設施實作層
│   │   ├── Data/                  # DB Connection, Dapper Adapter
│   │   └── Providers/             # SMS, OAuth, Cache, Payment Client
│   │
│   └── theme/
│       └── {themeName}/           # 5. 視覺主題與呈現層
│           ├── layouts/
│           └── templates/
```

### Root-level Runtime Config
1. `conf/docker`
放 Docker runtime 的 compose、image build 與本機容器開發設定。

不放什麼：
- application business logic
- entity-owned rule
- theme template

2. `conf/iis`
放 IIS site、application、rewrite rule 與對外 web entrypoint 的部署設定樣板。

不放什麼：
- entity business logic
- ASP.NET Core application code
- theme template

3. `conf/dotnet`
放 ASP.NET Core host、runtime、publish/deploy 與環境切換相關的設定樣板。

不放什麼：
- IIS site rule
- MSSQL schema
- module-owned rule

### 🛡 資料夾防腐守則
1.  **`src/public` 必須極薄**：只處理 Host composition。**嚴禁**放入業務邏輯、Payment 整合、Repo 慣例基底或 Theme 模板。原本習慣放在此處的自訂 Middleware 或 Filter，若屬單一實體應歸入 `Modules`，若屬全域規範應歸入 `conventions`。
2.  **`src/conventions` 是全站架構法典**：負責定義所有模組該長什麼樣子。**嚴禁**放入具體的 MSSQL 連線、Dapper 實作或單一 Entity 的商業規則。
3.  **`src/Modules/{Entity}` 是實體大腦**：最小化的 Entity 只需要 `feed.cs` 與對應的 Schema。Reaction、Outfit、Kit 只有在需求出現時才建立。
4.  **`src/infra` 負責髒活與技術細節**：專心處理資料庫連線、金流 API、Redis 與外部轉接。**嚴禁**放入 BaseFeed 等 Repo 規範，也**嚴禁**越權處理 Entity 業務規則。
5.  **`src/theme` 與 Host 脫鉤**：純粹的視覺資產。**嚴禁**包含 Controller 路由、DB 存取或 Auth/Payment 邏輯。
6.  **`conf/docker`、`conf/iis` 與 `conf/dotnet` 只承接部署與 host 設定**：它們是 runtime config，不是應用程式邏輯層。**嚴禁**把 module rule、Dapper 邏輯或 theme 模板塞進 `conf/`。

## 🚀 快速啟動與開發指南 (Quick Start & Development)

### 1. 環境需求
*   [Docker Desktop](https://www.docker.com/products/docker-desktop)
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更新版本

### 2. 系統初始化 (Bootstrap)
啟動專案（`docker compose --env-file .env -f conf/docker/docker-compose.yml up -d`）後，系統將自動套用 DDL 與 `init.sql`。系統採用「Zero Business Data」策略，啟動時將自動植入維持後台運作的最小 `tbl_menu`、`tbl_role` 與超級管理員等 Seed Data。

### 2.1 Bin Scripts
為了避免每次手打完整 compose 參數，專案目前提供以下包裝腳本：

1. `bin/build.sh`
等價於對 `conf/docker/docker-compose.yml` 執行 build，並使用 repo root 的 `.env`。

2. `bin/up.sh`
啟動目前專案的 compose services，適合本機開發與 smoke 前置。

3. `bin/down.sh`
停止目前專案的 compose services，但不刪除 volume。

4. `bin/clear.sh`
清除目前專案的 compose 資源（container / local image / volume / orphan）。它已收斂為專案範圍，不再影響整台機器上的其他 Docker 專案。

### 3. 開發實務：新增一個實體 (New Entity)
當架構決策確認需要新增 Entity 時，**最小變更路徑**如下：
1. 撰寫與執行 `sql` (Schema DDL)。
2. 建立 `src/Modules/{Entity}/feed.cs` 來擁有資料生命週期。
3. *視需求*再補上 `reaction.cs` (API)、`outfit.cs` (畫面) 或 `theme templates`。

### 4. 開發實務：串接外部 API (External Integration)
若只是要串接金流、簡訊等外部服務，而**未產生新實體**時，最常變更路徑如下：
1. 在 `src/infra` 實作 Client 或 Adapter。
2. 在 `src/Modules/{現有 Entity}` 中呼叫該基礎設施。
3. 只有當需要對外 Callback Route 時才動 `src/public`。
*(注意：除非發現該串接模式應升格為全專案的共用介面，否則不要輕易更動 `src/conventions`。)*

## 🧠 資料建模準則 (Data Modeling Principles)

在開發新功能前，請辨識 **「核心實體 (Entity)」**，並遵守表分解原則：
1. **主表 (`tbl_{entity}`)**：存放穩定、非多語系、常作為排序/過濾條件的核心營運欄位（如 `status`, `slug`, `insert_ts`）。
2. **語系表 (`tbl_{entity}_lang`)**：存放依語言變化的在地化內容（如 `title`, `content`）。
3. **中繼表 (`tbl_{entity}_meta`)**：存放具備高度擴展性、非主要查詢條件的 Key-Value 屬性。
4. **關聯表 (`tbl_{entity_a}_{entity_b}`)**：處理多對多實體關聯，嚴禁將關聯資料塞入 JSON 字串。
5. **模組專屬日誌 (`tbl_{entity}_log`)**：承接 WorkflowEngine 等操作的業務日誌（Module-owned log），確保審計軌跡與業務資料庫的 Transaction 一致性。

## 🤝 參與貢獻 (Contributing via FDD)

本專案採用 **文件驅動開發 (Flow Driven Development, FDD)** 工法。在提交任何 PR 之前，請確保：
1. 需求已在 `document/spec/<feature>/idea.md` 中收斂為具體的「實體」與「邊界」。
2. 實作計畫已在 `plan.md` 中拆分為可驗收的 Stage。
3. 程式碼變更未打破 FORKS 與 Directory Structure 的責任邊界，且通過 `check.md` 與 Smoke Test 驗證。
4. 任何架構決策漂移 (Drift) 都已如實記錄於 `history.md`。

---
*OntoCMS - 讓每一次開發，都成為穩固的工程資產。*

