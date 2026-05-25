---
name: "FDD Backlog Add"
description: "Use after FDD Focus when adding requirements, scenarios, or SBE to the current spec's existing idea.md and deciding whether that addition forces discuss-stage resync or plan/check updates."
argument-hint: "Describe the requirement, scenario, or SBE to append to the current spec, plus the intended effect or concern"
agent: "agent"
---

Add a new requirement, scenario, or scenario-based example to the current Flow Driven Development spec after `FDD Focus`, then resync the affected idea/history/plan/check documents as needed.

Before doing any work, apply these rules:

1. Treat [document/flow.llm.md](../../document/flow.llm.md) as the primary low-token execution contract for Flow Driven Development.
2. Treat [document/flow.md](../../document/flow.md) as the full engineer-oriented reference when more detail or rationale is needed.
3. Treat [copilot-instructions.md](../copilot-instructions.md) as the workspace-level always-on ruleset.
4. Treat [document/spec/.current-spec.md](../../document/spec/.current-spec.md) as the single source of truth for the current target spec.
5. If [document/spec/.current-spec.md](../../document/spec/.current-spec.md) is missing, unreadable, or does not point to a valid spec folder, stop immediately and tell the user to run the appropriate `FDD Focus` command first.
6. This command is only for extending the current spec's existing requirement basis. If the requested addition actually belongs to a different feature or should become a new spec, stop and say so clearly instead of forcing it into the current `idea.md`.
7. Treat this command as an `idea` / `(discuss)`-side change-control entry point. Do not jump directly into implementation or `plan` changes unless the requirement addition clearly affects stage split, acceptance scope, or current next steps.
8. When the user adds a requirement, scenario, or SBE, require enough detail to make the addition meaningful. The command should not succeed on the command name alone; it needs the concrete appended requirement content.
9. For F3CMS architecture, terminology, process, and responsibility-boundary questions, treat files under [document](../../document) as the primary source of truth instead of making generic framework assumptions.

Required execution order:

1. First read [document/spec/.current-spec.md](../../document/spec/.current-spec.md).
2. Read the resolved target spec's `history.md` first.
3. Then read its `plan.md` and `check.md` to understand the current stage, latest completed slice, and current next step.
4. Read the resolved target spec's `idea.md` only after that, because this command exists to append to the current requirement basis rather than to restart the whole feature from scratch.
5. Decide whether the requested addition truly belongs to the current spec or should be split into a different spec.
6. If the addition belongs to the current spec, update `idea.md` with the new requirement, scenario, or SBE in a way that keeps the requirement basis coherent.
7. Add a new `history.md` round summarizing what was appended, why, and whether the change forces discuss-stage resync, plan/check follow-up, or a next-step change.
8. If the requirement addition changes implementation split, acceptance scope, or current next step, update `plan.md` and/or `check.md` accordingly.
9. If the addition invalidates the current stage, say so explicitly and record the stage rollback or discuss-stage resync in `history.md`.

Response expectations:

- Start by confirming which spec folder was resolved from [document/spec/.current-spec.md](../../document/spec/.current-spec.md).
- Then summarize the current stage and explain that this command will read `history.md`, `plan.md`, `check.md`, and then `idea.md` before making changes.
- Clearly say whether the user's addition appears to belong to the current spec or should become a new spec.
- If the request lacks the actual appended requirement content, stop and say that `FDD Backlog Add` needs the concrete requirement/SBE text instead of only the command name.
- After finishing, state which of `idea.md`, `history.md`, `plan.md`, and `check.md` were updated, and whether the current stage changed.

Backlog addition task:

{{input}}