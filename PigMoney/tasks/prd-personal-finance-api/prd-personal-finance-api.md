# Product Requirements Document (PRD)
# Personal Finance API

## Overview

The Personal Finance API is a comprehensive backend service designed to help individual consumers manage their personal finances effectively. This API enables users to track their expenses and income, organize transactions by categories, set and monitor budgets, manage multiple financial accounts, and handle recurring transactions. The primary value proposition is providing users with a centralized, structured way to gain control over their financial life through tracking where money comes from and where it goes, preventing overspending through budget management, and maintaining organized financial records.

## Objectives

- **Enable comprehensive financial tracking**: Provide users with the ability to record and categorize 100% of their financial transactions (expenses and income)
- **Support proactive budget management**: Allow users to create budgets and track spending against defined limits to prevent overspending
- **Deliver high-performance operations**: Maintain API response times under 300ms for all endpoints
- **Ensure scalability**: Support 10,000 concurrent users without performance degradation
- **Provide reliable data management**: Enable CRUD operations for all financial entities with data integrity and validation
- **Maintain simplicity**: Focus on core financial management without complex analytics or third-party integrations

## User Stories

**As an individual consumer**, I want to record my daily expenses so that I can track where my money is being spent.

**As an individual consumer**, I want to categorize my transactions (groceries, utilities, entertainment, etc.) so that I can understand my spending patterns by category.

**As an individual consumer**, I want to record my income sources so that I have a complete picture of my financial inflows and outflows.

**As an individual consumer**, I want to create monthly budgets for different spending categories so that I can control my expenses and avoid overspending.

**As an individual consumer**, I want to manage multiple accounts or wallets (checking account, savings, credit card, cash) so that I can track balances across different financial instruments.

**As an individual consumer**, I want to set up recurring transactions (monthly rent, subscriptions, salary) so that I don't have to manually enter repetitive transactions.

**As an individual consumer**, I want to manage individual expense items within a transaction so that I can break down complex purchases (like grocery shopping) into detailed line items.

**As an individual consumer**, I want to update or delete my financial records so that I can correct mistakes or remove erroneous entries.

## Core Features

### 1. Expense Management

**What it does**: Allows users to create, read, update, and delete expense records with detailed information including amount, date, category, description, and associated account.

**Why it is important**: Expenses represent the primary data point for understanding spending behavior and managing personal finances effectively.

**How it works**: Users can submit expense data through API endpoints, which validates and persists the information to the database. Expenses can be queried individually or in lists with filtering capabilities.

**Functional Requirements**:
1. Create expense records with mandatory fields: amount, date, category ID, account ID
2. Create expense records with optional fields: description, notes
3. Retrieve a single expense by unique identifier
4. Retrieve a list of expenses with filtering by date range, category, and account
5. Update existing expense records with validation
6. Delete expense records with proper error handling
7. Associate expense items (line items) with parent expense records

### 2. Income Management

**What it does**: Enables users to record and manage income transactions from various sources (salary, freelance, investments, etc.).

**Why it is important**: Income tracking provides the complete financial picture and enables users to understand their net cash flow.

**How it works**: Similar to expense management, income records are created with relevant details and can be queried, updated, or deleted as needed.

**Functional Requirements**:
1. Create income records with mandatory fields: amount, date, source/category, account ID
2. Create income records with optional fields: description, notes
3. Retrieve a single income record by unique identifier
4. Retrieve a list of income records with filtering capabilities
5. Update existing income records
6. Delete income records

### 3. Category Management

**What it does**: Provides a system to organize and classify transactions into meaningful groups (groceries, utilities, entertainment, salary, etc.).

**Why it is important**: Categories enable users to understand spending patterns and make informed financial decisions based on transaction types.

**How it works**: Users can define custom categories or use predefined ones. Each transaction must be associated with a category.

**Functional Requirements**:
1. Create custom categories with name and type (expense or income)
2. Retrieve all available categories
3. Retrieve a single category by identifier
4. Update category information
5. Delete categories (with validation to prevent deletion if transactions exist)
6. Support categorization of both income and expense transactions

### 4. Budget Management

**What it does**: Allows users to set spending limits for specific categories over defined time periods (typically monthly).

**Why it is important**: Budgets are essential for proactive financial management and preventing overspending.

**How it works**: Users define budget amounts for categories and time periods. The system stores these limits for reference.

**Functional Requirements**:
1. Create budgets with category, amount limit, and time period (start/end date)
2. Retrieve all budgets
3. Retrieve budgets by category or time period
4. Update budget amounts and periods
5. Delete budgets
6. Support multiple active budgets for different categories simultaneously

### 5. Account/Wallet Management

**What it does**: Enables users to manage multiple financial accounts such as bank accounts, credit cards, cash wallets, and savings accounts.

**Why it is important**: Most users have multiple financial instruments, and tracking balances across accounts provides an accurate financial overview.

**How it works**: Users create account records with identifying information. All transactions are linked to specific accounts.

**Functional Requirements**:
1. Create accounts with name, type (checking, savings, credit card, cash), and optional initial balance
2. Retrieve all accounts
3. Retrieve a single account by identifier
4. Update account information
5. Delete accounts (with validation for existing transactions)
6. Associate all transactions with a specific account

### 6. Recurring Transaction Management

