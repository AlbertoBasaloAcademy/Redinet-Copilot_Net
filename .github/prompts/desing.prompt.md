---
description: 'Design the technical solution and guidelines to implement the product based on the PRD.'
agent: Architect
argument-hint: 'Provide the Product Requirements Document (PRD) or relevant context.'
---

# Design Technical Solution and Define Implementation Guidelines

## Task

Design the high-level technical solution and define the implementation guidelines based on the provided Product Requirements Document (PRD).

## Context

- [PRD.md](/PRD.md) for current project requirements
- [STRUCTURE.md](/docs/STRUCTURE.md) if there is a previous architecture documentation
- [docs](/docs/**.md) folder for any existing documentation

### IMPORTANT

If this is a brownfield project, consider the following:

- **Current implementation**: Review existing codebase and architecture to understand current design decisions and constraints.

- **Current STRUCTURE**: If this project has a previous STRUCTURE document, review it for existing architecture and guidelines, and update it as needed.

- Propose changes that align with the new PRD while considering existing systems. Mark those changes as "⚡ Proposed" in the STRUCTURE document.

## Steps

1. Define 
  - technical stack, 
  - architecture patterns,
  - development workflow.
2. Identify 
  - repository structure,
  - development infrastructure.
3. Write (create or update) them in a formal document at [/docs/STRUCTURE.md](/docs/STRUCTURE.md)

## Validation

- Ensure the STRUCTURE document includes:
  - [ ] Technical Stack and Architecture Patterns
  - [ ] Repository Structure and Development Infrastructure
