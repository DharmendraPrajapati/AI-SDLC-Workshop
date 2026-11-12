# Month-End AI Log Triage Autonomy Policy

**Purpose:** Speed month-end log triage without unreviewed production, evidence, or customer-data changes.

| Level | Permitted autonomy | Required controls |
|---|---|---|
| **1 — Read-only** | Search/summarize approved logs; cluster errors; suggest severity and affected component; cite records. No writes or production changes. | Read-only least-privilege access; approved sources; redact secrets/customer data; log query, sources, output and operator. Engineer verifies evidence. |
| **2 — Draft for review** | Draft incident summaries, severity, ownership, runbook steps, or ticket text; do not submit. | Level 1 controls; human verifies facts, severity, destination and redaction. Label drafts; no automatic posting, paging, suppression, or closure. Record reviewer. |
| **3 — Act with approval** | Execute only an exact, bounded, reversible action approved per instance (e.g., create/update ticket or run an allowlisted diagnostic). No remediation, access changes, suppression, deletion, or customer communication. | Prior controls; scoped credentials, allowlist, preview, explicit approval, audit trail, rate/time limits, tested undo. Stop on unexpected scope or result. |
| **4 — Act alone** | Perform only tested, low-risk, reversible allowlisted triage actions. No production remediation or destructive/external action under this policy. | Prior controls; isolated identity, enforced allowlist, independent monitoring, immutable audit, kill switch, tested rollback, impact bounds and escalation. Review access/outcomes periodically. |

## Promotion criteria

Promotion is task-specific; hold the current level until evidence is met and approval granted.

- **1 → 2:** **30** consecutive representative cases; **100%** source traceability; **zero** critical errors, missed severity-1 signals, or data disclosures; prompt, source allowlist and failure modes documented. Engineering lead approves.
- **2 → 3:** **50** reviewed drafts over **2** month-end cycles; **100%** human approval; **zero** unauthorized writes or severity-1/2 errors; **≥95%** material facts accepted without correction; allowlist/preview/audit/rollback/stop tests pass in non-production. Service owner and on-call lead approve.
- **3 → 4:** **100** approved executions of the exact action over **2** month-end cycles; **zero** unauthorized/out-of-scope/unrolled-back actions; **100%** audit completeness; fault tests pass for timeout, duplicate, wrong target, denied permission and kill switch; risk review and escalation/rollback documented. Engineering manager and service owner approve in writing. Limit autonomy to the tested allowlist.

Any safety incident, unexplained output, or material model/workflow change pauses promotion and downgrades the task pending review.

## Approval authority

- **Level 1:** Rotation engineer may operate approved read-only workflows.
- **Level 2:** Engineering lead approves promotion and draft destinations.
- **Level 3:** Service owner and on-call lead approve promotion and each production action.
- **Level 4:** Engineering manager and service owner approve promotion in writing. Any on-call engineer may kill or downgrade; the original authority must approve restoration.

## Day One Assessment

**Day-one ceiling: Level 1 — Read-only.** Six-person coverage does not replace task-specific validation; two engineers are new to this codebase and may not recognize misleading correlations or unsafe recommendations. Run AI only on approved, redacted logs with source citations; an engineer remains accountable for severity and response. Do not enable AI writes, paging, suppression, or production actions. Collect Level 1 promotion evidence before considering Level 2.
