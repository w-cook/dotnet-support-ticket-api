# ASP.NET Core Support Ticket API

A focused backend portfolio project built with C# and ASP.NET Core Web API.

## Purpose

This project is designed to demonstrate practical backend development skills in a way that supports:
- remote .NET / C# job applications
- maintenance and support-oriented software roles
- future freelance work involving ASP.NET Core bug fixing and API improvement

## Project Concept

The API models a simple support / issue tracking workflow with tickets, comments, users, and status history.

## MVP Goals

- create ticket
- get ticket by ID
- list tickets
- filter tickets
- update ticket status
- add comments
- track status history

## Planned Tech

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL or SQL Server
- Swagger / OpenAPI
- xUnit
- Docker

## Why This Project

This project is intentionally scoped around support and maintenance workflows rather than flashy greenfield features.

The goal is to create a portfolio asset that demonstrates:
- practical API design
- relational data modeling
- maintainability
- debugging and support mindset
- clear documentation

## Status

In progress

## Core Entities

### Ticket
- Id
- Title
- Description
- Priority
- Status
- CreatedAt
- UpdatedAt
- CreatedByUserId
- AssignedToUserId

### Comment
- Id
- TicketId
- AuthorUserId
- Body
- CreatedAt

### User
- Id
- Name
- Email

### StatusHistory
- Id
- TicketId
- OldStatus
- NewStatus
- ChangedAt

## MVP Endpoints

- POST /tickets
- GET /tickets/{id}
- GET /tickets
- PATCH /tickets/{id}/status
- POST /tickets/{id}/comments

### Planned Filters for GET /tickets
- status
- priority
- assignedToUserId
- createdByUserId

## Current Progress

- ASP.NET Core Web API project created
- OpenAPI and Swagger UI configured
- Core entity classes created
- Entity Framework Core added
- AppDbContext created
- SQL Server LocalDB configured
- Initial migration created
- Database schema generated successfully
- Basic seed data added