---
agent: Analyst
description: 'Write the specification for a feature.'
argument-hint: 'Provide the PRD and STRUCTURE documents and the feature to be specified.'
---

# Write a Specification Document for a Feature

## Task

Write detailed specifications for a prompted feature in Markdown format composed of three main parts:

1. Problem Specification

2. Solution Design

3. Acceptance Criteria

## Context

- [PRD.md](/docs/PRD.md)
- [STRUCTURE.md](/docs/STRUCTURE.md)
- [Feature Specification Template](../instructions/tpl_feature-spec.instructions.md)

## Steps

- Think about and write a short (up to 3) list of user stories that describe the feature from the user's perspective.

- Think about and write data models, software components, user interfaces, and aspects (monitoring, security, error handling) for the feature.

- Think about and write a short (up to 3) list of acceptance criteria for each user story using the EARS format: SHALL, WHEN, IF, THEN, WHILE, WHERE.

- CHOOSE THE SIMPLEST APPROACH FOR EACH ITEM.

- Generate a unique specification ID (e.g., 001-slug_short_description).

- Write the feature specification in Markdown format at `/docs/specs/{specId}.spec.md`.

## Validation

- Ensure the specs file [{specId}.spec.md](/docs/specs/{specId}.spec.md) exists
  - [ ] The file is relevant for the feature described in PRD.md
  - [ ] The file contains Problem Specification, Solution Design, and Acceptance Criteria sections