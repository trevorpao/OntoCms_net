# OntoCms_net History

### 第 17 輪討論結果
1. 本輪直接承接第 16 輪鎖定的最小 next step，沒有擴成 generic rerouter，也沒有先跳去 authority filter；實作只聚焦在 `Staff` module 自己的 route-contract resync proof，讓 `/api/staff/{method}` 重新成為對外 contract。
2. [src/Modules/Staff/reaction.cs](src/Modules/Staff/reaction.cs) 現在已收斂為 module-owned dispatcher：對外用 `GET|HEAD|POST /api/staff/{method}` 承接 `login`、`logout`、`status`、`session`，其中 login 仍維持原本的 account/pwd -> bcrypt verify -> cookie sign-in 流程，但不再透過脫離 module 命名的 `/authenticate`；原本的 [src/Modules/Staff/authenticate.cs](src/Modules/Staff/authenticate.cs) 也已移除。
3. 這次 resync 仍維持第 16 輪裁定的 owner boundary：`ReactionBase` 沒有被擴成全站 generic rerouter，auth side effect、cookie sign-in/sign-out、status/session payload 與 method dispatch 仍都留在 `Staff` module 內。也就是說，這次 proof 關閉的是 route-contract drift，而不是把 rerouter magic 回推到 shared base。
4. [src/public/wwwroot/backend/index.html](src/public/wwwroot/backend/index.html) 的 backend login form 也已同步從 `/authenticate?loginredirect=` 改到 `/api/staff/login?loginredirect=`，避免靜態入口繼續指向舊 contract。
5. 本輪先以 [bin/check-web-compile.sh](bin/check-web-compile.sh) 做 focused compile 驗證，結果通過，仍只有 [src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs) 與 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 兩個既有 warning；接著再用 Docker runtime proof，以臨時 smoke staff 驗證 `POST /api/staff/login` 會簽發 `onto_staff` cookie，`GET /api/staff/status` 會回 `isLogin = 1` 與 user payload，`GET /api/staff/session` 會回 role/authority claims，`POST /api/staff/logout` 會清掉 cookie，最後 `GET /api/staff/status` 回 `isLogin = 0`，且 smoke staff 已從 DB 清理。
6. 最新討論的下一步選項：
- route-contract resync 已關閉；下一步回到第一個 authority filter / `[OntoAuthorize(...)]` caller proof，驗證 request-time authorization 直接消費目前 cookie session 上的 role-derived authority claims。

### 第 16 輪討論結果
1. 本輪不是新增另一條需求，而是對第 15 輪的 routing decision 做明確裁定：使用者已明示 OntoCms 的邏輯由各 module(Entity) 自行決定，`ReactionBase` 只負責建立後台 CRUD 慣例 `{method}`，相鄰 middleware 也可放在各 module 中；因此 current spec 現在正式決定保留 rerouter magic。
2. 這個決定修正了第 15 輪中「不要回到 PHP 式無限制 rerouter magic」的方向。新的收斂點不是完全放棄 rerouter，而是把它明確界定成 module-owned routing mechanism：對外仍維持 `/api/{module}/{method}`，新增 API method 不需回頭修改集中 route 表；但 auth policy、validation、login/session side effect 與相鄰 middleware 仍由各 module 自行擁有，不上推到 generic `ReactionBase`。
3. 因此本輪已同步更新 [document/spec/OntoCms_net/idea.md](document/spec/OntoCms_net/idea.md)、[document/spec/OntoCms_net/plan.md](document/spec/OntoCms_net/plan.md) 與 [document/spec/OntoCms_net/check.md](document/spec/OntoCms_net/check.md)：`idea.md` 現在把 rerouter magic 記為已決議的架構規則；`plan.md` 改為由保留的 module-owned rerouter 承接 `/api/{module}/{method}`；`check.md` 則把 current next step 收斂為依這個新決策完成 `Staff` route-contract resync。
4. 這代表 current spec 沒有回到 generic `ReactionBase` 決定一切的方向，反而更明確地把責任分兩層：shared `ReactionBase` 只保留 CRUD 慣例與 helper，module reaction 自行擁有 rerouter/middleware/business rule。真正待關閉的 drift 也因此更具體：目前 [src/Modules/Staff/authenticate.cs](src/Modules/Staff/authenticate.cs) 還是 `/authenticate`，尚未依這個決策落成 `/api/staff/login`。
5. 最新討論的下一步選項：
- 先依新的 rerouter 決策做 `Staff` route-contract resync proof，把 login 對外入口改為 `/api/staff/login`，並補齊 logout/status 等 method 的同一路徑規則。
- route-contract resync 關閉後，再回到 authority filter / `[OntoAuthorize(...)]` 第一個 caller proof。

