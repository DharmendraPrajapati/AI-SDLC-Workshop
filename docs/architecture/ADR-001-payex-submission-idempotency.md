# ADR-001: Idempotent submission of approved invoices to PayEx

- **Status:** Proposed; contingent on PayEx capability verification
- **Decision owner:** Invoice application team
- **Scope:** Submission of an approved invoice to PayEx

## Context

The current application has no PayEx integration or database persistence. `ApprovalService.Approve` changes an invoice's status and decision metadata, then calls `IInvoiceRepository.Update` ([ApprovalService.cs](../../backend/src/InvoiceApp.Domain/ApprovalService.cs)); the API registers `InMemoryInvoiceRepository` ([Program.cs](../../backend/src/InvoiceApp.Api/Program.cs)). The repository implementation stores invoices in a process-local list ([InMemoryInvoiceRepository.cs](../../backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs)).

For the proposed production design, the team has identified ASP.NET Core 8, Azure SQL, Azure Service Bus, and PayEx as the intended technology context. Those components are design inputs, not descriptions of the current application.

A PayEx request that times out after 30 seconds has an ambiguous result: PayEx may have accepted it even though the application did not receive a response. Repeating the request without a provider-side deduplication guarantee can create a duplicate external effect. A local database transaction cannot atomically commit with PayEx.

## Decision

Prefer **a stable idempotency key for each logical invoice submission, backed by a durable Azure SQL deduplication/submission record with a uniqueness constraint**.

This decision is conditional: before production implementation, verify that PayEx atomically deduplicates requests for the same key, defines the key's scope and retention period, and returns or exposes the original submission result on retries. A reliable PayEx lookup by the same stable merchant reference may be an alternative way to resolve ambiguous outcomes.

The local record protects against duplicate internal operations. It does **not**, by itself, guarantee exactly-once PayEx effects. If PayEx provides neither idempotent submission nor reliable lookup, an ambiguous request must remain unresolved for reconciliation; automatic retry cannot safely promise exactly-once behavior.

## Proposed behavior

1. When an invoice becomes eligible for payment submission, create a durable submission record and a stable key representing that logical submission. Enforce uniqueness in Azure SQL so concurrent requests cannot create separate local submissions for the same logical operation.
2. Send that same key to PayEx on every attempt. Do not generate a fresh key for a retry of the same submission.
3. Persist an acknowledged PayEx result and link it to the submission record. Treat repeated requests and responses idempotently inside the application.
4. On timeout or connection loss, record the outcome as **unknown / pending verification**, not as a definite failure. Resolve it by retrying with the same key only if PayEx's contract guarantees deduplication, or by querying PayEx using the stable reference.
5. If PayEx cannot resolve the outcome, do not automatically issue a new logical submission. Escalate it for reconciliation or operator review.
6. Emit metrics and alerts for submission attempts, provider latency/timeouts, unknown outcomes, retry counts, and the age of the oldest unresolved submission.

Whether submission is synchronous with approval or asynchronous, and whether Service Bus is used to dispatch attempts, remain implementation decisions. If Azure Service Bus is used, treat deliveries as potentially repeated; message deduplication does not replace PayEx-side idempotency.

## Alternatives considered

### Transactional outbox with a worker

An outbox can commit the approved/submission state and an event in one Azure SQL transaction, then use a worker to publish/process it. This reduces the risk of losing work between database commit and message publication and supports retries. It does not make the external PayEx effect exactly once: a worker can lose the response after PayEx accepts a request and then process the message again. It still needs the same provider idempotency or lookup behavior described above.

### Optimistic submission with nightly reconciliation

This can be operationally simpler initially, but unknown results may remain unresolved until the reconciliation run. The job still cannot safely determine whether to retry an ambiguous submission unless PayEx supports deduplication or lookup. A nightly process is therefore a backstop, not a substitute for safe retries or prompt monitoring.

## Consequences

- The design requires durable state, unique constraints, retention and operational handling for unresolved submissions.
- The definition of “exactly once” is one logical submission/effect at PayEx, not one network request. Network attempts may occur more than once.
- Guarantees depend on PayEx's actual contract, including concurrent same-key requests, key retention, request equivalence rules, and result retrieval. These are currently unknown and are a release gate.
- The stated peak of 80 approvals per minute is a workload assumption for capacity testing, not evidence that the present app can sustain that rate.
- Azure SQL and Service Bus sizing, PayEx limits, delivery latency objectives, reconciliation frequency, and cost require measured workload and environment-specific estimates.

## Verification required before implementation

- Confirm PayEx supports idempotency keys or reliable status lookup for a stable merchant reference.
- Confirm key scope, retention, behavior under concurrent requests, and response semantics for repeated submissions.
- Confirm what fields make two requests equivalent and how corrections or resubmissions are represented as a new logical operation.
- Define alert thresholds, unresolved-submission handling, retry policy, reconciliation frequency, and operator runbook.
- Load-test the complete path at and above the stated 80 approvals per minute, including 30-second timeouts and provider throttling.
- Produce a cost estimate using actual Azure region/SKUs, existing capacity, expected monthly volume, retention, and support requirements.

## Where above info was wrong or useless

- **The local dedupe table was described too loosely in some wording.** It only prevents duplicate local records/operations when correctly constrained. It cannot establish whether PayEx accepted a timed-out request and cannot alone prevent duplicate provider effects. Provider-side idempotency or reliable status lookup is necessary for safe automated recovery.
- **The estimated “about 40 in-flight” calls was only a conditional arithmetic illustration.** It follows from 80/minute multiplied by 30 seconds if requests arrive steadily and all take 30 seconds to complete. It is not a measured concurrency, a system capacity limit, or proof that a failure will occur.
- **The cost estimates ($10–$100/month and $4,000–$24,000 or earlier ranges) were unsupported planning guesses.** They lacked Azure region/SKU, existing SQL capacity, traffic distribution, monthly volume, retention, PayEx fees, staffing rates, and implementation scope. The changing build ranges were inconsistent. Do not use them for budgeting.
- **The 3 a.m. incident narrative was a scenario, not a prediction.** Provider throttling, a growing unknown-outcome queue, and the listed on-call symptoms depend on implementation, telemetry, PayEx behavior, and actual load. The current repository has no payment call, database, worker, or those metrics.
- **A nightly reconciliation delay is conditional.** The delay depends on the schedule and detection/processing time; “hours” was not established by code or an operational schedule.
- **The stack was treated as if already implemented in parts of the discussion.** Azure SQL, Azure Service Bus, and PayEx are user-provided target assumptions. Current source registers an in-memory invoice repository and contains no PayEx or database integration ([Program.cs](../../backend/src/InvoiceApp.Api/Program.cs), [InMemoryInvoiceRepository.cs](../../backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs), [ApprovalService.cs](../../backend/src/InvoiceApp.Domain/ApprovalService.cs)).
