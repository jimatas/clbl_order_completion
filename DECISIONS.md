| Assumption / decision | Reason |
|---|---|
| An order must be in the `Submitted` state, contain at least one order line, have exact delivery quantities, and be at least six calendar months old. | Per the stated completion rules. Empty orders and overdeliveries are not considered complete; the six month boundary is inclusive. |
| Eligibility is evaluated by the `Order` entity; the use case only coordinates workflow. | Keeps business rules in the domain model and orchestration in the application service. |
| The current UTC time is obtained through `TimeProvider` and passed to `Order`. | Makes time behavior deterministic and testable. |
| Notification failures caused by HTTP `500` responses or connection errors are retried a configurable number of times. | These failures are considered transient for the purpose of this assignment. |
| An order is updated only after notification succeeds. On final notification failure, it is logged and skipped. | As required; the scheduled job can retry it later. |
| Exactly-once notification cannot be guaranteed. | The external service, as implemented, provides no idempotency guarantee. A local failure after notification but before updating may cause a duplicate later. |
| The endpoint only acknowledges batch processing but does not return per-order outcomes. | The supplied use case contract returns `void`; changing it is considered outside the scope of the assignment. |