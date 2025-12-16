---
agent: agent
model: GPT-5.2 (Preview)
description: 'Write the code for a feature based on its implementation plan.'
argument-hint: 'Provide the feature implementation plan.'
---

# Write Code Implementation for a Feature

## Task

Write the code implementation for a prompted feature based on its implementation plan.

Do not include any tests, only provide the code.

## Context

- The specs file [{specId}.spec.md](/docs/specs/{specId}.spec.md) 
- The plan file [{specId}.plan.md](/docs/specs/{specId}.plan.md) 
- [/.github/instructions](../instructions) - Existing instruction files directory
- [STRUCTURE.md](/docs/STRUCTURE.md) - Contains the technology stack and approved dependencies

## Steps

- Read and analyze the implementation plan to understand the feature requirements.

- Write the code implementation in appropriate files and directories following project conventions.

- Don not wait for approval or feedback, proceed directly to implementation.

- Update the [STRUCTURE.md](/docs/STRUCTURE.md) document if new dependencies or significant architectural changes are introduced.

- Commit the changes with a descriptive message indicating the feature implemented.

## Validation

- Ensure the code implementation is complete and is syntactically correct
  - [ ] The code builds/compiles without errors
  - [ ] No tests are included, no need testing at this point  
  - [ ] Structure document is updated if necessary
  - [ ] Feature brach is clean and ready for merging