### 第 15 輪討論結果
1. 本輪使用 `FDD Backlog Add` 追加的是 `Staff` reaction API 的外部 route contract，而不是新的獨立 feature；因此這項需求仍屬於 [document/spec/OntoCms_net/idea.md](document/spec/OntoCms_net/idea.md) 的同一份 OntoCms 核心框架 spec，範圍落在 Stage 2 的 `Reaction & Auth` 主線。
2. 本輪明確參考了 PHP 的 [docker-f3cms/www/f3cms/modules/Staff/reaction.php](docker-f3cms/www/f3cms/modules/Staff/reaction.php) 與 [docker-f3cms/www/f3cms/libs/Reaction.php](docker-f3cms/www/f3cms/libs/Reaction.php)：在舊 F3CMS 中，`rStaff::do_login()` 對外是透過 `GET|HEAD|POST /api/@module/@method = \F3CMS\Reaction->do_rerouter` 呈現成 `/api/staff/login`，而新增 API method 時不需再另外修改一份舊式集中 route 設定。
3. 這次 backlog 追加改變的不是 claims mapping 或 login business rule，而是 `Staff` reaction 的外部命名契約與 route-extension 規則。因此本輪已把 requirement 回寫到 [document/spec/OntoCms_net/idea.md](document/spec/OntoCms_net/idea.md)：`Staff` 的 reaction API 應對齊舊 F3CMS 的模組/方法 contract，至少以 `login` 為例，對外入口應是 `/api/staff/login`，而不是脫離 module 慣例的 `/authenticate`。
4. 本輪也同步更新 [document/spec/OntoCms_net/plan.md](document/spec/OntoCms_net/plan.md) 與 [document/spec/OntoCms_net/check.md](document/spec/OntoCms_net/check.md)，因為這次 requirement 追加已直接影響 current next step：既有 Stage 2 原本準備前進到 authority filter proof，但目前程式存在明確 drift，亦即 [src/Modules/Staff/authenticate.cs](src/Modules/Staff/authenticate.cs) 仍使用 `/authenticate`，尚未對齊新的 `/api/staff/login` contract。
5. 這次追加也迫使 current spec 進行 discuss-side resync：先前文件已決定不把 PHP `Reaction.php::do_rerouter()` 的無限制 dynamic reflection 整包搬進 `.NET` shared layer，但本輪又新增「外部 `/api/{module}/{method}` contract 必須保留、且新增 method 不需回頭修改集中 route 表」的 requirement。這代表下一步要先收斂的是 `.NET` 端如何承接這個 route contract，而不是直接往 `[OntoAuthorize(...)]` proof 推進。
6. 最新討論的下一步選項：
- 先做 `Staff` route-contract resync proof，把 login 對外入口改為 `/api/staff/login`，並決定 `.NET` 端要用哪種可維護 routing 策略承接 `/api/{module}/{method}`，同時避免回到 PHP 式無限制 rerouter magic。
- 在 route-contract resync 關閉後，再回到原本延後的 authority filter / `[OntoAuthorize(...)]` 第一個 caller proof。

