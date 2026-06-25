# Step 4 — Subscribe to events

OMC needs an active subscription on the **NotificatiesAPI** so that it receives events when cases, tasks, decisions, or messages are created or updated.

---

## 4.1 What to subscribe to

Configure subscriptions for the following channels (`kanaal`) in Open Notificaties:

| Channel (`kanaal`) | Relevant scenarios |
|---|---|
| `zaken` | Case Created, Case Updated, Case Closed |
| `objecten` | Task Assigned, Message Received |
| `besluiten` | Decision Made |

For each channel, subscribe to the `create` action (`actie`).

> **Note:** Due to a race condition in the ZGW stack, OMC listens for `status` resource events on the `zaken` channel rather than the `zaak` resource directly. This is by design — do not subscribe to `resource: zaak` for case scenarios.

---

## 4.2 Subscription endpoint

Point the subscription callback URL to OMC's listen endpoint:

```
POST https://<your-omc-domain>/Events/Listen
```

OMC must be externally reachable from Open Notificaties for this to work. If OMC is behind a reverse proxy, ensure the proxy forwards the correct headers.

---

## 4.3 Verify the subscription

After creating the subscription, Open Notificaties will send a test ping to the OMC endpoint to verify it is reachable. OMC responds to test pings with `206 Partial Content` — this is expected and does not indicate an error.

You can confirm this in the Open Notificaties admin UI where the subscription status should show as active.

Create a test case in your ZGW environment and verify that a notification event appears in Open Notificaties directed at OMC.
