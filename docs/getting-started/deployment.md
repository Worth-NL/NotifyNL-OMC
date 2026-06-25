# Deploying OMC

OMC requires the following ZGW APIs to be running and accessible:

- Open Notificaties
- Open Zaak
- Open Klant (v1 or v2 depending on [workflow version](../workflows/versions.md))
- Objecten + ObjectTypen
- Contactmomenten / Klantinteracties

You can deploy OMC either by building from source or by using the Helm chart available to authorized users in the Worth-NL Helm chart repository.

---

## Deployment in 5 steps

Follow these steps in order. Each step is covered in detail on its own page.

| Step | What you do |
|---|---|
| [Step 1](step-1-notify-environment.md) | Create a NotifyNL account, generate an API key, and create templates for each scenario |
| [Step 2](step-2-zgw-api-keys.md) | Generate API keys and JWT credentials so OMC can access your ZGW services |
| [Step 3](step-3-deploy-and-test.md) | Deploy OMC with the Helm chart, set all environment variables, run health checks |
| [Step 4](step-4-event-subscriptions.md) | Subscribe OMC to the relevant events on the NotificatiesAPI |
| [Step 5](step-5-callback-configuration.md) | Configure NotifyNL to POST delivery status back to OMC |

---

## Build from source

```bash
git clone git@github.com:Worth-NL/NotifyNL-OMC.git
cd NotifyNL-OMC
docker build -f OMC/Infrastructure/WebApi/EventsHandler/Dockerfile --force-rm -t omc .
```

> The `--force-rm` flag and specifying the Dockerfile path from the repo root are both required to avoid a Docker cache key error.

After building the image, proceed to [Step 3](step-3-deploy-and-test.md) to configure and run the container.