### 第 14 輪討論結果
1. 本輪承接第 13 輪留下的 next step，將 `AuthenticationHandler` 的 header-based claims proof 收斂成真正可持有憑證的最小 staff login path，而不是繼續把 `X-Onto-Staff-Id` 當成長期入口。範圍仍刻意維持在最薄的登入/會話閉環：帳密驗證、cookie 簽入、session claims 回讀。
2. [src/Modules/Staff/authenticate.cs](src/Modules/Staff/authenticate.cs) 現在已新增 `POST /authenticate`，會從 form 讀取 `Account` / `Pwd`，透過 [src/Modules/Staff/feed.cs](src/Modules/Staff/feed.cs) 依 account 讀取 `tbl_staff + tbl_role`，再用 bcrypt 驗證密碼後簽發 cookie；[src/conventions/Auth/StaffClaimsPrincipalFactory.cs](src/conventions/Auth/StaffClaimsPrincipalFactory.cs) 也被抽出來供登入與既有 header proof 共用，避免 claims 組裝在兩個入口各自維護一份。
3. [src/public/Program.cs](src/public/Program.cs) 已補上 `StaffCookie` scheme 的 cookie auth registration，並把 API login/access denied redirect 改成直接回 `401/403`；[src/Modules/Staff/reaction.cs](src/Modules/Staff/reaction.cs) 的 `GET /api/staff/session` 也已改成走 cookie scheme，讓 session proof 真正驗到正式登入後的 claims，而不是只驗 header scheme。
4. 本輪先用 [bin/check-web-compile.sh](bin/check-web-compile.sh) 做 focused compile 驗證；中途只修一個局部 defect： [src/Modules/Staff/authenticate.cs](src/Modules/Staff/authenticate.cs) 的 redirect 判斷用了錯的 `StartsWith` overload，修正後 compile 通過，仍只有 [src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs) 與 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 兩個既有 warning。
5. 本輪接著做 Docker runtime proof。先確認 repo 靜態 backend page 裡預填的 `1234` 並不是目前 seed hash 的真實密碼，因此沒有把 `trevor/1234` 的 wrong-data 結果誤判成程式缺陷；之後依 `.env` 指向的 `OntoCms` 資料庫，臨時插入一筆已知 bcrypt 密碼的 `Verified` smoke staff，實際驗證 `POST /authenticate` 會回 `code = 1`、`Set-Cookie: onto_staff=...` 與 redirect 資訊，接著 `GET /api/staff/session` 會回 `auth_smoke_cookie / Administrator / role_id = 1` 與五個 authorities，最後再把該 smoke staff 從 DB 刪除。這代表 cookie-based staff login path 已完成最小 end-to-end proof，且 claims 仍直接來自 `Role` mapping。
6. 最新討論的下一步選項：
- 先補第一個 authority filter / `[OntoAuthorize(...)]` proof，驗證 request-time authorization 直接消費目前 cookie session 上的 authority claims。
- 若要先補 session 相鄰 closeout，下一個最小 slice 可補 `logout` 或 staff login 的 footmark / last-login 類 owner-side side effect，但不應回頭把 claims mapping 拉出 `Role` owner boundary。

### 第 13 輪討論結果
1. 本輪承接第 12 輪已明確鎖定的 next step，直接實作 `AuthenticationHandler` 的最小 consumption proof，而不是提前做完整 staff login / session。重點不是密碼流程，而是驗證 claims 來源是否確實收斂到 `Role` entity 已提供的 authority mapping。
2. [src/Modules/Staff/feed.cs](src/Modules/Staff/feed.cs) 現在已補上最薄的 `StaffFeed` read-side owner boundary，負責從 `dbo.tbl_staff` 與 `dbo.tbl_role` 讀出 staff 狀態、role title、role priv；[src/conventions/Auth/StaffAuthenticationHandler.cs](src/conventions/Auth/StaffAuthenticationHandler.cs) 則以自訂 `StaffHeader` scheme 讀取 `X-Onto-Staff-Id`，只做最小驗證後直接消費 [src/Modules/Role/feed.cs](src/Modules/Role/feed.cs) 的 authority helper，把 role bitmask 轉成 claims。
3. [src/public/Program.cs](src/public/Program.cs) 已補上 authentication service registration 與 `UseAuthentication()`；[src/Modules/Staff/reaction.cs](src/Modules/Staff/reaction.cs) 也新增受保護的 `GET /api/staff/session` proof route，用來把 handler 實際產出的 claims 回出來，避免只停留在 compile-level wiring。
4. 本輪先用 [bin/check-web-compile.sh](bin/check-web-compile.sh) 做 focused compile 驗證；唯一局部 defect 是 [src/public/Program.cs](src/public/Program.cs) 在 top-level registration 少了 `AuthenticationSchemeOptions` 的完整型別名稱，修正後 build 通過，仍只有 [src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs) 與 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 兩個既有 warning。
5. 本輪接著用 `docker compose --env-file .env -f conf/docker/docker-compose.yml build web && up -d web` 做 runtime proof，再以 `curl -k` 驗證 `GET /api/staff/session`：未帶 `X-Onto-Staff-Id` 時回 `401`；帶 `X-Onto-Staff-Id: 1` 時回 `trevor / Administrator / role_id = 1` 與 `base.cms`、`mgr.cms`、`base.member`、`mgr.member`、`mgr.site` 五個 authorities。這表示 `AuthenticationHandler` 已確實消費 `Role` mapping，而不是 handler 自己維護另一套 permission table。
6. 最新討論的下一步選項：
- 先把這個 header-based proof 往真正 staff login path 收斂，例如補帳號/密碼驗證、session 或 cookie 憑證承載，避免 `X-Onto-Staff-Id` 停留為長期入口。
- 若要往 P1 backlog 主線前進，下一個 executable slice 可開始拆 staff login credential flow 或第一個需要權限攔截的內容管理 caller，但都應繼續直接消費 `Role` entity 的 authority mapping。

