# OntoCms_net History

### 第 5 輪討論結果
1. 本輪承接前一輪已辨識出的最小 shared Reaction slice，先對照 PHP 的 [docker-f3cms/www/f3cms/libs/Reaction.php](/Users/trevor/bitbucket/docker-f3cms/www/f3cms/libs/Reaction.php) 補 shared layer proof，而不是直接擴張更多 `{Entity}`-specific flow。
2. [src/Modules/Option/feed.cs](src/Modules/Option/feed.cs) 現在已實作 `IReactionListFeed<OptionRecord>` 與 `IReactionOptionsFeed<OptionOption>`，以 `LimitRowsAsync()` 與 `GetOptionsAsync()` 承接 `api/option/list` / `api/option/get_opts` 需要的最小 read-side feed contract。
3. [src/Modules/Option/reaction.cs](src/Modules/Option/reaction.cs) 已新增 `list` 與 `get_opts` route，直接透過 [src/conventions/HMVC/BaseReactionController.cs](src/conventions/HMVC/BaseReactionController.cs) 的 `ReactListAsync()` / `ReactGetOptionsAsync()` shared flow 承接，讓 `get` / `save` 之外再多兩個 caller 完成 shared Reaction proof。
4. 本輪已使用 [bin/check-web.sh](bin/check-web.sh) 完成 Docker-first compile 驗證；結果 build 通過，只有 [src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs) 與 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 兩個既有 warning，沒有新增 error。
5. 最新討論的下一步選項：
- 若要繼續擴充 `ReactionBase`，下一個最小 caller 應先落在 `del` 或 request validation rule wrapper，而不是 dynamic rerouter / upload。
- 若先做文件 closeout，則同步更新 [document/spec/OntoCms_net/check.md](document/spec/OntoCms_net/check.md) 並視需要整理 reference API drift。

