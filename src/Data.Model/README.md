# Ploch.Common.Data.Model

## Overview

Library provides a set of interfaces and classes that can be used to standardize the entity model.

## Motivation

In many database-driven apps, we need similar or even the same model types.

Entities usually have an `Id` property.
Lots of them have audit properties like `CreatedTime`, `ModifiedTime` or `AccessedTime`.
The same goes for `Name` or `Title` properties.

This project tries to bring some standardization to those types, which brings many benefits.
To name a few:

- Audit properties can be handled automatically, without the need to implement them in every entity
- It makes it possible to re-use components and UI elements between projects
- Common styling can be implemented, especially in strongly typed UIs like XAML or Blazor
- UI components can be created that can be used to edit any entity that implements a common interface

## Interfaces

Most of the types in this library have self-explanatory names, for example:

- [`IHasId`](./IHasId.cs) - an entity that has an `Id` property
- [`IHasName`](./IHasName.cs) - an entity that has a `Name` property
- [`IHasTitle`](./IHasTitle.cs) - an entity that has a `Title` property

### Auditable Entities Interfaces

Library has a set of interfaces for created, modified and accessed date/time and account properites.
Those are usually used in audit trails. Having those properties in a common interface makes it possible to implement
a centralized logic for handling of them.

- [`IHasCreatedTime`](./IHasCreatedTime.cs) - an entity that has a `CreatedTime` property
- [`IHasCreatedBy`](./IHasCreatedBy.cs) - an entity that has a `CreatedBy` property
- [`IHasModifiedTime`](./IHasModifiedTime.cs) - an entity that has a `ModifiedTime` property
- [`IHasModifiedBy`](./IHasModifiedBy.cs) - an entity that has a `ModifiedBy` property
- [`IHasAccessedTime`](./IHasAccessedTime.cs) - an entity that has a `AccessedTime` property
- [`IHasAccessedBy`](./IHasAccessedBy.cs) - an entity that has a `AccessedBy` property
- [`IHasAudit`](./IHasAudit.cs) - an entity that has all of the above properties

The full list of interfaces is available in the
[API documentation](https://github.ploch.dev/ploch-common/api/Ploch.Common.Data.Model.html).
