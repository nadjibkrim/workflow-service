# Workflow Service

This is a minimal ASP.NET Core 8.0 microservice that manages the lifecycle of records through a state machine. Records can transition between states automatically based on conditions associated with each state.

## Features

- ✅ Minimal API built with .NET 8
- 🔁 State machine logic using `Next()`-based automatic transitions
- 📦 SQLite by default (can switch to PostgreSQL)
- 🔄 Supports manual and automatic state transitions
- 📂 Easily adaptable for real-world workflows

## Endpoints

- `POST /records` — create a new record
- `GET /records` — list all records
- `GET /records/{id}` — get a specific record
- `POST /records/{id}/transition` — manually set next state (optional)
- `POST /records/{id}/next` — automatically determine and apply the next state based on rules

## Run locally

```bash
dotnet restore
dotnet ef database update
dotnet run
