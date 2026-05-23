
# Directory Structure

以下是我依你這輪所有澄清後，整理出的 OntoCms_net 最終版 Directory Structure 與責任說明。

**最終版**
```text
OntoCms_net/
├── .github/
│   ├── copilot-instructions.md
│   └── prompts/
│       ├── fdd-focus.prompt.md
│       ├── fdd-sprint.prompt.md
│       ├── fdd-review.prompt.md
│       ├── fdd-refactor.prompt.md
│       ├── fdd-retrospective.prompt.md
│       └── fdd-flow-llm-align.prompt.md
├── bin/
│   ├── build.sh
│   ├── up.sh
│   ├── down.sh
│   └── clear.sh
├── conf/
│   ├── docker/
│   │   ├── docker-compose.yml
│   │   └── Dockerfile
│   ├── iis/
│   ├── mssql/
│   └── dotnet/
├── database/
│   └── mssql/
│       ├── data/
│       └── backup/
├── document/
│   ├── flow.md
│   ├── flow.llm.md
│   ├── glossary.md
│   ├── guides/
│   ├── reference/
│   ├── sql/
│   │   ├── init.sql
│   │   ├── Schema.sql
│   │   ├── Seed.sql
│   │   └── archive/
│   └── spec/
│       ├── .current-spec.md
│       ├── prompts.md
│       └── OntoCms_net/
│           ├── idea.md
│           ├── history.md
│           ├── plan.md
│           ├── check.md
│           └── optimization.md
├── log/
│   ├── iis/
│   ├── mssql/
│   └── dotnet/
├── src/
│   ├── public/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── wwwroot/
│   ├── conventions/
│   │   ├── BaseFeed.cs
│   │   ├── BaseReaction.cs
│   │   ├── BaseOutfit.cs
│   │   ├── BaseKit.cs
│   │   ├── Attributes/
│   │   ├── Routing/
│   │   ├── Responses/
│   │   ├── Authorization/
│   │   └── HMVC/
│   ├── Modules/
│   │   ├── Post/
│   │   │   ├── feed.cs
│   │   │   ├── reaction.cs
│   │   │   ├── outfit.cs
│   │   │   ├── kit.cs
│   │   │   ├── model.cs
│   │   │   └── dto.cs
│   │   ├── Menu/
│   │   ├── Option/
│   │   ├── Staff/
│   │   └── Role/
│   ├── infra/
│   │   ├── Data/
│   │   ├── Auth/
│   │   ├── Cache/
│   │   ├── Http/
│   │   ├── Payments/
│   │   └── Sql/
│   ├── theme/
│   │   └── default/
│   │       ├── layouts/
│   │       ├── partials/
│   │       ├── frontend/
│   │       ├── backend/
│   │       └── assets/
│   └── tests/
│       ├── Unit/
│       ├── Integration/
│       └── Smoke/
├── OntoCms_net.sln
└── README.md
```

**核心原則**
1. `src/public` 是最薄的網站入口。
只放 host bootstrap、設定與對外靜態資源，不承接 entity 邏輯，不承接 repo-level conventions。

2. `src/conventions` 是整個 repo 的慣例來源。
它對應你現在說的「不是某個 entity 擁有，但屬於 OntoCms 自己的共用規則」。
這一層最接近你在 F3CMS 中感受到的兩種慣例來源裡，「專案自己那一側」的角色。

3. `src/Modules/{Entity}` 是 entity owner。
最小新增 Entity 時，真正必要的 owner code 先收斂在 `feed.cs`。
其他像 `reaction.cs`、`outfit.cs`、`kit.cs` 只有在需求出現時才補；而 `kit.cs` 與 `src/conventions` 都可以承接不屬於單一 CRUD 畫面的應用層邏輯。

4. `src/infra` 是技術實作層。
它不負責 repo 的開發慣例，只負責把 MSSQL、Dapper、HTTP client、payment provider、cache、auth provider 等技術接起來。

