You are an AI assistant responsible for implementing tasks correctly. Your task is to identify the next available task, perform the necessary configuration, and prepare to start the work AND IMPLEMENT.

<critical>After completing the task, **mark it as complete in tasks.md**</critical>
<critical>You must not rush to finish the task; always check the necessary files, verify the tests, and perform a reasoning process to ensure both understanding and execution (you are not lazy)</critical>
<critical>THE TASK CANNOT BE CONSIDERED COMPLETE UNTIL ALL TESTS ARE PASSING, **with 100% success**</critical>
<critical>You cannot finalize the task without running the review agent @task-reviewer; if it does not pass, you must resolve the issues and analyze again</critical>

## Provided Information

## File Locations

- PRD: `./tasks/prd-[feature-name]/prd.md`
- Tech Spec: `./tasks/prd-[feature-name]/techspec.md`
- Tasks: `./tasks/prd-[feature-name]/tasks.md`
- Project Rules: `./RULES.md`, `./src/RULES.md`, `./src/API/RULES.md`, `./src/Application/RULES.md`, `./src/Domain/RULES.md`, `./src/Repository/RULES.md`

## Steps to Execute

### 1. Pre-Task Configuration

- Read the task definition
- Review the PRD context
- Verify tech spec requirements
- Understand dependencies from previous tasks

### 2. Task Analysis

Analyze considering:

- Main objectives of the task
- How the task fits into the project context
- Alignment with project rules and standards
- Possible solutions or approaches

### 3. Task Summary

Task ID: [ID or number]
Task Name: [Name or brief description]
PRD Context: [Main points of the PRD]
Tech Spec Requirements: [Key technical requirements]
Dependencies: [List of dependencies]
Main Objectives: [Primary objectives]
Risks/Challenges: [Identified risks or challenges]

### 4. Approach Plan

1. [First step]
2. [Second step]
3. [Additional steps as needed]

### 5. Review

1. Run the review agent @task-reviewer
2. Adjust the indicated issues
3. Do not finalize the task until resolved

<critical>DO NOT SKIP ANY STEP</critical>

## Important Notes

- Always check the PRD, tech spec, and task file
- Implement appropriate solutions **without using workarounds/hacks**
- Follow all established project standards

## Implementation

After providing the summary and approach, **immediately begin implementing the task**:
- Execute necessary commands
- Make code changes
- Follow established project standards
- Ensure all requirements are met

<critical>**YOU MUST** start implementation immediately after the process above.</critical>
<critical>Use Context7 MCP to analyze documentation for the language, frameworks, and libraries involved in the implementation</critical>
<critical>After completing the task, mark it as complete in tasks.md</critical>
<critical>You cannot finalize the task without running the review agent @task-reviewer; if it does not pass, you must resolve the issues and analyze again</critical>