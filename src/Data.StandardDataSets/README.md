# Ploch.Data.StandardDataSets

Ready-made reference data sets for seeding databases and populating lookup tables.

## Overview

This library contains classes that can generate commonly used datasets like a country list.

| Type | Purpose |
|---|---|
| `Regions` | World regions, countries and their standard codes |

To seed a `DbContext` with it, derive from the abstract
`DataSeeder<TDbContext>` in [Ploch.Data.EFCore](https://www.nuget.org/packages/Ploch.Data.EFCore/)
and call `Execute()`.

## Documentation

- [Full documentation](https://data.github.ploch.dev/)
