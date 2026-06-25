# Privacy by design

OMC and NotifyNL are designed to minimise the handling and exposure of personal data throughout the notification workflow.

---

## Data minimisation

OMC sends **information-lean notifications** (`informatiearm notificeren`). Email and SMS templates are deliberately designed to avoid including unnecessary personal data. Only data that explicitly appears in a template placeholder is shared with NotifyNL.

The following data is **never sent to NotifyNL** unless it is part of a template:

- Full case status details
- Case descriptions beyond what appears in `((zaak.omschrijving))`
- Contact history
- Any data not referenced by a `((placeholder))`

---

## Data retention

| Component | Retention |
|---|---|
| **OMC** | Retains **no data**. Stateless by design. |
| **NotifyNL** | Retains notification records for **5 days** only, solely to provide delivery confirmation. After 5 days, notification content is deleted. |
| **Log files** | Log files (including Sentry) contain **no personal information**. |

---

## BRP data

When OMC queries BRP / Haal Centraal to retrieve a citizen's address for letter delivery:

- BRP responses are **never persisted** by OMC
- BSN numbers are used internally for lookup only and are **never logged**
- All BRP requests include correlation identifiers for auditability

See [BRP / Haal Centraal](../integrations/brp-haalcentraal.md) for the full security model.

---

## Contact moments

After a notification is delivered (or fails), OMC writes a contact moment (`contactmoment`) to OpenKlant. This record contains:

- The notification subject and body (as sent to the citizen)
- The delivery result (success or failure)
- A timestamp

This provides an auditable delivery history visible in the citizen portal, without requiring NotifyNL to retain the data beyond its 5-day window.
