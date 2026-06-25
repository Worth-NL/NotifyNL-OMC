# Scalability

---

## Stateless design

OMC holds no state between requests. Every incoming event is processed independently with no shared in-memory state between concurrent requests (beyond configuration caching, which is read-only after startup).

This means:

- **Multiple instances** of OMC can run simultaneously without coordination
- **Horizontal scaling** is straightforward — add more replicas in Kubernetes as load increases
- **No sticky sessions** are required at the load balancer
- **Restart** of any instance has no impact on other instances or on in-flight notifications

---

## Configuration caching

OMC caches configuration values and frequently-read ZGW responses in thread-safe concurrent dictionaries after first read. This avoids repeated calls to the same endpoint within a single request lifecycle while remaining safe under concurrent load.

---

## HTTP connection pooling

Outbound HTTP connections to ZGW services are pooled and reused across requests. The pool is configured via `appsettings.json`:

| Setting | Default | Description |
|---|---|---|
| `Network.ConnectionLifetimeInSeconds` | `90` | How long a pooled connection is kept alive |
| `Network.HttpRequestTimeoutInSeconds` | `60` | Per-request timeout |
| `Network.HttpRequestsSimultaneousNumber` | `20` | Max concurrent outbound requests |

Adjust these values in `appsettings.json` or via [environment variable override](../configuration/appsettings.md#overriding-appsettings-values-with-environment-variables) if your ZGW services are under heavy load or have stricter connection limits.

---

## Deployment

OMC is deployed as a Docker container, typically via Helm on Kubernetes. Set `replicaCount` in your Helm values to scale horizontally. No additional coordination configuration is needed.
