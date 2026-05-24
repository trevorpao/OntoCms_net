
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
OntoCms_net/
├── bin/                         # Docker / bootstrap scripts
│   ├── bootstrap-db.sh          # 顯式 DB bootstrap 入口
│   ├── build.sh
│   ├── up.sh
│   ├── down.sh
│   └── clear.sh
│
├── conf/                        # Docker / host / deploy 設定
│   ├── docker/
│   ├── dotnet/
│   ├── iis/
│   └── mssql/
│
├── document/                    # FDD、guide、SQL 與 spec source-of-truth
│   ├── guides/
│   ├── reference/
│   ├── spec/
│   └── sql/
│
├── src/
│   ├── cli/                     # 顯式 CLI command project
│   │   ├── Bootstrap/
│   │   ├── OntoCms.Cli.csproj
│   │   └── Program.cs
│   │
│   ├── public/                    # 1. 最薄的網站入口
│   │   ├── appsettings.json
│   │   ├── OntoCms.Web.csproj
│   │   ├── Program.cs             # 僅負責 host startup
│   │   └── wwwroot/               # 對外靜態資源
│   │
│   ├── conventions/               # 2. 全 Repo 開發慣例與共用骨架
│   │   ├── Attributes/
│   │   ├── Authorization/
│   │   ├── HMVC/                  # BaseFeedRepository, BaseReactionController...
│   │   ├── Responses/
│   │   └── Routing/
│   │
│   ├── Modules/                   # 3. 實體擁有者 (Entity Owners)
│   │   └── Option/
│   │       ├── feed.cs            # 最小必備：實體生命週期
│   │       ├── reaction.cs        # 視需求補：API 控制器
│   │       ├── outfit.cs          # 視需求補：頁面路由
│   │       ├── kit.cs             # 視需求補：領域規則
│   │       └── smoke.cs           # 視需求補：冒煙測試契約
│   │
│   ├── infra/                     # 4. 技術基礎設施實作層
│   │
│   └── theme/                     # 5. 視覺主題與呈現層
│       └── default/
│           ├── frontend/
│           ├── layouts/
│           ├── partials/
│           └── assets/
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
3.  **`src/cli` 是顯式操作命令層**：像 DB bootstrap 這類維運命令放在獨立 CLI project，由 image 一起交付，但不回流到 web startup，也不由 web project 編譯承接。
4.  **`src/Modules/{Entity}` 是實體大腦**：最小化的 Entity 只需要 `feed.cs` 與對應的 Schema。Reaction、Outfit、Kit、Smoke 只有在需求出現時才建立。
5.  **`src/infra` 負責髒活與技術細節**：專心處理資料庫連線、金流 API、Redis 與外部轉接。**嚴禁**放入 BaseFeed 等 Repo 規範，也**嚴禁**越權處理 Entity 業務規則。
6.  **`src/theme` 與 Host 脫鉤**：純粹的視覺資產。**嚴禁**包含 Controller 路由、DB 存取或 Auth/Payment 邏輯。
7.  **`conf/docker`、`conf/iis` 與 `conf/dotnet` 只承接部署與 host 設定**：它們是 runtime config，不是應用程式邏輯層。**嚴禁**把 module rule、Dapper 邏輯或 theme 模板塞進 `conf/`。

## 🚀 快速啟動與開發指南 (Quick Start & Development)