### 第 12 輪討論結果
1. 本輪使用 `FDD Backlog Add` 把「完整 OntoCMS 應具備的功能清單」正式追加回 current spec，但沒有把所有列出的能力都視為同一波必做項，而是先收斂第一優先序功能，避免 backlog 一次擴成無法執行的超大範圍。
2. 本輪判定這個需求追加仍屬於 [document/spec/OntoCms_net/idea.md](document/spec/OntoCms_net/idea.md) 的同一份 OntoCMS 核心框架 spec，因為它補的是整體功能優先序，而不是另一個獨立 feature。對當前 spec 而言，第一優先序應先包含四塊：A 的核心內容發布、B 的核心導覽整理、C 的核心素材/meta、D 的核心後台治理。
3. 具體來說，本輪已把第一優先序寫成：單頁/文章/新聞/公告/專案的共通內容發布基線、多語系、封面、版型、上線時間與狀態控管；分類、標籤、選單階層；媒體/圖片素材與 meta；以及 staff / role / login / system option。這些能力共同構成「可運作 CMS 核心」的最小閉環。
4. 同時，本輪也明確把下列項目排除在當前第一優先序之外：搜尋、作者/書籍/術語等獨立內容關聯對象、聯絡表單、訂閱資料、廣告管理、追蹤紀錄與其他營運輔助資料。這些不會消失，但會留在後續 backlog，而不是現在就壓進同一個 executable plan。
5. 因此本輪已同步更新 [document/spec/OntoCms_net/idea.md](document/spec/OntoCms_net/idea.md)、[document/spec/OntoCms_net/plan.md](document/spec/OntoCms_net/plan.md) 與 [document/spec/OntoCms_net/check.md](document/spec/OntoCms_net/check.md)：`idea.md` 明確寫下 Priority-1 功能基線，`plan.md` 補上 Priority-1 feature baseline 與各 stage 對應範圍，`check.md` 則新增一條 open item，提醒後續要把這份 P1 backlog 繼續拆成更小的 executable slices。
6. 這次 backlog 追加改變了當前 spec 的功能優先序描述，但沒有推翻目前已在進行的 owner-boundary 主線；就 immediate next step 而言，仍可維持先補 `AuthenticationHandler` consumption proof，因為它屬於 P1-D 後台治理核心的一部分。