### 第 4 輪討論結果
1. 本輪確認現行 history 寫法發生 FDD drift：舊版採 `Stage / Summary / Drift / Next Step` 的單份狀態報告，未依 [document/flow.md](document/flow.md#L282-L286) 要求使用逐輪承接格式，因此無法清楚表達哪一輪改變 stage、哪一輪辨識 drift、哪一輪調整 next step。
2. 因此本輪將 [document/spec/OntoCms_net/history.md](document/spec/OntoCms_net/history.md) 改寫為 append-only 的輪次格式，並以摘要輪方式承接既有進度；目前 stage 仍記為 `(done)`，但這裡指的是當前位於可執行的實作與驗證 stage，不代表 OntoCms_net feature-level 已完成。
3. 目前最新已落地的 slice 包含 `IReactionGetFeed<T>`、`IReactionListFeed<T>`、`IReactionOptionsFeed<T>`、`BaseReactionController` 的 contract-based shared flow、`OptionReaction` 的 contract proof、[bin/check-web.sh](bin/check-web.sh) 的日常 web compile check，以及 [conf/docker/Dockerfile](conf/docker/Dockerfile) publish copy 範圍收斂。
4. 目前主線尚未完成的部分仍是 ReactionBase 的下一個 shared caller，`list` 或 `get_opts` 應作為下一個最小 slice；後續若再有 spec sync，也必須以新增新一輪的方式承接，而不是回到單份總表模式。
5. 最新討論的下一步選項：
- 先做 `ReactionBase` 的 `list` 或 `get_opts` 第一個 entity proof，完成後新增下一輪 history。
- 若先做 spec closeout，則同步補齊 [document/spec/OntoCms_net/check.md](document/spec/OntoCms_net/check.md) 與本檔的新一輪承接，再視需要 commit。

### 第 3 輪討論結果
1. 本輪完成 HMVC shared reaction/feed contract 的第一個收斂：新增 [src/conventions/HMVC/IReactionFeedContracts.cs](src/conventions/HMVC/IReactionFeedContracts.cs)，定義 `IReactionGetFeed<T>`、`IReactionListFeed<T>`、`IReactionOptionsFeed<T>`。
2. [src/conventions/HMVC/BaseReactionController.cs](src/conventions/HMVC/BaseReactionController.cs) 已新增可直接吃 feed-side contract 的 shared flow；[src/Modules/Option/feed.cs](src/Modules/Option/feed.cs) 已實作 `IReactionGetFeed<OptionRecord>`，而 [src/Modules/Option/reaction.cs](src/Modules/Option/reaction.cs) 的 `get` / `save` 已改用 shared flow + feed contract，降低 entity reaction delegate wiring。
3. 本輪同步新增 [bin/check-web.sh](bin/check-web.sh) 與 [bin/docker-web-check-entrypoint.sh](bin/docker-web-check-entrypoint.sh) 作為 Docker-first 的日常 .NET web compile check，並收斂 [conf/docker/Dockerfile](conf/docker/Dockerfile) build-stage copy 範圍以縮小 publish cache invalidation。
4. [document/spec/OntoCms_net/plan.md](document/spec/OntoCms_net/plan.md) 已補回 reaction feed contract 規劃；[document/spec/OntoCms_net/history.md](document/spec/OntoCms_net/history.md) 與 [document/spec/OntoCms_net/check.md](document/spec/OntoCms_net/check.md) 也已辨識需要同步這批進度，避免 spec 與 runtime 再次漂移。
5. 最新討論的下一步選項：
- 以 `list` 或 `get_opts` 補第二個 contract-based reaction caller。
- 先把 spec sync 補齊，再決定下一個 Reaction shared slice。

### 第 2 輪討論結果
1. 本輪把 Dapper-first / SQL-first 與 `SqlKata` 僅限 read-side 的規則正式沉澱到 shared docs 與 base repository 實作；read-side compile / execute 細節收斂到 base-level 小介面，不讓 `SqlKata` 細節散落於 entity caller。
2. [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 已補上 `OneAsync()`、`LotsAsync()`、`LimitRowsAsync()` 等 read-side helper；[src/conventions/HMVC/BaseRelationRepository.cs](src/conventions/HMVC/BaseRelationRepository.cs) 已補上 `NewReadQuery()`、`CompileReadCommand()`、`ReadManyAsync()`，而 write-side `ReplaceSaveManyAsync()` 仍維持 Dapper + transaction。
3. 本輪同時修正 FORKS owner-boundary drift：[src/Modules/Post/relation.cs](src/Modules/Post/relation.cs) 已併回 [src/Modules/Post/feed.cs](src/Modules/Post/feed.cs) 的 owner-side private helper，不再保留 standalone relation file；Post tag relation 的 save / byTag / owner -> tag ids caller 與 Docker smoke 已完成驗證。
4. [src/conventions/HMVC/BaseOutfitController.cs](src/conventions/HMVC/BaseOutfitController.cs)、[src/conventions/HMVC/BaseReactionController.cs](src/conventions/HMVC/BaseReactionController.cs)、[src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs)、[src/conventions/HMVC/BaseSmoke.cs](src/conventions/HMVC/BaseSmoke.cs) 等 shared HMVC base 已建立最小邊界，但仍刻意排除 dynamic rerouter、upload、heavy export、account/login side effect 等 entity-owned 流程。
5. 最新討論的下一步選項：
- 先補 shared Reaction flow 的第一個 contract-based caller。
- 若 relation 要再推進，必須先找到真正 schema-backed 的 counter caller，而不是硬做 generic counter。

### 第 1 輪討論結果
1. 本輪完成 OntoCms_net 第一版 bootstrap baseline：建立 `.env`、[conf/docker/docker-compose.yml](conf/docker/docker-compose.yml)、`OntoCms_net.sln`、`src/public` web skeleton、`src/cli` CLI project、Docker build graph 與 SQL bootstrap 路徑，並以 Docker 作為 runtime source of truth。
2. `document/sql/init.sql` 與每日增量 SQL 已改寫為 MSSQL 版本，並在乾淨 SQL Server 2022 容器中完成 init / incremental 套用；web 與 db compose 路徑已驗證可啟動。
3. 前台最小 walking skeleton 已完成：首頁可 SSR 顯示 `tbl_option.page/title`，`api/option/get` 已可返回 option payload；後續首頁主內容再由 `Post` 承接 about 內容與多語系 path。
4. `Option` 已成為第一個完整 FORKS module proof，包含 [src/Modules/Option/feed.cs](src/Modules/Option/feed.cs)、[src/Modules/Option/reaction.cs](src/Modules/Option/reaction.cs)、[src/Modules/Option/outfit.cs](src/Modules/Option/outfit.cs)、[src/Modules/Option/kit.cs](src/Modules/Option/kit.cs)、[src/Modules/Option/smoke.cs](src/Modules/Option/smoke.cs)；後續 shared base 與 module-owned caller 都以這條 baseline 持續展開。
5. 最新討論的下一步選項：
- 沿著 FORKS/HMVC baseline 繼續補 shared base 與 entity caller。
- 先以最小可驗證 slice 推進 Feed / Relation / Reaction 的共通邊界，再逐步補 smoke 與 spec sync。
