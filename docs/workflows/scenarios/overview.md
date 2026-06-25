# Scenarios

OMC implements six notification scenarios. Each scenario is triggered by a specific event from Open Notificaties and follows a defined processing workflow.

---

## How scenarios work

When OMC receives an event from Open Notificaties, it:

1. **Pre-validates** the incoming JSON (action, channel, resource)
2. **Fetches** the relevant case, party, and type data from the ZGW APIs
3. **Matches** the event to a scenario (or returns `206 Partial Content` if no scenario applies)
4. **Checks the whitelist** — the case type must be whitelisted or `"*"` must be set
5. **Checks the `informeren` flag** — the case type must have notifications enabled
6. **Determines the party and contact method** (see address selection below)
7. **Sends the notification** via NotifyNL using the configured template
8. **Receives the delivery callback** from NotifyNL and writes a contact moment to OpenKlant

---

## Supported scenarios

| Scenario | Page |
|---|---|
| Case Created | [case-created.md](case-created.md) |
| Case Updated | [case-updated.md](case-updated.md) |
| Case Closed | [case-closed.md](case-closed.md) |
| Task Assigned | [task-assigned.md](task-assigned.md) |
| Decision Made | [decision-made.md](decision-made.md) |
| Message Received | [message-received.md](message-received.md) |

---

## Party and address selection

Once OMC has identified the relevant party (citizen or organisation), it selects a delivery address in this order:

1. **Case identifier match** — if the party has a digital address (`digitaalAdres`) that matches the case identifier, that address is used
2. **Preferred address** (`voorkeursAdres`) — if set on the party record, this is used as fallback
3. **Fallback address** — the first available digital address of the appropriate type

The address type (email, phone/SMS, postal) determines the notification channel.

> In workflow v2, OMC supports both **citizens** (identified by BSN — `bsn`) and **organisations** (identified by KVK number — `kvk`). In workflow v1, only citizens are supported.

---

## Template placeholders

Each scenario defines a set of `((placeholder))` variables that must be present in your NotifyNL templates. The placeholders are filled at send time with data fetched from the ZGW APIs.

Common placeholders available in all scenarios:

| Placeholder | Description |
|---|---|
| `((klant.voornaam))` | First name of the party |
| `((klant.voorvoegselAchternaam))` | Name infix (e.g. "van der") |
| `((klant.achternaam))` | Last name of the party |

Scenario-specific placeholders are listed on each scenario page.

---

## Not implemented scenario

If the incoming event cannot be matched to any scenario (wrong action/channel/resource combination, or a scenario not yet implemented), OMC returns `206 Partial Content`. This tells Open Notificaties **not to retry** the event, since retrying would produce the same result.

OMC returns a readable message in the response body explaining why no scenario was matched.
