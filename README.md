# XtremeIdiots Portal - Website

| Stage | Status |
| --- | --- |
| Code Quality | [![Code Quality](https://github.com/frasermolyneux/xtremeidiots-portal/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/xtremeidiots-portal/actions/workflows/codequality.yml) |
| Build | [![Build Status](https://dev.azure.com/frasermolyneux/XtremeIdiots-Public/_apis/build/status%2Fxtremeidiots-portal.ReleaseToProduction?repoName=frasermolyneux%2Fxtremeidiots-portal&branchName=main&stageName=build_and_validate)](https://dev.azure.com/frasermolyneux/XtremeIdiots-Public/_build/latest?definitionId=188&repoName=frasermolyneux%2Fxtremeidiots-portal&branchName=main) |
| Release to Production | [![Build Status](https://dev.azure.com/frasermolyneux/XtremeIdiots-Public/_apis/build/status%2Fxtremeidiots-portal.ReleaseToProduction?repoName=frasermolyneux%2Fxtremeidiots-portal&branchName=main&stageName=deploy_prd)](https://dev.azure.com/frasermolyneux/XtremeIdiots-Public/_build/latest?definitionId=188&repoName=frasermolyneux%2Fxtremeidiots-portal&branchName=main)|

## Documentation

* [manual-steps](/docs/manual-steps.md)

---

## Overview

This repository contains XtremeIdiots Portal solution that provides player and game server management for the XtremeIdiots community. There are several integrations with the game servers to collect player data and services to enforce player bans.

The primary users for the website are the community admins that perform the game server and player management.

---

## Related Projects

* [frasermolyneux/azure-landing-zones](https://github.com/frasermolyneux/azure-landing-zones) - The deploy service principal is managed by this project, as is the workload subscription.
* [frasermolyneux/platform-connectivity](https://github.com/frasermolyneux/platform-connectivity) - The platform connectivity project provides DNS and Azure Front Door shared resources.
* [frasermolyneux/platform-strategic-services](https://github.com/frasermolyneux/platform-strategic-services) - The platform strategic services project provides a shared services such as API Management and App Service Plans.

---

## Solution

TODO

---

## Azure Pipelines

The `one-pipeline` is within the `.azure-pipelines` folder and output is visible on the [frasermolyneux/Personal-Public](https://dev.azure.com/frasermolyneux/XtremeIdiots-Public/_build?definitionId=177) Azure DevOps project.
The `.github` folder contains `dependabot` configuration and some code quality workflows.

---

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

---

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
