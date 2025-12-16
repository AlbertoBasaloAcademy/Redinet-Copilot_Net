---
agent: Architect
description: 'Guid the implementation process by creating instruction files for project technology stack.'
argument-hint: 'Provide the STRUCTURE document or relevant context.'
---

# Create or update instructions to cover structure tech stack

## Task

Generate comprehensive instruction files for each tech stack item and architecture decision, providing best practices and usage guidelines for AI agents and developers.

## Context

- [STRUCTURE.md](/docs/STRUCTURE.md) - Contains the technology stack and approved dependencies
- [Clean Code Instructions](../instructions/bst_clean-code.instructions.md) - Example of an existing instruction file
- [/.github/instructions](../instructions) - Existing instruction files directory

## Steps

### 1. Analysis Phase

- Read and analyze [STRUCTURE.md](/docs/STRUCTURE.md) to identify all approved technologies (technology items), including:
  - Programming Languages
  - Application Frameworks
  - Production Libraries
  - Development Tools
  - Services
- Read and analyze the [/.github/instructions](../instructions) directory for existing instruction files
- Create a list of missing instruction files needed for each technology item

### 2. Research and Generation

For each missing instruction file:

- [ ] Use #web/fetch tool to visit and read the [copilot custom instructions catalog](https://github.com/github/awesome-copilot/blob/main/docs/README.instructions.md)
- [ ] Choose the most relevant instruction file as a base template
- [ ] Use #web/fetch tool to research the documentation and best practices for the technology item
- [ ] Summarize (less than 250 lines) findings and create a draft for the instruction file.

### 3. File Creation

Create instruction files at [/.github/instructions](../instructions) folder following this naming convention: `{type}-{name}.instructions.md`

Type can be:
- `lng` for languages
- `frm` for frameworks
- `lib` for libraries
- `tol` for tools
- `srv` for services

Try to use simple examples and keep the instructions file concise, ideally under 250 lines.

## Validation

- Ensure all technology items from STRUCTURE.md have corresponding instruction files
  - [ ] All approved dependencies have corresponding instruction files
  - [ ] Files are properly formatted with front matter
  - [ ] Examples are relevant to the development environment
  - [ ] Language and framework integration is covered
  - [ ] Files follow consistent structure and naming

