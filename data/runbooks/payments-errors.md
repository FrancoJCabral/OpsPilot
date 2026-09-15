# Payments errors

Keywords: payments, payments 503, payment gateway, payments deployment.

For HTTP 502/503 responses after a deployment, compare the active release configuration with the previous version. Check payment-provider health, gateway timeouts, credentials, circuit-breaker state, and downstream latency. If errors started with the release and dependencies are healthy, roll back and preserve logs and correlation IDs.
