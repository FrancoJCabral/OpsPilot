# Authentication errors

For 401/403 spikes, verify token issuer, audience, signing keys, clock skew, and credential expiration. Confirm identity-provider availability and inspect a rejected token without logging secrets. Rotate expired credentials and refresh cached signing metadata when required.