### 第 11 輪討論結果
1. 本輪承接第 10 輪已明確收斂的 next step，直接落 `Role` entity baseline 的第一個 `.NET` slice，而不是跳去 `AuthenticationHandler`。範圍維持在最小 read-side proof：先補 `RoleFeed` / `RoleReaction` / `RoleOutfit`，只處理 role 讀取、權限映射與 list / option 呈現，不碰 login side effect。
2. [src/Modules/Role/feed.cs](src/Modules/Role/feed.cs) 現在已成為 `Role` owner-side baseline：它承接 `tbl_role` 的 `get` / `list` / `get_opts` 讀取，並明確擁有 `priv` bitmask 對應的 authority mapping helper，包括 authority option、authority title/name 展開、`hasAuth()` 與 authority value parse；這一層刻意讓後續 staff login 只能消費 `Role` 的 mapping，而不能在 handler 內重做一套規則。
3. [src/Modules/Role/reaction.cs](src/Modules/Role/reaction.cs) 已新增 `get` / `list` / `get_opts` / `get_auth_opts` route，並用 entity-owned transform 對齊 PHP `rRole` 的責任：`list` 會把 `priv` 轉成 title 串接結果，`get` 會把 `priv` 轉成 checkbox 可消費的 authority values 與 authority names。 [src/Modules/Role/outfit.cs](src/Modules/Role/outfit.cs) 也已補上最薄 owner boundary，避免 `Role` 仍是空 module。
4. 本輪先用 [bin/check-web-compile.sh](bin/check-web-compile.sh) 做 Docker-first compile 驗證；中途只修到一個局部 defect：`RoleReaction` 不能直接在 generic `TRow` 上做 record `with`，以及 `RoleRecord` 需要改成可供 Dapper parameterless materialization 的形狀。修正後 compile 已通過，仍只有 [src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs) 與 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 兩個既有 warning。
5. 本輪接著用 `docker compose --env-file .env -f conf/docker/docker-compose.yml build web && up -d web` 做 runtime proof；實際 `curl` 結果如下：`GET /api/role/list` 會回兩筆 seed role，且 `priv` 已轉為權限標題字串；`GET /api/role/get?id=1` 會回 `auth: [1,2,4,8,16]` 與對應 authority names；`GET /api/role/get_opts` 會回可供 staff role selector 使用的 role option；`GET /api/role/get_auth_opts` 會回五個 authority option。這代表 `Role` 已可作為 staff login claims mapping 的 source of truth。
6. 最新討論的下一步選項：
- 先補最小 `AuthenticationHandler` proof，驗證 staff login 產生的 claims 直接消費 `RoleFeed` 的 authority mapping，而不是 handler 私有常數表。
- 若要繼續擴 `Role` entity 本身，下一個最小 slice 可補 save path，但仍要避免把 login/session side effect 混進 `Role` module。

### 第 10 輪討論結果
1. 本輪第一次實際使用 `FDD Backlog Add` 承接 current spec 的需求追加，主題是把「staff 登入的基本角色權限對應」正式納入 [document/spec/OntoCms_net/idea.md](document/spec/OntoCms_net/idea.md)，而不是只停留在前一輪對 `Role` baseline 的 next-step 提醒。
2. 這次追加內容仍屬於 current spec 的 Stage 2 主線，沒有跨出 OntoCms_net，也不是新 spec；因此本輪沒有另開新 spec，而是把 requirement 明確寫成：`Staff` login 必須以前置 `Role` entity baseline 提供的 `priv` mapping、authority option 與 `hasAuth` 類 helper 作為權限真實來源，不能在 `AuthenticationHandler` 內再造一套 permission 規則。
3. 本輪參考 PHP 的 `docker-f3cms/www/f3cms/modules/Role` FORKS，確認 `fRole` 的 owner boundary 包含 `getAuth()`、`getAuthOpts()`、`hasAuth()`、parse / reverse 與 `_handleColumn()` 對 `priv` 的處理；因此 `.NET` 版 backlog 追加也對齊這個方向，把 staff login 明確降格為 consumer，而不是 role rule 的擁有者。
4. 因此本輪已同步更新 [document/spec/OntoCms_net/idea.md](document/spec/OntoCms_net/idea.md)、[document/spec/OntoCms_net/plan.md](document/spec/OntoCms_net/plan.md) 與 [document/spec/OntoCms_net/check.md](document/spec/OntoCms_net/check.md)：`idea.md` 新增 staff login 角色權限對應 scenario，`plan.md` 的 Stage 2 現在明確要求 `Role` baseline 先提供可供 login 消費的 authority mapping，`check.md` 也補上「不得在 handler 內重做 role/permission 對照」這個 open item。
5. 這次 backlog 追加沒有推翻目前 stage；current spec 仍可維持 `(done)`，但 Stage 2 的 next step 已更明確地收斂為「先落 `Role` entity baseline，再接 staff login claims mapping」。
6. 最新討論的下一步選項：
- 先以 `RoleFeed` + `RoleReaction` 落第一個 read-side baseline，包含權限 option 與 list 呈現，關閉 `Role` entity 的 owner boundary。
- 再補最小 `AuthenticationHandler` proof，驗證 staff login 產生的 claims 直接來自 `Role` mapping，而不是 handler 私有常數表。

