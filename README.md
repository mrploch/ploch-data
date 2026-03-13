[![Build, Test and Analyze .NET](https://github.com/mrploch/ploch-data/actions/workflows/build-dotnet.yml/badge.svg)](https://github.com/mrploch/ploch-data/actions/workflows/build-dotnet.yml)
[![pages-build-deployment](https://github.com/mrploch/ploch-data/actions/workflows/pages/pages-build-deployment/badge.svg)](https://github.com/mrploch/ploch-data/actions/workflows/pages/pages-build-deployment)
[![Qodana](https://github.com/mrploch/ploch-data/actions/workflows/code_quality.yml/badge.svg)](https://github.com/mrploch/ploch-data/actions/workflows/code_quality.yml)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mrploch_ploch-data&metric=alert_status&token=1ea9277b2f110b6b2d99685a20c037074d08d1c1)](https://sonarcloud.io/summary/new_code?id=mrploch_ploch-data)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrploch_ploch-data&metric=coverage&token=1ea9277b2f110b6b2d99685a20c037074d08d1c1)](https://sonarcloud.io/summary/new_code?id=mrploch_ploch-data)

| Package | Version | Downloads |
|---------|---------|-----------|
| Ploch.Data.Model | [![version][model-v]][model] | [![downloads][model-d]][model] |
| Ploch.Data.GenericRepository | [![version][gr-v]][gr] | [![downloads][gr-d]][gr] |
| Ploch.Data.GenericRepository.EFCore | [![version][gref-v]][gref] | [![downloads][gref-d]][gref] |
| Ploch.Data.EFCore | [![version][ef-v]][ef] | [![downloads][ef-d]][ef] |

[model]: https://www.nuget.org/packages/Ploch.Data.Model/
[model-v]: https://img.shields.io/nuget/v/Ploch.Data.Model.svg
[model-d]: https://img.shields.io/nuget/dt/Ploch.Data.Model.svg
[gr]: https://www.nuget.org/packages/Ploch.Data.GenericRepository/
[gr-v]: https://img.shields.io/nuget/v/Ploch.Data.GenericRepository.svg
[gr-d]: https://img.shields.io/nuget/dt/Ploch.Data.GenericRepository.svg
[gref]: https://www.nuget.org/packages/Ploch.Data.GenericRepository.EFCore/
[gref-v]: https://img.shields.io/nuget/v/Ploch.Data.GenericRepository.EFCore.svg
[gref-d]: https://img.shields.io/nuget/dt/Ploch.Data.GenericRepository.EFCore.svg
[ef]: https://www.nuget.org/packages/Ploch.Data.EFCore/
[ef-v]: https://img.shields.io/nuget/v/Ploch.Data.EFCore.svg
[ef-d]: https://img.shields.io/nuget/dt/Ploch.Data.EFCore.svg

# Ploch.Data Libraries

## Overview

This repository contains various projects for working with data in .NET.

## Features

### [Generic Repository and Unit of Work](src/Data.GenericRepository/README.md)

A generic repository and unit of work pattern implementation for Entity Framework Core.

### [Common Data Model](src/Data.Model/README.md)

A set of common interfaces and classes for building a domain data model.

### [Data Utilities](src/Data.Utilities/README.md)

Various utilities for working with data.

### [Entity Framework Core Utilities](src/Data.EFCore)

Utility classes for working with Entity Framework Core.

### [Common Datasets](src/Data.StandardDataSets)

Common data sets like a list of regions, countries, etc.
