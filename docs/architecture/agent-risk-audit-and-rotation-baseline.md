# Agent Risk Audit and Rotation Baseline

**Status:** Proposed controls; baseline captured from repository artifacts available 5 November 2026.

AI contribution and source traceability are recorded in [agent-risk-audit-augmentation-log.md](agent-risk-audit-augmentation-log.md).

## Risk audit: lethal trifecta

The requested setup is said to be at the end of `ops/README.md`, but that file ends with a “No answers here” section. It describes synthetic operational evidence packs and says they are not application runtime configuration; it contains no agent identity, model/provider, connected tools, permissions, or data-flow definition. Therefore this repository cannot establish which trifecta legs the deployed agent actually has. Do not treat this assessment as certification of a runtime setup.

| Leg | Assessment from available artifacts |
|---|---|
| **Access to sensitive/private data** | **Potentially present in the intended live workflow; not demonstrated as real data in these fixtures.** The operations material includes incident logs, metrics, and deployment evidence. `ops/README.md` explicitly says names, hosts, and figures are invented and no real customer or vendor appears. Production telemetry should be treated as sensitive unless classified otherwise. |
| **Untrusted input** | **Potentially present; provenance and sanitization are unspecified.** Logs and incident evidence may contain externally supplied values, but `ops/README.md` does not describe an agent ingestion path or validation controls. |
| **Ability to act or communicate externally** | **Unknown / not evidenced.** No agent setup or tool grants are specified. The INC-2047 README says its evidence is read-only and the decision/action belongs to a human, but that does not prove what tools a separate agent runtime has. |

### Risk decision and redesign

**Do not authorize a live agent connection based on this repository alone.** For the initial rotation, configure the agent to remove the action/egress leg:

- Give it read-only access only to approved, redacted incident evidence.
- Do not connect ticket-write, paging, email/chat-posting, shell, deployment, cloud-control-plane, payment, or arbitrary HTTP tools.
- Return summaries only to the assigned engineer in the approved internal review surface; the engineer decides whether to copy/publish or act.
- Use only an organization-approved model/data-processing path for live telemetry. If the provider or retention/data-use terms are unapproved, do not send production logs to it.
- Record the agent's identity, data sources, model/provider, tool grants, and audit destination before enabling it.

This keeps potentially sensitive evidence and untrusted log content from coexisting with autonomous action tools. The data and untrusted-input legs may still exist; the initial control is to remove independent action/communication. **Revisit before adding any write-capable tool, outbound integration, production credential, or unrestricted live-log source**, and after any provider/data-retention change. Re-audit and test least privilege, prompt-injection handling, output confinement, and approval/kill-switch behavior before enabling such a connector.

## Baseline metrics before rotation

Capture a trailing 90-day baseline before rotation start, and report event counts plus median and p85 where applicable. These are engineering delivery measures; do not infer them from CI job duration.

| Metric | Definition and exact source/query | Available now? |
|---|---|---|
| **Lead time for changes** | For each successful production deployment, `deployed_at - commit_committed_at` for the deployed SHA; report median/p85 and sample count over the trailing 90 days. Source: production deployment records joined to SCM commit SHA/timestamp (e.g. GitHub Deployments/Actions environment deployment plus Git commit metadata). Query: `SELECT percentile_cont(0.50) WITHIN GROUP (ORDER BY d.deployed_at - c.committed_at) AS median_lead_time, percentile_cont(0.85) WITHIN GROUP (ORDER BY d.deployed_at - c.committed_at) AS p85_lead_time, count(*) FROM production_deployments d JOIN commits c ON c.sha = d.commit_sha WHERE d.status = 'succeeded' AND d.environment = 'production' AND d.deployed_at >= :window_start;` | **No valid value.** Repository commit timestamps do not map to production deployment timestamps or deployed SHAs. `ops/pipeline/run-timings.csv` records CI start and step durations only. |
| **Change failure rate (CFR)** | `unique production deployments causing a rollback, hotfix, or incident within 7 days / all successful production deployments` in the same window. Source: deployment history joined to incident/rollback records by deployment ID/SHA. Query: `SELECT count(DISTINCT d.id) FILTER (WHERE EXISTS (SELECT 1 FROM change_failures f WHERE f.deployment_id = d.id AND f.occurred_at <= d.deployed_at + interval '7 days'))::decimal / NULLIF(count(DISTINCT d.id), 0) FROM production_deployments d WHERE d.status = 'succeeded' AND d.environment = 'production' AND d.deployed_at >= :window_start;` | **No valid value.** The CI file has run conclusions, not production deployment outcomes or links from incidents to deployments. Two failed CI runs cannot be counted as production change failures. |
| **Review turnaround** | For each PR, first human review submitted timestamp minus PR created timestamp; report median/p85, count, and unreviewed PRs separately. Source: GitHub Pull Requests and Reviews API (`created_at`, human `submitted_at`), excluding bot reviews; for example, `GET /repos/{owner}/{repo}/pulls?state=all` joined to `GET /repos/{owner}/{repo}/pulls/{number}/reviews`, then calculate `MIN(submitted_at) - created_at`. | **No valid value.** Git commit history and CI run start times contain neither PR creation nor review-submission timestamps. |

### Measurements captured from the repository today

`ops/README.md` describes the pipeline artifact as per-step timings for the last 20 runs and states the operational figures are invented. The following are therefore **fixture baselines only**, not live service measurements:

1. **CI run failure rate (not CFR): 2/20 = 10%.** In `ops/pipeline/run-timings.csv`, grouping rows by `run_number` and taking the run-level `conclusion` yields 18 successes and 2 failures. Both failures are pull-request runs (runs 6 and 14); main push runs are 0/8 failures. This measures CI executions, not deployed changes, and repeat runs/branches may not represent unique changes.
2. **CI pipeline duration: median 1,875.5 seconds (31m 15.5s); mean 1,880.5 seconds (31m 20.5s); range 1,702–2,108 seconds (28m 22s–35m 8s).** Query: group `ops/pipeline/run-timings.csv` by `run_number`, sum `duration_seconds` across each run's steps, then compute median/mean/min/max across the 20 totals. This is summed step runtime, not change lead time or review turnaround.

**Baseline gap:** None of the three requested measures (lead time, CFR, review turnaround) can be calculated validly from the checked-in repository and CI history. Capture them from deployment, incident, and PR-review systems before rotation starts. The two measured CI figures above are the only comparable operational measurements currently available; do not substitute or relabel them as DORA metrics.
