# AI Augmentation Log: Agent Risk Audit and Rotation Baseline

- **Activity date:** 2026-11-05
- **Work item:** Audit the proposed agent setup against the lethal trifecta and establish available pre-rotation delivery baselines.
- **AI contribution:** Repository inspection, risk categorization, proposed least-privilege redesign, metric definitions, CI fixture aggregation, and drafting of the associated audit/baseline document.
- **Human approval:** Not recorded. This log does not assert that a person reviewed or approved the recommendations.
- **Model/version:** Not recorded by the available session context.
- **External data:** No external system was queried. Evidence below comes from files and Git history in this workspace.

## Evidence and generated claims

| Output claim or recommendation | Evidence consulted | AI operation | Validation / limitation |
|---|---|---|---|
| `ops/README.md` does not describe an agent configuration; it describes synthetic operations packs and ends with “No answers here.” | `ops/README.md`, especially its scenario note and final section | Inspected file content and searched for agent/tool setup references. | Confirmed by direct file read and `rg`; no agent identity, model, permissions, tool grants, or data flow were found. This is a repository-scope finding, not proof about an external runtime. |
| Sensitive-data leg is potential for live logs, but not evidenced as real in the fixtures. | `ops/README.md`; `ops/incidents/INC-2041/README.md`; `ops/incidents/INC-2047/README.md` | Separated the fictional sample data described in the README from production telemetry assumptions. | README states names, hosts, and figures are invented. Whether a future live source contains sensitive data requires owner classification. |
| Untrusted-input leg is potential; source provenance/sanitization are unspecified. | Incident evidence-pack descriptions in `ops/README.md` and `ops/incidents/INC-2047/README.md` | Identified that logs/incident evidence may include externally supplied values, without assuming an agent ingestion path exists. | No ingestion, trust-boundary, or sanitization configuration is present in the inspected repository material. |
| Agent ability to act externally is unknown; remove it from the initial design. | `ops/incidents/INC-2047/README.md` states evidence is read-only and actions require a human; no agent tool configuration was found. | Recommended a constrained initial workflow: approved redacted read-only evidence, no write/action connectors, engineer-mediated output. | This is a proposed control, not a verified property of a deployed agent. Must compare against the actual host/provider tool grants before use. |
| Capture lead time from production deployment time joined to deployed commit time. | `ops/pipeline/run-timings.csv`; Git history | Defined an event-based metric and a sample SQL-shaped query. | Not computable from available files: CI start times and Git commits do not identify successful production deployment SHAs/timestamps. |
| Capture change failure rate from production deployments linked to incidents/rollbacks. | `ops/pipeline/run-timings.csv`; `ops/incidents/` evidence descriptions | Defined a deployment-level numerator/denominator and sample query. | Not computable from the available repository. No production deployment catalog or deployment-to-failure links are recorded. CI failures are not production change failures. |
| Capture review turnaround from PR creation to first human review submission. | Git commit history and `ops/pipeline/run-timings.csv` | Specified GitHub PR/reviews timestamps and calculation. | Not computable from available files: neither source contains PR creation and review-submission events. |
| Available CI fixture has 20 runs, 18 successes, 2 failures; both failures are PR runs; main pushes are 0/8 failures. | `ops/pipeline/run-timings.csv` columns `run_number`, `event`, `conclusion` | Grouped rows by run number; counted one unique conclusion per run and split PR vs push. | Recomputed with PowerShell `Import-Csv`, `Group-Object run_number`, and `Group-Object Conclusion/Event`. Fixture-specific only; `ops/README.md` says its names, hosts, and figures are invented. Not a CFR. |
| Summed CI step runtime has median 1,875.5 s, mean 1,880.5 s, min 1,702 s, max 2,108 s. | `ops/pipeline/run-timings.csv` columns `run_number`, `duration_seconds` | Summed step durations per run; calculated distribution across the 20 run totals. | Recomputed with PowerShell `Import-Csv`, `Group-Object run_number`, `Measure-Object -Sum`, sorting totals and averaging middle two for the even-sized median. Summed step runtime is not necessarily elapsed wall-clock pipeline duration if steps overlap; not lead time or review turnaround. |

## Deliverable

Associated analysis: [agent-risk-audit-and-rotation-baseline.md](agent-risk-audit-and-rotation-baseline.md).

## Review and change record

- **Required human follow-up:** Confirm the actual AI host/provider, log data classification, retention/training terms, tool permissions, and audit destination before connecting production evidence.
- **Required baseline follow-up:** Obtain deployment events, incident/rollback-to-deployment links, and PR review events; calculate the three requested measures over a defined trailing window.
- **Approval / sign-off:** Pending; no reviewer name or approval event was supplied.
- **Correction policy:** If source evidence, event definitions, or calculations change, update the associated report and record the changed claim, evidence, and reviewer here.
