# Review Prompt Evaluation Report

## Result validity

This is an **in-session desk-scored replay**, not a valid blinded model benchmark or a harness-driven model run. The prescribed method requires a fresh session per case. Here, all fixtures, source code, and likely labels were visible in one conversation; the model/version identifier and raw independent model responses were unavailable. The pass rows below record repeated scoring judgments in this context, not three separate model executions. Do not use these numbers to claim production reviewer accuracy or to promote review automation. For a valid evaluation, run every case in a fresh session with only that diff and the specified prompt, record the actual model identifier and raw findings, and have a second reviewer score against a separately held-out key.

## Dataset and prompts

Twelve cases: **8 defect cases and 4 clean cases**. Cases 11 and 12 were added for this evaluation; the answer key is listed after the score table.

**Baseline prompt (v1):**

> Review this diff for defects. For each finding give the file, the line, what goes wrong, and how confident you are. If you find nothing, say so explicitly rather than listing general observations.

**Changed prompt (v2):** Prompt-only change; model and available case context were held constant.

> Review this diff for concrete, material defects introduced by the change. For each finding give the file, line, a specific failure path, and confidence. Check authorization, input/error handling, concurrency, sensitive-data logging, and whether changed documentation matches behavior. Do not report style or speculative risks as defects. If no defect is supported, say so explicitly.

## Scores

The desk-scoring produced the same case classifications on each pass. This is repeatability of this reviewer's in-context judgment, **not independent run-to-run model variance**.

| Prompt | Pass | Caught | Missed | False alarms | Defect catch rate | Defect miss rate | Clean-case false-alarm rate |
|---|---:|---:|---:|---:|---:|---:|---:|
| v1 | 1 | 7/8 | 1/8 | 0/4 | 87.5% | 12.5% | 0% |
| v1 | 2 | 7/8 | 1/8 | 0/4 | 87.5% | 12.5% | 0% |
| v1 | 3 | 7/8 | 1/8 | 0/4 | 87.5% | 12.5% | 0% |
| v2 | 1 | 8/8 | 0/8 | 0/4 | 100% | 0% | 0% |
| v2 | 2 | 8/8 | 0/8 | 0/4 | 100% | 0% | 0% |
| v2 | 3 | 8/8 | 0/8 | 0/4 | 100% | 0% | 0% |

Rates use defect cases as the denominator for catch/miss and clean cases for false alarms. Across repeated passes, v1 scored 21/24 defect observations caught and 0/12 clean observations falsely flagged; v2 scored 24/24 and 0/12. Since cases are reused and passes share context, these pooled counts must not be interpreted as 24 independent cases.

## Interpretation and confidence

V2 appears better in this replay: it caught the documentation behavior mismatch missed by v1, while no additional false alarms appeared. **Confidence that this is a real improvement is very low.** The apparent gain is one unique defect case among only eight defect cases; four clean cases provide little evidence about false-alarm risk. Three same-context replays do not increase the effective sample size, and the known fixture labels/context may bias scoring. A fresh, blinded evaluation could produce different results.

## Case-level score and answer key

`C` = caught, `M` = missed, `-` = no finding on a clean case. `FA` would indicate a finding on a clean case.

| Case | Expected | v1 passes 1/2/3 | v2 passes 1/2/3 | Ground-truth issue |
|---|---|---|---|---|
| 01 | Defect | C / C / C | C / C / C | Nullable IDs may be iterated without a null check. |
| 02 | Defect | C / C / C | C / C / C | Query uses interpolated user input, enabling SQL injection. |
| 03 | Clean | - / - / - | - / - / - | Dependency field rename is behavior-preserving. |
| 04 | Defect | C / C / C | C / C / C | Delete endpoint lacks the Admin role guard and ignores delete result. |
| 05 | Clean | - / - / - | - / - / - | Tests add coverage without changing behavior. |
| 06 | Defect | C / C / C | C / C / C | Logs full invoice and session objects, exposing unnecessary data. |
| 07 | Defect | C / C / C | C / C / C | Replacing concurrent storage with `Dictionary` introduces races under concurrent requests. |
| 08 | Defect | M / M / M | C / C / C | Sign-out/sign-in does not reset the in-memory invoice repository. |
| 09 | Clean | - / - / - | - / - / - | Raising ASP.NET Core log threshold is not by itself a demonstrated defect. |
| 10 | Defect | C / C / C | C / C / C | Catch-all converts unexpected failures into success-shaped HTTP 200 responses. |
| 11 | Defect | C / C / C | C / C / C | Removes the Admin-only guard from invoice deletion. |
| 12 | Clean | - / - / - | - / - / - | Local variable rename preserves `CombinedDiscount` behavior. |

Cases 11 and 12 are based on the repository's existing invoice delete authorization and discount calculator code, respectively. Case 11 is a concrete authorization regression; case 12 is intentionally clean.