### 1. 環境需求
*   [Docker Desktop](https://www.docker.com/products/docker-desktop)
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更新版本

### 2. 系統初始化 (Bootstrap)
目前 web 啟動不再自動執行資料庫 bootstrap。啟動專案後，若需要初始化或重建 DB，請明確執行 `bin/bootstrap-db.sh`，由獨立 CLI project 承接資料庫 bootstrap。

目前 compose project name 應固定為 `ontocms_net`，避免和其他語言版本的 OntoCMS 容器、network、volume 名稱衝突；`bin/*.sh` 與 `conf/docker/docker-compose.yml` 也都應維持這個名稱。

### 2.1 Bin Scripts
為了避免每次手打完整 compose 參數，專案目前提供以下包裝腳本：

1. `bin/build.sh`
等價於對 `conf/docker/docker-compose.yml` 執行可快取的 `build web`，並使用 repo root 的 `.env`。這個腳本不再強制 `--no-cache`，也不再順手 publish CLI。

2. `bin/up.sh`
啟動目前專案的 compose services，適合本機開發與 smoke 前置。它現在也會一起拉起長駐的 `cli` dev container，讓後續 CLI 命令可直接走 `docker compose exec`。

3. `bin/down.sh`
停止目前專案的 compose services，但不刪除 volume。

4. `bin/clear.sh`
清除目前專案的 compose 資源（container / local image / volume / orphan）。它已收斂為專案範圍，不再影響整台機器上的其他 Docker 專案。

5. `bin/bootstrap-db.sh`
以顯式命令方式執行資料庫 bootstrap。這個腳本現在會透過 compose 的 `cli` service 執行；該 service 只會在 `src/cli`、`src/conventions`、`src/Modules`、`document/sql` 較新時增量 build，否則直接執行既有的 `OntoCms.Cli.dll`，避免每次 CLI 啟動都重跑 `dotnet run` 的 build 檢查。

6. `bin/cli.sh`
以長駐 `cli` dev container 執行任意 CLI 命令。腳本會優先走 `docker compose exec`；只有 `db` 或 `cli` 尚未啟動時才補做 `up -d`。在目前 Docker 驗證下，`bin/cli.sh smoke:post-save` 約可壓到首次 1.9 秒、熱路徑 1.3 秒。

### 2.2 macOS HTTPS 信任鏈
如果希望在 macOS 瀏覽器中直接開啟 `https://loc.f3cms.com:4433/`，而不是每次用 `curl -k` 或手動略過警告，請先完成 hosts 與 mkcert 的本機設定。

1. 安裝 mkcert 與 nss。

```bash
brew install mkcert nss
```

2. 確認 `loc.f3cms.com` 指向本機。

```bash
sudo sh -c 'printf "\n127.0.0.1 loc.f3cms.com\n" >> /etc/hosts'
```

若 `/etc/hosts` 已有這筆資料，不要重複追加。

3. 執行 `bin/build.sh`。

```bash
bin/build.sh
```

這個腳本現在會先做以下事情，再進入 Docker build：

```text
mkcert -install
mkcert loc.f3cms.com localhost 127.0.0.1 ::1
openssl pkcs12 -export -> conf/iis/loc.f3cms.com.pfx
```

生成結果會放在 [conf/iis](conf/iis)：
- `loc.f3cms.com.pem`
- `loc.f3cms.com-key.pem`
- `loc.f3cms.com.pfx`

這些檔案已被 root [.gitignore](.gitignore) 排除，不會進 git。

4. 啟動服務。

```bash
bin/up.sh
```

5. 直接在瀏覽器開啟：

```text
https://loc.f3cms.com:4433/
```

若要用 curl 驗證完整 host/path，可用：

```bash
curl -k --resolve loc.f3cms.com:4433:127.0.0.1 https://loc.f3cms.com:4433/
```

### 2.3 信任鏈注意事項
1. `mkcert -install` 會把本機 CA 安裝到 macOS trust store；如果你使用 Firefox，還需要先安裝 `nss`，再重新執行一次 `mkcert -install`。
2. repo 內的 Docker container 只負責讀取 [conf/iis](conf/iis) 的 `loc.f3cms.com.pfx`；不再於 container 內自生憑證。
3. 若 shell 中的 `curl` 不是使用 macOS 系統 trust store，而是來自其他 OpenSSL 發行版，仍可能需要 `-k`；這不代表 Kestrel 沒有正確載入 mkcert 憑證。

### 3. 開發實務：新增一個實體 (New Entity)
當架構決策確認需要新增 Entity 時，**最小變更路徑**如下：
1. 撰寫與執行 `sql` (Schema DDL)。
2. 建立 `src/Modules/{Entity}/feed.cs` 來擁有資料生命週期。
3. *視需求*再補上 `reaction.cs` (API)、`outfit.cs` (畫面)、`kit.cs`、`smoke.cs` 或 theme templates。

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

