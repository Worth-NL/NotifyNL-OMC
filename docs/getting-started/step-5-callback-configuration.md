# Step 5 — Configure the delivery callback

After NotifyNL delivers a notification, it can POST the delivery status back to OMC. OMC uses this callback to write a **contact moment** (`contactmoment`) to OpenKlant — creating a delivery receipt visible in the citizen portal.

---

## 5.1 Make OMC externally reachable

NotifyNL's servers need to reach your OMC instance over HTTPS. If your deployment requires mutual TLS (2-way SSL), ensure the appropriate certificates are in place.

---

## 5.2 Generate a long-lived Bearer token

NotifyNL needs a Bearer token to authenticate its POST requests to OMC. Generate a long-lived token using the [Secrets Manager](../authentication/secrets-manager.md):

```
OMC.SecretsManager.exe 525960
```

This generates a token valid for approximately one year (525,960 minutes). Store it securely.

---

## 5.3 Configure the callback in NotifyNL

In the NotifyNL admin portal:

1. Go to your service settings
2. Set the **callback URL** to:
   ```
   POST https://<your-omc-domain>/Notify/Confirm
   ```
3. Set the **Bearer token** to the token generated in step 5.2

---

## 5.4 Verify dual logging

Send a test notification (e.g. by creating a case as in [Step 3](step-3-deploy-and-test.md)). You should see two log entries in OMC:

1. One log entry for the **outgoing notification** sent to NotifyNL
2. One log entry for the **callback received** from NotifyNL, which creates a contact moment in OpenKlant

In OpenKlant, a `contactmoment` should now be visible for the citizen linked to the test case.

> Contact moments are created for every notification sent, including failed deliveries. The contact moment records whether delivery succeeded or failed, providing an auditable history.
