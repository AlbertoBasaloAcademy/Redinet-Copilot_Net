---
agent: Planner
model: GPT-5.2 (Preview)
description: 'Write the code implementation plan for a feature based on its specification.'
argument-hint: 'Provide the feature specification.'
---

# Write Code Implementation for a Feature

## Task

Write the code implementation plan for a prompted feature based on its specification document in Markdown format.

Do not include any tests nor code, just the plan.

## Context

- The specs file [{specId}.spec.md](/docs/specs/{specId}.spec.md) 
- [/.github/instructions](../instructions) - Existing instruction files directory
- [STRUCTURE.md](/docs/STRUCTURE.md) - Contains the technology stack and approved dependencies

## Steps

- Read and analyze the specification document to understand the feature requirements.

- Review current project structure and previous related implementations for consistency.

- Plan the code structure and components needed to implement the feature.

- Commit and clean repository before starting the implementation. Create an isolated branch for the feature.


## Validation

- Ensure the code implementation plan is complete 
  - [ ] Feature branch is clean and ready for merging