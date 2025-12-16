---
agent: Plan
model: GPT-5.2 (Preview)
description: 'Write the code implementation for a feature based on its specification.'
argument-hint: 'Provide the feature specification.'
---

# Write Code Implementation for a Feature

## Task

Write the code implementation for a prompted feature based on its specification document in Markdown format.

Do not include any tests, only provide the code.

## Context

- The specs file [{specId}.spec.md](/docs/specs/{specId}.spec.md) 
- [/.github/instructions](../instructions) - Existing instruction files directory
- [STRUCTURE.md](/docs/STRUCTURE.md) - Contains the technology stack and approved dependencies

## Steps

- Read and analyze the specification document to understand the feature requirements.

- Review current project structure and previous related implementations for consistency.

- Plan the code structure and components needed to implement the feature.

- Write the code implementation in appropriate files and directories following project conventions.

- Update the [STRUCTURE.md](/docs/STRUCTURE.md) document if new dependencies or significant architectural changes are introduced.

## Validation

- Ensure the code implementation is complete and is syntactically correct
  - [ ] The code builds/compiles without errors
  - [ ] No tests are included, no need testing at this point  
  - [ ] Structure document is updated if necessary