# Getting Started

## OAuth

The services uses oAuth against the XtremeIdiots website for login, in the user secrets for the project the following is required:

```json
  "XtremeIdiotsAuth": {
    "ClientId": "xxxxx",
    "ClientSecret": "xxxxx"
  }
```

A client Id and secret can be sourced by posting in the forums or raising an issue on GitHub. Alternatively a PR with a 'local development' solution that works around needing oAuth would be very beneficial.

---

## Azure Storage Emulator

The identity storage is backed by Azure tables so to run locally the Azure Storage Emulator must be installed and running.

---
