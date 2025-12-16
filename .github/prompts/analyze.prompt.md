---
agent: Analyst
description: 'Analyze the requirements of the product and generate a PRD.'
argument-hint: 'Provide the product name and any specific briefing or context.'
---

# Analyze Requirements and Generate PRD

## Task

Analyze the product requirements and generate a comprehensive Product Requirements Document (PRD) for this project.

## Context

- [README.md](/README.md) for current project overview
- [docs](/docs/**.md) folder for any existing documentation (specifically look for briefings or requirement docs)
- Ask the user for any specific briefing or context related to the product.

### IMPORTANT

If this is a _brownfield_ project, consider the following:

- **Current implementation**: Review existing codebase to understand current features and technical restrictions.

- **Current PRD**: If this project has a previous PRD document, review it for existing features, and update it as needed.

## Steps

1. Define 
  - product scope, 
  - goals [2,3]
2. Identify 
  - functional requirements [3,9],
  - technical constraints [2,5]
3. Write them in a formal format at [/docs/PRD.md](/docs/PRD.md)

## Validation

- Ensure the PRD includes:
  - [ ] Overview and Goals
  - [ ] Functional Requirements
  - [ ] Technical Requirements
  - [ ] System C4 Context diagram