# Ploch.Common.Data.Model

## Overview

This library contains common interfaces for database entities.

## Motivation

In most database-driven apps, we need similar or even the same types of common objects in our database.

In many projects, I had to create a `Category` or a `Tag` class.

Also, I find myself using the same properties on many entity types.

Things like `Name`, `Title`, `Description` and so on.

Standardization of those types brings a few benefits:

- It makes it possible to re-use components and UI elements between projects
- Common styling can be implemented, especially in strongly typed UIs like XAML or Blazor

*
    - UI components can be created that can be used to edit any entity that implements a common interface