**What it does**: Supports the definition of transactions that repeat on a regular schedule (daily, weekly, monthly, yearly).

**Why it is important**: Many financial transactions are recurring (rent, subscriptions, salary), and managing them systematically reduces manual data entry.

**How it works**: Users define recurring transaction templates with frequency, amount, category, and account information.

**Functional Requirements**:
1. Create recurring transaction templates with frequency (daily, weekly, monthly, yearly)
2. Define mandatory fields: amount, category, account, recurrence pattern, start date
3. Define optional fields: end date, description
4. Retrieve all recurring transactions
5. Update recurring transaction details
6. Delete recurring transactions
7. Support both income and expense recurring patterns

### 7. Expense Item Management

**What it does**: Allows detailed breakdown of expenses into individual line items (e.g., breaking down a grocery receipt into bread, milk, vegetables).

**Why it is important**: Detailed itemization provides granular insights into spending and enables more accurate categorization.

**How it works**: Expense items are child records linked to a parent expense transaction, each with its own amount, description, and optional category.

**Functional Requirements**:
1. Create expense items linked to a parent expense
2. Define mandatory fields: amount, description, parent expense ID
3. Define optional fields: item-level category
4. Retrieve all items for a specific expense
5. Update expense item details
6. Delete individual expense items
7. Ensure sum of expense items does not exceed parent expense amount

## User Experience

### User Personas

**Primary Persona: Budget-Conscious Individual**
- Age: 25-45
- Needs: Track spending, stick to budget, understand financial habits
- Technical comfort: Moderate (using through a frontend application)
- Goals: Reduce unnecessary spending, save for specific goals, avoid debt

### Main User Flows

**Recording a Simple Expense**:
1. User initiates expense creation through client application
2. Client sends POST request to /api/expenses with amount, date, category, account
3. API validates data and creates expense record
4. API returns success response with created expense details
5. Client updates UI to reflect new expense

**Creating a Monthly Budget**:
1. User selects budget creation feature
2. Client sends POST request to /api/budgets with category, amount, start/end dates
3. API validates budget data
4. API creates budget record
5. Client displays confirmation and budget summary

**Managing Recurring Bills**:
1. User creates recurring transaction for monthly rent
2. Client sends POST request to /api/recurring-transactions with details and monthly frequency
3. API stores recurring transaction template
4. System maintains template for reference

### UI/UX Considerations

- API responses must be consistent and predictable (standard response format)
- Error messages must be clear and actionable
- All list endpoints should support pagination for performance
- Date formats should be consistent (ISO 8601)
- Validation errors should specify which fields are problematic

### Accessibility Requirements

- API documentation must be clear and comprehensive
- Standard HTTP methods and status codes must be used consistently
- Response times must meet performance targets to support responsive user interfaces
- Error responses must provide sufficient detail for client applications to display helpful messages to users

## High-Level Technical Constraints

### Technology Stack
- .NET platform with C# programming language
- PostgreSQL database for data persistence
- RESTful API architecture following SOLID principles
- Repository pattern for data access layer
- Microsoft.Extensions.Logging for application logging
- No MediatR framework to be used

### API Response Standardization
All API responses must follow this exact format:
```json
{
    "error": [],
    "message": "",
    "statusCode": 201
}
```

### Performance Requirements
- Response time: All API endpoints must respond within 300ms under normal load conditions
- Concurrency: System must support 10,000 concurrent users without performance degradation
- Scalability: Architecture must support horizontal scaling

### Data and Security Considerations
- No passwords or sensitive data should be stored directly in code
- Use environment variables or secure configuration management for sensitive settings
- Data validation on all inputs to prevent invalid data persistence

### Architecture Mandates
- Strict adherence to SOLID principles
- Repository pattern for all data access operations
- Avoid circular dependencies between modules
- No Console.WriteLine() usage; use structured logging framework
- Implement proper dependency injection

### HTTP Status Codes
- 200: Success
- 201: Created
- 400: Bad request (missing or invalid data)
- 404: Resource not found
- 500: Server error

## Out of Scope

The following features and capabilities are explicitly **excluded** from this PRD:

### Authentication and Authorization
- User login/logout functionality
- Password management
- Session management
- Role-based access control
- API authentication tokens or keys
- User registration

### Third-Party Integrations
- Bank account connections
- Automatic transaction imports from financial institutions
- Credit card data feeds
- Payment processing integrations
- External API integrations

### Advanced Features
- Multi-currency support
- Currency conversion
- Investment portfolio tracking
- Loan and debt management
- Tax reporting and calculations
- Financial goal tracking with progress metrics
- Automated savings recommendations

### Analytics and Reporting
- Spending trend analysis
- Budget vs actual comparison reports
- Category spending breakdowns
- Monthly/yearly financial summaries
- Data visualization endpoints
- Predictive analytics
- Anomaly detection

### Mobile and Frontend
- Mobile application development
- Web frontend application
- Mobile notifications
- Dashboard UI

### Code Quality Items Explicitly Excluded
- Inline code comments (code should be self-documenting)

### Future Considerations (Not in Current Scope)
- Multi-user support
- Data export functionality (CSV, PDF)
- Bill reminders and notifications
- Receipt image upload and storage
- Shared budgets or accounts
- Financial advice or recommendations
