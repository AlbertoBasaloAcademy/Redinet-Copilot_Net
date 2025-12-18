---
agent: Plan
model: GPT-5.2 (copilot)
description: 'Write the testing code for a feature based on its specification.'
argument-hint: 'Provide the feature specification.'
---

# Write Unit Tests for a Feature

## Task

Write the unit testing code for a prompted feature based on its specification.

## Context  

- The specs file [{specId}.spec.md](/docs/specs/{specId}.spec.md)  
- [/.github/instructions](../instructions/bst_unit-test.instructions.md) - Unit test best practices and guidelines
- [/.github/instructions](../instructions/frm_n-unit.instructions.md) - NUnit test generation instructions
- [STRUCTURE.md](/docs/STRUCTURE.md) - Contains the technology stack and approved dependencies

## Steps

- Read and analyze the specification document to understand the feature requirements.

- Review current implementation (use git commands to check the relevant code and history).

- Follow the guidelines from the instruction files to write effective unit tests using.

## Validation

- Ensure the testing code implementation is complete and is syntactically correct
  - [ ] The code builds/compiles without errors
  - [ ] Run all tests and verify they pass successfully
  - [ ] If tests fail, debug and fix the issues; if get caught in a loop, STOP and ask for help;
