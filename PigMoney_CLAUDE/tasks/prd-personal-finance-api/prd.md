# Product Requirements Document (PRD)
# Personal Finance Management API

## Overview

Individuals managing personal finances lack a centralized, structured way to track income, expenses, budgets, and accounts in one place. This API provides a single-user backend service for complete personal finance control — enabling users to manage wallets (accounts), categorize transactions, monitor budgets, and gain full visibility over their financial health. It is designed to be consumed by a web front-end and a personal finance dashboard.

## Objectives

- Enable 100% tracking of all financial transactions (income and expenses)
- Provide structured financial data to power a web front-end and dashboard
- Support real-time financial visibility through filterable, paginated data access
- Respond to all API requests within 500ms under normal load
- Handle concurrent requests reliably without data inconsistency

## User Stories

**As a dashboard consumer (front-end application), I want to:**

1. Create and manage financial accounts (wallets) so I can organize money across different sources (e.g., bank account, cash, credit card).
2. Create and manage categories so I can classify transactions meaningfully.
3. Register income transactions linked to an account so I can track money coming in.
4. Register expense transactions linked to an account and category so I can track money going out.
5. Add itemized line items to an expense so I can break down complex purchases.
6. Create budgets linked to a category so I can set spending limits per category.
7. Retrieve paginated lists of transactions with filters (e.g., by date, category, account) so I can analyze financial data efficiently.
8. Update and delete any financial record so I can correct or remove outdated data.

## Core Features

### 1. Account Management
Manages financial accounts (wallets). Each account has a name, type (e.g., checking, savings, cash, credit), and current balance.

**Functional Requirements:**
- FR-1.1: The system shall allow creating an account with name, type, and initial balance.
- FR-1.2: The system shall allow updating account name, type, and balance.
- FR-1.3: The system shall allow deleting an account.
- FR-1.4: The system shall return a paginated list of accounts.
- FR-1.5: The system shall return a single account by ID.

### 2. Category Management
Manages transaction categories used to classify incomes and expenses.

**Functional Requirements:**
- FR-2.1: The system shall allow creating a category with a name and optional description.
- FR-2.2: The system shall allow updating a category.
- FR-2.3: The system shall allow deleting a category.
- FR-2.4: The system shall return a paginated list of categories.
- FR-2.5: The system shall return a single category by ID.

### 3. Income Management
Tracks all money coming in, linked to an account.

**Functional Requirements:**
- FR-3.1: The system shall allow registering an income with amount, date, description, and associated account.
- FR-3.2: The system shall allow updating income records.
- FR-3.3: The system shall allow deleting income records.
- FR-3.4: The system shall return a paginated list of incomes, filterable by date range and account.
- FR-3.5: The system shall return a single income record by ID.

### 4. Expense Management
Tracks all money going out, linked to an account and a category.

**Functional Requirements:**
- FR-4.1: The system shall allow registering an expense with amount, date, description, account, and category.
- FR-4.2: The system shall allow updating expense records.
- FR-4.3: The system shall allow deleting expense records.
- FR-4.4: The system shall return a paginated list of expenses, filterable by date range, category, and account.
- FR-4.5: The system shall return a single expense record by ID.

### 5. Expense Item Management
Allows breaking down an expense into individual line items.

**Functional Requirements:**
- FR-5.1: The system shall allow adding expense items to an existing expense, each with name, quantity, and unit price.
- FR-5.2: The system shall allow updating an expense item.
- FR-5.3: The system shall allow deleting an expense item.
- FR-5.4: The system shall return all items belonging to a given expense (paginated).
- FR-5.5: The system shall return a single expense item by ID.

### 6. Budget Management
Defines spending limits per category for a given period.

**Functional Requirements:**
- FR-6.1: The system shall allow creating a budget with a category, period (start and end date), and limit amount.
- FR-6.2: The system shall allow updating a budget.
- FR-6.3: The system shall allow deleting a budget.
- FR-6.4: The system shall return a paginated list of budgets, filterable by category and period.
- FR-6.5: The system shall return a single budget by ID.

### 7. Pagination
All list endpoints must support pagination.

**Functional Requirements:**
- FR-7.1: All GET list endpoints shall accept `page` and `pageSize` query parameters.
- FR-7.2: All paginated responses shall include total record count, current page, page size, and data array.

### 8. Standard API Response
All API responses must follow a consistent structure.

**Functional Requirements:**
- FR-8.1: All responses shall include `statusCode`, `message`, and `error` fields alongside response data.
- FR-8.2: Validation errors shall return HTTP 400 with a list of error messages.
- FR-8.3: Not found resources shall return HTTP 404.
- FR-8.4: Server failures shall return HTTP 500.

## User Experience

**Primary consumer:** Web front-end and personal finance dashboard applications.

**Main flows:**
1. Set up accounts and categories before recording transactions.
2. Record incomes and expenses daily, linked to the correct account and category.
3. Break down large expenses using expense items.
4. Set budgets per category to control spending.
5. Query paginated and filtered lists to populate dashboard charts and tables.

**API UX considerations:**
- Consistent, predictable JSON response envelope across all endpoints.
- Meaningful error messages to support front-end form validation.
- Filter parameters are optional; absence returns all records (paginated).

## High-Level Technical Constraints

- The API must be built on **.NET** and persisted in **PostgreSQL**.
- All API responses must return within **500ms** under normal operating conditions.
- The API must handle **concurrent requests** reliably without data corruption.
- No external integrations are required.
- No authentication or authorization layer is implemented.
- All responses must follow the standardized envelope: `{ "statusCode": int, "message": string, "error": [] }`.
- The API is a single-user system; no multi-tenancy or data isolation is required.

## Out of Scope

- User authentication and authorization (login, JWT, OAuth).
- Multi-user support or data isolation between users.
- External integrations (bank feeds, payment gateways, currency conversion).
- Reporting or analytics endpoints (summaries, charts, aggregations).
- Real-time notifications or webhooks.
- Mobile application or front-end implementation.
- Soft delete or audit log functionality.
