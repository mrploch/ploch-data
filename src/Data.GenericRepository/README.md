# Generic Repository and Unit of Work

## Overview

The generic [Repository](https://martinfowler.com/eaaCatalog/repository.html) and
[Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html) pattern implementation
for .NET.

## Motivation

The repository and unit of work patterns are a common way of abstracting the data access layer.
They are often used in conjunction with an ORM like the [Entity Framework](https://docs.microsoft.com/en-us/ef/core/).

While many consider EF Core a repository and unit of work, it lacks the ability to be easily mocked with a simple
interfaces.
Some may argue that you can test your code against an in-memory database, it is not always the best option.
It also ties you to a particular ORM, even if, in some cases, you might not want to use the ORM at all.

