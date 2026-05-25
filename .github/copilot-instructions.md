# Project Guidelines

## Build, Test, and Validation
- Prefer the project's existing Docker environment for validation, smoke scripts, PHP execution, workflow verification, and post-change runtime checks.
- Use existing `docker compose` services and container paths as the default baseline.
- For day-to-day .NET web compile checks, prefer `bin/check-web-compile.sh` first. It runs an incremental Docker-based `dotnet build` through the compose `cli` service and is cheaper than rebuilding the web image. `bin/check-web.sh` remains a compatibility wrapper so older docs or habits do not break.
- Reserve `docker compose --env-file .env -f conf/docker/docker-compose.yml build web` for image/publish validation, Dockerfile changes, or final runtime packaging checks.
- Only fall back to local PHP when Docker is unavailable or explicitly required.

## Source of Truth and FDD
- Treat Docker as the runtime source of truth when host and container results differ.
- Treat `.env` as the database source of truth; do not guess or replace provided credentials.
- Treat `document/` as the source of truth for F3CMS architecture, terminology, and process.
- For FDD work, use `document/flow.md` as the full source and `document/flow.llm.md` as the low-token summary.
- For `document/spec/<feature>/`, read `history.md` first, then `plan.md` and `check.md`; only return to `idea.md` if earlier stages are incomplete or invalidated.
- For `idea.md`, prefer example/scenario-driven convergence over abstract prose. Require at least one mainline scenario; add a boundary or counter-example when scope edges matter.

## FORK Maintenance Priority
- Treat FORK maintainability as a repo-level decision rule, not as an optional preference.
- When convenience conflicts with future upstream sync, diff readability, owner-boundary clarity, or rollback safety, choose the option that keeps the FORK easier to maintain.
- Use `document/guides/` as the source of truth for this judgment. In particular:
	1. `document/guides/fdd_porting_guide.md` for the rule that workspace-level instructions must preserve stable LLM behavior across future handoffs.
	2. `document/guides/llm_dba_guide.md` for ownership-first reasoning and the rule that convenience-oriented shortcuts must not override long-term structure.
	3. `document/guides/sd_conventions.md` for module boundary rules, especially that entity-specific logic stays module-owned and should not be pushed into generic helpers or `libs` just because it is convenient.
- Prefer module-owned boundaries, stable interfaces, and explicit data flow over project-specific shortcuts that increase coupling.
- Avoid introducing or expanding dependencies on project-specific global helpers such as `mh()` or similar convenience entry points when a lower-coupling owner-side path exists.
- For module `Kit` / `Feed` / `Reaction` decisions, preserve owner boundaries first; do not optimize for the shortest local wiring if that makes later FORK maintenance harder.
- If a proposed change is locally convenient but creates extra FORK-only coupling, stop and either choose the lower-coupling design or explicitly tell the user about the tradeoff before implementing.
- Keep relation helpers module-owned and owner-file-first; do not introduce standalone `relation.cs` module files unless the upstream/FORKS source already has a matching owner boundary to justify it.

## Data Access Strategy
- Treat Feed / relation data access as Dapper-first and SQL-first by default.
- Keep SQL as a first-class artifact; do not hide ownership-heavy save flows behind tracking ORM patterns.
- `SqlKata` is allowed only as a thin query-builder second option, mainly for read-side where / order / paging composition and SQL + bindings compilation.
- Do not let `SqlKata` or any query-builder take over save / upsert / `_lang` / `_meta` / relation orchestration.
- Feed owners must keep explicit control of CRUD, transaction boundaries, and write ordering.
- Avoid introducing Entity Framework-style model tracking or abstractions that blur owner boundaries.

### Data Access Checklist
- Before adding any helper or abstraction, ask whether the SQL can stay explicit in the owner Feed / relation path.
- Use raw Dapper first for save flows, owner-controlled CRUD, `_lang`, `_meta`, relation writes, and transaction-scoped orchestration.
- Reach for `SqlKata` only when it makes a read-side query clearer through where / order / paging composition or SQL + bindings compilation.
- If a design would make `SqlKata` decide write ordering, transaction boundaries, or relation ownership, reject it and keep that logic in the owner module.
- When relation behavior is owner-specific, prefer a private/nested owner-side helper or owner-file implementation over a new standalone relation module file.
- When `SqlKata` is used on the read-side, keep query compile / execute details behind a small owner-side or base-level interface instead of scattering them across feed / relation callers.


Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

These guidelines bias toward caution over speed. Use judgment for trivial tasks.

## 1. Think Before Coding

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them. Do not pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

Before editing project documents:
- Preserve artifact ownership: `idea.md` for requirement basis, `plan.md` for executable breakdown, `check.md` for verification status, `history.md` for stage/progress/next-step continuity.
- Do not move progress notes or temporary next steps into `idea.md`.

## 2. Simplicity First

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

For documentation changes:
- Prefer the smallest wording change that makes the rule clearer or more enforceable.
- Do not expand a low-token summary into a second full manual unless the user explicitly wants that.

## 3. Surgical Changes

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

For documentation alignment work:
- Calibrate summary documents against their full source instead of rewriting both from scratch.
- If a rule belongs only in the full explanation layer, keep it there rather than copying it verbatim into summary files.

## 4. Goal-Driven Execution

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

For FDD and spec work, define success in document terms as well:
- stage is correctly identified
- source-of-truth files are read in the right order
- drift is explicitly classified before editing
- updated documents remain aligned after the change
