[![Build, Test and Analyze .NET](https://github.com/mrploch/ploch-data/actions/workflows/build-dotnet.yml/badge.svg)](https://github.com/mrploch/ploch-data/actions/workflows/build-dotnet.yml)
[![pages-build-deployment](https://github.com/mrploch/ploch-data/actions/workflows/pages/pages-build-deployment/badge.svg)](https://github.com/mrploch/ploch-data/actions/workflows/pages/pages-build-deployment)
[![Qodana](https://github.com/mrploch/ploch-data/actions/workflows/code_quality.yml/badge.svg)](https://github.com/mrploch/ploch-data/actions/workflows/code_quality.yml)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mrploch_ploch-data&metric=alert_status&token=1ea9277b2f110b6b2d99685a20c037074d08d1c1)](https://sonarcloud.io/summary/new_code?id=mrploch_ploch-data)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrploch_ploch-data&metric=coverage&token=1ea9277b2f110b6b2d99685a20c037074d08d1c1)](https://sonarcloud.io/summary/new_code?id=mrploch_ploch-data)

| Package | NuGet |
|---------|-------|
| Ploch.Data.Model | [![NuGet](https://img.shields.io/nuget/v/Ploch.Data.Model.svg)](https://www.nuget.org/packages/Ploch.Data.Model/) [![NuGet Downloads](https://img.shields.io/nuget/dt/Ploch.Data.Model.svg)](https://www.nuget.org/packages/Ploch.Data.Model/) |
| Ploch.Data.GenericRepository | [![NuGet](https://img.shields.io/nuget/v/Ploch.Data.GenericRepository.svg)](https://www.nuget.org/packages/Ploch.Data.GenericRepository/) [![NuGet Downloads](https://img.shields.io/nuget/dt/Ploch.Data.GenericRepository.svg)](https://www.nuget.org/packages/Ploch.Data.GenericRepository/) |
| Ploch.Data.GenericRepository.EFCore | [![NuGet](https://img.shields.io/nuget/v/Ploch.Data.GenericRepository.EFCore.svg)](https://www.nuget.org/packages/Ploch.Data.GenericRepository.EFCore/) [![NuGet Downloads](https://img.shields.io/nuget/dt/Ploch.Data.GenericRepository.EFCore.svg)](https://www.nuget.org/packages/Ploch.Data.GenericRepository.EFCore/) |
| Ploch.Data.EFCore | [![NuGet](https://img.shields.io/nuget/v/Ploch.Data.EFCore.svg)](https://www.nuget.org/packages/Ploch.Data.EFCore/) [![NuGet Downloads](https://img.shields.io/nuget/dt/Ploch.Data.EFCore.svg)](https://www.nuget.org/packages/Ploch.Data.EFCore/) |

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
