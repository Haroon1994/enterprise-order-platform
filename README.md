# Enterprise Order Platform

A modular-monolith order and inventory backend built with C# and ASP.NET Core on .NET 10.

Status: Milestone 0 (project scaffold). No business functionality yet.

## Prerequisites

- .NET 10 SDK
- Git

## Build and test

Run `dotnet build` and then `dotnet test` from the repository root.

## Run

Run `dotnet run --project src/EnterpriseOrderPlatform.Api --urls http://localhost:5080`, then open http://localhost:5080/ping.

## Structure

- `src/` application projects (Api, Application, Domain, Infrastructure)
- `tests/` unit and integration tests
