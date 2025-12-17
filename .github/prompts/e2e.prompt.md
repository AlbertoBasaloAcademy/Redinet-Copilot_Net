---
agent: Plan
model: GPT-5.2 (Preview)
description: 'Write the End to End testing code for a feature based on its specification.'
argument-hint: 'Provide the feature specification.'
---

# Write E2E Tests for a Feature

## Task

Write the end to end testing code for a prompted feature based on its specification.

## Context  

- The specs file [{specId}.spec.md](/docs/specs/{specId}.spec.md)  
- [STRUCTURE.md](/docs/STRUCTURE.md) - Contains the technology stack and approved dependencies

## Steps

- Read and analyze the specification document to understand the feature requirements.

- Write a bash script with curl commands to simulate user interactions with the application, covering all critical user journeys as outlined in the specification.

- Save the script at [/docs/specs/{specId}.e2e.sh](/docs/specs/{specId}.e2e.sh).

## Validation

- Ensure the testing code implementation is complete and is syntactically correct
  - [ ] The code builds/compiles without errors
  - [ ] Run the main program 
  - [ ] Run all tests and verify they pass successfully
  - [ ] If tests fail, debug and fix the issues; if get caught in a loop, STOP and ask for help;
  - [ ] Stop or kill any running instances of the main program and tests after validation is complete