### 第 9 輪討論結果
1. 本輪承接前一輪對 FDD 指令語意的討論，進一步確認需要一個獨立入口來處理「在 `FDD Focus` 之後，對 current spec 的既有 `idea.md` 追加 requirement / scenario / SBE」這種情境；這不是新 spec 初始化，也不應混成一般 `FDD Sprint`。
2. 因此本輪已新增 [.github/prompts/fdd-backlog-add.prompt.md](.github/prompts/fdd-backlog-add.prompt.md)，正式定義 `FDD Backlog Add` 指令。它的責任不是直接寫 code，而是把追加內容正式納入 current spec 的 `idea.md`，並判斷是否需要回到 `(discuss)`、同步 `plan.md` / `check.md`，或改變目前 next step。
3. 本輪同步更新 [document/flow.md](document/flow.md) 與 [document/flow.llm.md](document/flow.llm.md)，把 `FDD Backlog Add` 加入 command 對應，並明確寫下限制：這個指令必須帶具體追加內容，不能只送指令名稱；若追加內容其實已跨出 current spec，應停止並改走新 spec 流程。
4. 這代表目前 FDD command set 已從單純的 `Focus / Sprint / Review / Refactor / Retrospective`，補上了既有 spec 在 `idea` / `(discuss)` 側的需求變更控制入口；後續若要追加需求，不應只留在聊天室或 `history.md`，而應使用 `FDD Backlog Add` 正式回寫 `idea.md`。
5. 最新討論的下一步選項：
- 若要驗證這個新指令的實際工作流，下一步可用某個現有 spec 做一次小型 requirement / SBE append 演練。
- 若要繼續 current spec 的 HMVC / Role / auth 主線，則下一步可回到 `Role` entity baseline 的第一個 `.NET` slice。

### 第 8 輪討論結果
1. 本輪承接第 7 輪之後的兩個明確問題。第一，現有 [bin/check-web.sh](bin/check-web.sh) 雖然一直被當成日常 web compile check 使用，但它實際上是透過 compose `cli` service 執行 `dotnet build`，不是 `web` image rebuild；名稱容易誤導為「檢查 web runtime」。
2. 因此本輪已將 compose `cli` 內的增量 compile 檢查正式抽成 [bin/check-web-compile.sh](bin/check-web-compile.sh) 作為 canonical 命名，並把 [bin/check-web.sh](bin/check-web.sh) 保留為 compatibility wrapper；[.github/copilot-instructions.md](.github/copilot-instructions.md) 也已同步改為優先使用新名稱。
3. 第二，登入 / `AuthenticationHandler` 的下一步不應直接從 `tbl_staff` 與 claims abstraction 開始，而應先參考 PHP 的 `docker-f3cms/www/f3cms/modules/Role` FORKS 建立 `Role` entity baseline。PHP `Role` module 的 owner 邊界很清楚：`fRole` 承接 `priv` bitmask、auth option 與 `_handleColumn`，`rRole` 承接 list / handleRow 的呈現轉換，`oRole` 仍保持極薄。
4. 因此本輪已把這個前置順序寫回 [document/spec/OntoCms_net/plan.md](document/spec/OntoCms_net/plan.md)：Stage 2 現在先有 `Task 2.0: Role Entity Baseline 先行`，再進入 `AuthenticationHandler` / claims；[document/spec/OntoCms_net/check.md](document/spec/OntoCms_net/check.md) 也同步標記目前 repo 尚無 `Role` module，不應先跳去 login / session / claims 的 owner-side 實作。
5. 最新討論的下一步選項：
- 先把 `Role` entity 的第一個 `.NET` slice 落成 `RoleFeed` + `RoleReaction` 的 read-side baseline，對齊 PHP `fRole` / `rRole` 的 owner boundary。
- 若只處理工具鏈 closeout，則下一步應先驗證 `bin/check-web-compile.sh` / `bin/check-web.sh` wrapper 行為一致，並把這輪 rename drift 收斂完成。