5. `src/theme/{themeName}` 是現在 theme 的對應層。
它對應你原本對 theme 的直覺，不應和 public host 綁在一起。

6. `conf/iis` 與 `conf/dotnet` 是部署與 host 設定層。
`conf/iis` 放 IIS site / application / rewrite rule 之類的 web entrypoint 設定；`conf/dotnet` 放 ASP.NET Core host、runtime、publish / deploy 的設定樣板。它們不是 app layer，也不是 infra 實作層。

**各資料夾責任**
1. `src/public`
放什麼：
- Program 啟動入口
- appsettings
- wwwroot
- 最少量 host composition

不放什麼：
- entity business logic
- payment provider integration
- repo-wide base classes
- theme templates

2. `src/conventions`
放什麼：
- BaseFeed
- BaseReaction
- BaseOutfit
- BaseKit
- route / response / auth attribute 的 repo-wide contract
- HMVC / FORKS 的共用骨架
- generic module dispatch convention

不放什麼：
- MSSQL connection
- Dapper adapter
- payment API client
- Redis / cache provider
- entity-specific business rule

3. `src/Modules/{Entity}`
放什麼：
- `feed.cs`
- `reaction.cs`
- `outfit.cs`
- `kit.cs`
- `model.cs`
- `dto.cs`

最小新增 Entity 時，至少動：
- sql
- `src/Modules/{Entity}/feed.cs`

4. `src/infra`
放什麼：
- DB connection / transaction
- Dapper implementation
- payment client
- SMS / OAuth / mail adapter
- cache adapter
- webhook transport / HTTP implementation

不放什麼：
- BaseFeed / BaseReaction 這種 repo convention
- entity owner business rule
- theme layout

5. `src/theme/{themeName}`
放什麼：
- layouts
- partials
- frontend templates
- backend templates
- theme assets

不放什麼：
- route
- controller
- DB access
- auth
- payment orchestration

6. `conf/iis`
放什麼：
- IIS site / application 設定
- URL rewrite rule
- 對外 web entrypoint 的部署樣板

不放什麼：
- C# application code
- entity business logic
- theme template

7. `conf/dotnet`
放什麼：
- ASP.NET Core host / runtime 設定樣板
- publish / deploy 設定
- 環境切換相關設定補充

不放什麼：
- IIS site rule
- MSSQL schema
- module-owned rule

**新增一個 Entity 時**
最小變更：
1. sql
2. `src/Modules/{Entity}/feed.cs`

視需求再補：
1. `reaction.cs`
2. `outfit.cs`
3. `kit.cs`
4. theme templates
5. seed / menu / role config

**不新增 Entity，只串接外部 API 時**
最常變更：
1. `src/infra`
2. `src/Modules` 內既有 owner module
3. 視 callback / route 需要才動 `src/public`
4. 視畫面需要才動 `src/theme`

原則上不先動：
1. `src/conventions`

只有當你發現這次整合暴露出可重用、應升格為全 repo 規則的模式，才回寫到 `src/conventions`。

**關於原本 public/Middleware 與 public/Filters**
最終版不建議作為常駐資料夾存在。
它們的內容應拆流到三處：
1. entity-owned 的，回 `{Entity}/outfit.cs` 或 `{Entity}/kit.cs`
2. repo-owned convention 的，回 `src/conventions`
3. 真正 host startup 的，只留在 `src/public/Program.cs`

所以最終版的 `src/public` 刻意保持很薄。

**最終命名決策總結**
1. `public`
代表對外入口，不是邏輯層
2. `conventions`
代表整個 repo 的共用開發慣例來源
3. `Modules`
代表 entity owner 與主要應用邏輯
4. `infra`
代表技術實作
5. `theme`
代表可切換的呈現層資產

這一版最大的價值，是把你剛剛一路澄清的幾件事都固定下來了：
- `feed.cs` 是最小 Entity owner
- `public` 要夠薄，避免意外暴露
- `theme` 要像原本的 theme
- `conventions` 是 repo-wide，不是只屬於 app
- `infra` 只做技術實作，不吃 repo 規範