### 第 7 輪討論結果
1. 本輪承接第 6 輪留下的 delete path 判斷，先確認 `ReactionBase` 若要補 `del`，前提必須是 feed-side 先提供 delete contract；因此本輪沒有讓 reaction 直接碰 SQL，而是先在 [src/conventions/HMVC/IReactionFeedContracts.cs](src/conventions/HMVC/IReactionFeedContracts.cs) 補上 `IReactionDeleteFeed`，並在 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 補上最小單表 `DeleteMainRowAsync()` helper。
2. [src/conventions/HMVC/BaseReactionController.cs](src/conventions/HMVC/BaseReactionController.cs) 現在已補上 `ReactDeleteAsync()` shared flow；[src/Modules/Option/feed.cs](src/Modules/Option/feed.cs) 已實作 `IReactionDeleteFeed`，而 [src/Modules/Option/reaction.cs](src/Modules/Option/reaction.cs) 已新增 `del` route，讓 `Option` 成為第一個 delete contract proof。
3. 本輪 runtime 驗證中途抓到一個相鄰 defect： [src/Modules/Option/feed.cs](src/Modules/Option/feed.cs) 的 SqlKata table 名稱使用 `"[dbo].[tbl_option]"`，在 rebuilt `web` runtime 下會造成 `Invalid object name '[dbo].[tbl_option]'`；因此本輪已同步修正為 `"dbo.tbl_option"`，避免 `get` / `list` / `get_opts` / `GetSiteTitleAsync()` 在實際 web image 中使用無效 table name。
4. 本輪已先用 [bin/check-web.sh](bin/check-web.sh) 完成 compile 驗證，之後再用 `docker compose ... build web && up -d web` 做 runtime proof：先 `POST /api/option/save` 建立臨時 option，再 `POST /api/option/del` 刪除，最後 `GET /api/option/get?id=<created_id>` 已回 `{"code":8004,"data":[],"csrf":""}`；另外依 `.env` 內的 DB 連線資訊直接查 `dbo.tbl_option`，也已確認刪除後 `COUNT(1) = 0`。
5. 最新討論的下一步選項：
- 若要繼續擴充 `ReactionBase`，下一個最小 slice 可回到 delete 相鄰的 rule wrapper，例如 `del` rule group，但仍維持 feed-side contract + shared flow 的 owner boundary。
- 若要離開 Reaction/Kit 線，則下一個較自然的 shared slice可改往 OutfitBase 或 SmokeBase 的第一個 caller proof。

### 第 6 輪討論結果
1. 本輪承接第 5 輪留下的最小 Reaction / Kit 相鄰 slice，先確認 `del` 目前仍缺少對應的 feed contract 與共通 delete helper，因此若硬做 `ReactDelAsync()` 會跨到新的 owner-boundary 設計，不符合最小下一步原則。
2. 因此本輪改走更窄的 shared validation rule wrapper： [src/conventions/HMVC/BaseReactionController.cs](src/conventions/HMVC/BaseReactionController.cs) 現在已補上可吃 kit rule group 的 `ReactSaveAsync()` overload，先承接 `required` / `integer` 這類 save caller 足夠的最小規則，並在 unsupported rule 出現時 fail-closed 回傳 `Unverified`。
3. [src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs) 已補上 `SaveRuleGroupName`，而 [src/Modules/Option/kit.cs](src/Modules/Option/kit.cs) 現在以 `Group` / `Loader` / `Status` / `Name` / `Content` 的 required 規則作為第一個 entity proof；[src/Modules/Option/reaction.cs](src/Modules/Option/reaction.cs) 的 `save` route 已改為透過 shared validation wrapper + `OptionKit.Rule` 承接。
4. 本輪中途抓到一個實際 runtime 差異：由於 [src/Modules/Option/feed.cs](src/Modules/Option/feed.cs) 的 `WriteModel` 原本使用 non-nullable string，`[ApiController]` 會先回 ASP.NET Core 的自動 400，導致 shared wrapper 沒有機會接管 missing-field path；因此本輪已將 `WriteModel` 的字串輸入欄位改為 nullable，讓 save missing-field 由 shared wrapper 回傳 F3CMS 風格 envelope。
5. 本輪已使用 [bin/check-web.sh](bin/check-web.sh) 完成 Docker-first compile 驗證，並用 `docker compose ... build web && up -d web` 後實際呼叫 `POST /api/option/save` 驗證 runtime 行為；結果現在會正確回傳 `{"code":8004,"data":{"fields":["Group","Loader","Status","Name","Content"]},"csrf":""}`。compile 與 runtime 驗證都通過，只有 [src/conventions/HMVC/BaseKit.cs](src/conventions/HMVC/BaseKit.cs) 與 [src/conventions/HMVC/BaseFeedRepository.cs](src/conventions/HMVC/BaseFeedRepository.cs) 兩個既有 warning。
6. 最新討論的下一步選項：
- 若要繼續擴充 `ReactionBase`，下一個最小 caller 可回到 `del`，但前提是先補對應的 feed-side delete contract，而不是讓 reaction 直接碰 SQL。
- 若要繼續擴充 `KitBase`，下一個最小 caller 可補 login/save 另一組 rule wrapper，但仍不碰 session/mail side effect。

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
