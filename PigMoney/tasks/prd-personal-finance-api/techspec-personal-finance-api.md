# Technical Specification

## Executive Summary

The Personal Finance API implements a RESTful service on .NET 10.0 and PostgreSQL, structured in four layers: API, Application, Domain, and Repository. The architecture leverages Entity Framework Core with the Repository pattern, FluentValidation for input validation, and custom Result<T> pattern for error handling. All entities inherit from a BaseEntity abstract class with int IDs (auto-increment), CreatedAt, and UpdatedAt timestamps. The API layer uses endpoint routing through dedicated route classes in a /routes folder, with middleware-based automatic response wrapping to enforce the standard response format (error[], message, statusCode). Services are organized per entity (6 separate services), repositories implement one-per-aggregate pattern, and soft deletes preserve data integrity for referenced entities. All list endpoints enforce pagination (default 50 items/page), filtering uses indexed columns for performance, and expense items validation occurs at both application and database levels. The implementation prioritizes horizontal scalability, sub-300ms response times, and 10,000 concurrent user support through stateless design and connection pooling.

## System Architecture

### Components Overview

**New Components:**

1. **Domain Layer (./src/Domain/)**
   - BaseEntity abstract class with Id, CreatedAt, UpdatedAt
   - Entity models: Expense, Income, Category, Budget, Account, ExpenseItem
   - Enums: TransactionType, AccountType
   - Custom Result<T> and Result classes for operation outcomes
   - IRepository<T> base interface with IAggregateRoot marker
   - All entities marked as aggregate roots via IAggregateRoot interface

2. **Repository Layer (./src/Repository/)**
   - DbContext configuration with entity mappings
   - Repository implementations for each aggregate (6 repositories)
   - Database migrations for schema creation
   - Entity configuration classes for Fluent API mappings
   - Connection management and pooling configuration
   - Query optimization with includes and filtering

3. **Application Layer (./src/Application/)**
   - Service interfaces and implementations (6 services)
   - DTOs for requests and responses organized by entity
   - FluentValidation validators for all request DTOs
   - Business logic for expense items sum validation
   - Soft delete logic for categories and accounts
   - Mapping logic between entities and DTOs

4. **API Layer (./src/API/)**
   - Route classes in ./routes folder for each entity
   - ResponseWrapperMiddleware for standardized responses
   - ExceptionHandlerMiddleware for global error handling
   - Program.cs with service registrations and middleware pipeline
   - API configuration and CORS setup
   - Logging configuration with Microsoft.Extensions.Logging

**Modified Components:**
- Program.cs: Add EF Core, repositories, services, validators, and middleware
- API.csproj: Add FluentValidation.AspNetCore, Npgsql.EntityFrameworkCore.PostgreSQL packages
- Repository.csproj: Add EF Core packages
- Application.csproj: Add FluentValidation package

**Relationships and Data Flow:**
```
HTTP Request → Middleware (Logging) → Route Handler → FluentValidation
    ↓
Service Layer (Business Logic) → Repository (Data Access) → EF Core → PostgreSQL
    ↓
Result<T> → Response Wrapper Middleware → Standard JSON Response
```

- API routes depend on Application services (IServices)
- Application services depend on Repository interfaces (IRepositories) and Domain entities
- Repositories depend on EF Core DbContext and Domain entities
- Domain layer is independent (no external dependencies)
- Middleware wraps all responses before sending to client

## Implementation Design

### Main Interfaces

```csharp
public interface IRepository<T> where T : IAggregateRoot
{
    Task<Result<T>> GetByIdAsync(int id);
    Task<Result<IEnumerable<T>>> GetAllAsync(int page, int pageSize);
    Task<Result<T>> AddAsync(T entity);
    Task<Result<T>> UpdateAsync(T entity);
    Task<Result> DeleteAsync(int id);
    Task<Result> SaveChangesAsync();
}

public interface IExpenseService
{
    Task<Result<ExpenseResponse>> CreateExpenseAsync(CreateExpenseRequest request);
    Task<Result<ExpenseResponse>> GetExpenseByIdAsync(int id);
    Task<Result<PaginatedList<ExpenseResponse>>> GetExpensesAsync(ExpenseFilterParams filters);
    Task<Result<ExpenseResponse>> UpdateExpenseAsync(int id, UpdateExpenseRequest request);
    Task<Result> DeleteExpenseAsync(int id);
}

public interface IIncomeService
{
    Task<Result<IncomeResponse>> CreateIncomeAsync(CreateIncomeRequest request);
    Task<Result<IncomeResponse>> GetIncomeByIdAsync(int id);
    Task<Result<PaginatedList<IncomeResponse>>> GetIncomesAsync(IncomeFilterParams filters);
    Task<Result<IncomeResponse>> UpdateIncomeAsync(int id, UpdateIncomeRequest request);
    Task<Result> DeleteIncomeAsync(int id);
}

public interface ICategoryService
{
    Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request);
    Task<Result<PaginatedList<CategoryResponse>>> GetAllCategoriesAsync(int page, int pageSize);
    Task<Result<CategoryResponse>> GetCategoryByIdAsync(int id);
    Task<Result<CategoryResponse>> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
    Task<Result> DeleteCategoryAsync(int id);
}

public interface IAccountService
{
    Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request);
    Task<Result<PaginatedList<AccountResponse>>> GetAllAccountsAsync(int page, int pageSize);
    Task<Result<AccountResponse>> GetAccountByIdAsync(int id);
    Task<Result<AccountResponse>> UpdateAccountAsync(int id, UpdateAccountRequest request);
    Task<Result> DeleteAccountAsync(int id);
}

public interface IBudgetService
{
    Task<Result<BudgetResponse>> CreateBudgetAsync(CreateBudgetRequest request);
    Task<Result<PaginatedList<BudgetResponse>>> GetAllBudgetsAsync(int page, int pageSize);
    Task<Result<BudgetResponse>> GetBudgetByIdAsync(int id);
    Task<Result<PaginatedList<BudgetResponse>>> GetBudgetsByCategoryAsync(int categoryId, int page, int pageSize);
    Task<Result<BudgetResponse>> UpdateBudgetAsync(int id, UpdateBudgetRequest request);
    Task<Result> DeleteBudgetAsync(int id);
}

public interface IExpenseItemService
{
    Task<Result<ExpenseItemResponse>> CreateExpenseItemAsync(CreateExpenseItemRequest request);
    Task<Result<PaginatedList<ExpenseItemResponse>>> GetExpenseItemsByExpenseIdAsync(int expenseId, int page, int pageSize);
    Task<Result<ExpenseItemResponse>> UpdateExpenseItemAsync(int id, UpdateExpenseItemRequest request);
    Task<Result> DeleteExpenseItemAsync(int id);
}
```

### Data Models

**Domain Entities:**

```csharp
public abstract class BaseEntity : IAggregateRoot
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}

public class Expense : BaseEntity
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public int AccountId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    public Category Category { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public List<ExpenseItem> ExpenseItems { get; set; } = [];
}

public class Income : BaseEntity
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public int AccountId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    public Category Category { get; set; } = null!;
    public Account Account { get; set; } = null!;
}

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    
    public List<Expense> Expenses { get; set; } = [];
    public List<Income> Incomes { get; set; } = [];
    public List<Budget> Budgets { get; set; } = [];
}

public enum TransactionType
{
    Expense = 0,
    Income = 1
}

public class Budget : BaseEntity
{
    public int CategoryId { get; set; }
    public decimal LimitAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public Category Category { get; set; } = null!;
}

public class Account : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal InitialBalance { get; set; }
    
    public List<Expense> Expenses { get; set; } = [];
    public List<Income> Incomes { get; set; } = [];
}

public enum AccountType
{
    Checking = 0,
    Savings = 1,
    CreditCard = 2,
    Cash = 3
}

public class ExpenseItem : BaseEntity
{
    public int ExpenseId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    
    public Expense Expense { get; set; } = null!;
    public Category? Category { get; set; }
}

public interface IAggregateRoot { }

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Error { get; }
    
    public Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string error) => new(false, default, error);
}

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    
    public Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
}
```

**Request/Response DTOs:**

```csharp
public record CreateExpenseRequest(
    decimal Amount,
    DateTime Date,
    int CategoryId,
    int AccountId,
    string? Description,
    string? Notes
);

public record UpdateExpenseRequest(
    decimal? Amount,
    DateTime? Date,
    int? CategoryId,
    int? AccountId,
    string? Description,
    string? Notes
);

public record ExpenseResponse(
    int Id,
    decimal Amount,
    DateTime Date,
    int CategoryId,
    string CategoryName,
    int AccountId,
    string AccountName,
    string Description,
    string Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ExpenseFilterParams(
    DateTime? StartDate,
    DateTime? EndDate,
    int? CategoryId,
    int? AccountId,
    int Page = 1,
    int PageSize = 50
);

public record PaginatedList<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record StandardApiResponse<T>(
    T? Data,
    string[] Error,
    string Message,
    int StatusCode
);
```

**Database Schema:**

```sql
CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type INTEGER NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE accounts (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type INTEGER NOT NULL,
    initial_balance DECIMAL(18, 2) NOT NULL DEFAULT 0,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE expenses (
    id SERIAL PRIMARY KEY,
    amount DECIMAL(18, 2) NOT NULL CHECK (amount >= 0),
    date TIMESTAMP NOT NULL,
    category_id INTEGER NOT NULL REFERENCES categories(id),
    account_id INTEGER NOT NULL REFERENCES accounts(id),
    description VARCHAR(500) NOT NULL DEFAULT '',
    notes TEXT NOT NULL DEFAULT '',
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE incomes (
    id SERIAL PRIMARY KEY,
    amount DECIMAL(18, 2) NOT NULL CHECK (amount >= 0),
    date TIMESTAMP NOT NULL,
    category_id INTEGER NOT NULL REFERENCES categories(id),
    account_id INTEGER NOT NULL REFERENCES accounts(id),
    description VARCHAR(500) NOT NULL DEFAULT '',
    notes TEXT NOT NULL DEFAULT '',
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE budgets (
    id SERIAL PRIMARY KEY,
    category_id INTEGER NOT NULL REFERENCES categories(id),
    limit_amount DECIMAL(18, 2) NOT NULL CHECK (limit_amount >= 0),
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CHECK (end_date > start_date)
);

CREATE TABLE expense_items (
    id SERIAL PRIMARY KEY,
    expense_id INTEGER NOT NULL REFERENCES expenses(id) ON DELETE CASCADE,
    amount DECIMAL(18, 2) NOT NULL CHECK (amount >= 0),
    description VARCHAR(200) NOT NULL,
    category_id INTEGER REFERENCES categories(id),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE FUNCTION check_expense_items_sum()
RETURNS TRIGGER AS $$
DECLARE
    expense_amount DECIMAL(18, 2);
    items_sum DECIMAL(18, 2);
BEGIN
    SELECT amount INTO expense_amount FROM expenses WHERE id = NEW.expense_id AND is_deleted = FALSE;
    SELECT COALESCE(SUM(amount), 0) INTO items_sum FROM expense_items WHERE expense_id = NEW.expense_id AND is_deleted = FALSE;
    
    IF items_sum > expense_amount THEN
        RAISE EXCEPTION 'Sum of expense items cannot exceed parent expense amount';
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER validate_expense_items_sum
AFTER INSERT OR UPDATE ON expense_items
FOR EACH ROW
EXECUTE FUNCTION check_expense_items_sum();

CREATE INDEX idx_expenses_date ON expenses(date) WHERE is_deleted = FALSE;
CREATE INDEX idx_expenses_category ON expenses(category_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_expenses_account ON expenses(account_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_incomes_date ON incomes(date) WHERE is_deleted = FALSE;
CREATE INDEX idx_incomes_category ON incomes(category_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_incomes_account ON incomes(account_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_budgets_category ON budgets(category_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_budgets_dates ON budgets(start_date, end_date) WHERE is_deleted = FALSE;
CREATE INDEX idx_expense_items_expense ON expense_items(expense_id) WHERE is_deleted = FALSE;
```

### API Endpoints

**Expense Routes (`./routes/ExpenseRoutes.cs`):**
- `POST /api/v1/expenses` - Create expense (201)
- `GET /api/v1/expenses/{id}` - Get expense by ID (200/404)
- `GET /api/v1/expenses?startDate&endDate&categoryId&accountId&page&pageSize` - Get filtered expenses (200)
- `PUT /api/v1/expenses/{id}` - Update expense (200/404)
- `DELETE /api/v1/expenses/{id}` - Soft delete expense (200/404)

**Income Routes (`./routes/IncomeRoutes.cs`):**
- `POST /api/v1/incomes` - Create income (201)
- `GET /api/v1/incomes/{id}` - Get income by ID (200/404)
- `GET /api/v1/incomes?startDate&endDate&categoryId&accountId&page&pageSize` - Get filtered incomes (200)
- `PUT /api/v1/incomes/{id}` - Update income (200/404)
- `DELETE /api/v1/incomes/{id}` - Soft delete income (200/404)

**Category Routes (`./routes/CategoryRoutes.cs`):**
- `POST /api/v1/categories` - Create category (201)
- `GET /api/v1/categories?page&pageSize` - Get all categories (200)
- `GET /api/v1/categories/{id}` - Get category by ID (200/404)
- `PUT /api/v1/categories/{id}` - Update category (200/404)
- `DELETE /api/v1/categories/{id}` - Soft delete category with validation (200/400/404)

**Budget Routes (`./routes/BudgetRoutes.cs`):**
- `POST /api/v1/budgets` - Create budget (201)
- `GET /api/v1/budgets?page&pageSize` - Get all budgets (200)
- `GET /api/v1/budgets/{id}` - Get budget by ID (200/404)
- `GET /api/v1/budgets/by-category/{categoryId}?page&pageSize` - Get budgets by category (200)
- `PUT /api/v1/budgets/{id}` - Update budget (200/404)
- `DELETE /api/v1/budgets/{id}` - Soft delete budget (200/404)

**Account Routes (`./routes/AccountRoutes.cs`):**
- `POST /api/v1/accounts` - Create account (201)
- `GET /api/v1/accounts?page&pageSize` - Get all accounts (200)
- `GET /api/v1/accounts/{id}` - Get account by ID (200/404)
- `PUT /api/v1/accounts/{id}` - Update account (200/404)
- `DELETE /api/v1/accounts/{id}` - Soft delete account with validation (200/400/404)

**Expense Item Routes (`./routes/ExpenseItemRoutes.cs`):**
- `POST /api/v1/expense-items` - Create expense item with validation (201/400)
- `GET /api/v1/expense-items?expenseId&page&pageSize` - Get expense items by expense (200)
- `PUT /api/v1/expense-items/{id}` - Update expense item with validation (200/400/404)
- `DELETE /api/v1/expense-items/{id}` - Soft delete expense item (200/404)

## Integration Points

**PostgreSQL Database:**
- Connection via Npgsql.EntityFrameworkCore.PostgreSQL
- Connection string from appsettings.json (no sensitive data in code)
- Connection pooling: Min=5, Max=100 connections
- Command timeout: 30 seconds
- Retry policy: 3 attempts with exponential backoff

**Entity Framework Core Configuration:**
- DbContext with entity configurations via Fluent API
- Migration-based schema management
- Lazy loading disabled (explicit includes only)
- Query tracking disabled for read-only operations
- Global query filter for soft deletes: `IsDeleted == false`

**FluentValidation Integration:**
- Automatic validation before service layer execution
- Validators registered in DI container
- Custom validation rules for business logic
- Error messages returned in standard format

**Error Handling Strategy:**
- Result<T> pattern for expected failures (business validation, not found)
- Global exception handler middleware for unexpected errors
- Logged errors include correlation ID for tracing
- Client receives sanitized error messages (no stack traces)
- Different status codes: 400 (validation), 404 (not found), 500 (server error)

## Testing Approach

### Unit Tests

**Service Layer Tests:**
- Mock IRepository<T> implementations
- Test business logic validation (expense items sum validation)
- Test soft delete logic with dependency checks
- Test Result<T> success and failure paths
- Test DTO mapping logic

**Repository Layer Tests:**
- Use EF Core InMemory provider for isolation
- Test CRUD operations
- Test filtering and pagination logic
- Test soft delete query filtering
- Test include/eager loading functionality

**Critical Test Scenarios:**
- Creating expense with valid data returns Success with ExpenseResponse
- Creating expense with non-existent category returns Failure
- Soft deleting category with expenses returns Failure
- Expense items sum exceeding parent amount returns Failure
- Filtering expenses by date range returns correct subset
- Pagination returns correct page count and total items

### Integration Tests

**Full Stack Tests:**
- Test complete flow: Route → Service → Repository → Database
- Use test PostgreSQL container (Testcontainers)
- Test middleware pipeline (response wrapping, exception handling)
- Test FluentValidation integration with routes
- Test concurrent operations (multiple users updating same entity)

**Test Data Requirements:**
- Seed categories for expense and income types
- Seed test accounts (checking, savings, cash)
- Create test expenses with various dates and amounts
- Test edge cases: soft deleted records, orphaned items

## Development Sequencing

### Technical Dependencies

**Phase 1: Foundation**
1. Install NuGet packages: EF Core, Npgsql provider, FluentValidation
2. Create BaseEntity and IAggregateRoot in Domain
3. Create Result<T> and Result classes in Domain
4. Configure connection string in appsettings.json

**Phase 2: Domain Entities**
1. Create all entity classes inheriting from BaseEntity
2. Create enums (TransactionType, AccountType)
3. Configure navigation properties and relationships

**Phase 3: Repository Layer**
1. Create DbContext with entity configurations
2. Create entity configuration classes (Fluent API)
3. Generate and apply initial migration
4. Create IRepository<T> interface
5. Create generic Repository<T> base implementation
6. Create specific repository implementations if needed

**Phase 4: Application Services**
1. Create service interfaces in Application layer
2. Create request/response DTOs
3. Create FluentValidation validators for DTOs
4. Implement service classes with business logic
5. Implement soft delete validation logic
6. Implement expense items sum validation

**Phase 5: API Routes**
1. Create route classes in ./routes folder
2. Map endpoints to service methods
3. Implement response mapping
4. Register routes in Program.cs

**Phase 6: Middleware**
1. Create ResponseWrapperMiddleware
2. Create ExceptionHandlerMiddleware
3. Configure middleware pipeline in Program.cs
4. Configure logging with Microsoft.Extensions.Logging

**Phase 7: Testing**
1. Write unit tests for services
2. Write repository tests with InMemory provider
3. Write integration tests with test database
4. Performance testing with concurrent users

**Blocking Dependencies:**
- PostgreSQL database server (v12+)
- .NET 10.0 SDK
- Entity Framework Core 10.x
- Npgsql.EntityFrameworkCore.PostgreSQL 10.x
- FluentValidation.AspNetCore 11.x

## Monitoring and Observability

**Logging Strategy:**
- Use Microsoft.Extensions.Logging throughout
- Structured logging with scopes for request correlation
- Log levels: Information (operations), Warning (validation failures), Error (exceptions)

**Key Metrics:**
- Request duration per endpoint (histogram)
- Total requests per endpoint (counter)
- Database query duration (histogram)
- Active database connections (gauge)
- Failed requests by error type (counter)

**Log Examples:**
```csharp
_logger.LogInformation("Creating expense for account {AccountId} with amount {Amount}", 
    request.AccountId, request.Amount);

_logger.LogWarning("Category {CategoryId} not found for expense creation", 
    request.CategoryId);

_logger.LogError(exception, "Failed to create expense. Request: {@Request}", 
    request);
```

## Technical Considerations

### Key Decisions

**1. Entity Framework Core vs Dapper**
- **Choice**: Entity Framework Core
- **Justification**: Repository RULES.md mandates EF Core, provides change tracking, migration support, and LINQ queries
- **Trade-offs**: Slightly more overhead vs. better maintainability and type safety

**2. Int vs Guid for Entity IDs**
- **Choice**: Int with auto-increment
- **Justification**: Domain RULES.md specifies int IDs, sufficient for single-database deployment, better performance
- **Trade-offs**: Less suitable for distributed systems vs. simpler implementation

**3. Soft Delete Pattern**
- **Choice**: IsDeleted flag with global query filters
- **Justification**: Preserves data integrity, allows audit trails, prevents deletion of referenced entities
- **Trade-offs**: More complex queries vs. data preservation and safety

**4. Custom Result<T> vs Library**
- **Choice**: Custom Result<T> implementation in Domain
- **Justification**: No external dependencies in Domain layer, full control over implementation, aligns with project rules
- **Trade-offs**: Maintain custom code vs. complete control

**5. Route Classes vs Controllers**
- **Choice**: Route extension methods in ./routes folder
- **Justification**: API RULES.md mandates routes in ./routes, cleaner separation, follows minimal API pattern
- **Trade-offs**: Less familiar pattern vs. better organization

**6. Middleware-based Response Wrapping**
- **Choice**: ResponseWrapperMiddleware automatically wraps all responses
- **Justification**: Ensures consistent format, reduces duplication, centralizes response transformation
- **Trade-offs**: Less control per endpoint vs. guaranteed consistency

### Known Risks

**1. Expense Items Sum Validation Performance**
- **Challenge**: Database trigger fires on every insert/update, may impact performance under high load
- **Mitigation**: Application-level validation first (fail fast), database constraint as final safeguard, index on expense_id
- **Research Needed**: Load testing to measure trigger overhead vs. benefit

**2. Soft Delete Query Performance**
- **Challenge**: Global query filter adds WHERE is_deleted = false to all queries, may impact performance
- **Mitigation**: Partial indexes on is_deleted = false, regular index maintenance, consider composite indexes
- **Research Needed**: Query execution plan analysis for complex filtered queries

**3. Connection Pool Exhaustion**
- **Challenge**: 10,000 concurrent users may exhaust connection pool
- **Mitigation**: Connection pooling (Max=100), async/await throughout, short-lived connections, connection timeout monitoring
- **Research Needed**: Load testing to determine optimal pool size and timeout settings

**4. Date Range Query Performance**
- **Challenge**: Large date ranges on expenses/incomes may be slow with large datasets
- **Mitigation**: Mandatory pagination, indexed date columns, consider table partitioning by date for future
- **Research Needed**: Performance testing with 1M+ records, partitioning strategy evaluation

**5. FluentValidation Async Limitations**
- **Challenge**: Async validation rules may not work seamlessly with automatic validation
- **Mitigation**: Use manual validation in services for complex async rules, keep route-level validation synchronous
- **Research Needed**: Test async validation scenarios with EF Core database lookups

### Compliance with Standards

**RULES.md:**
- ✅ All classes use primary constructors
- ✅ String.Equals for string comparisons
- ✅ List properties instantiated at declaration
- ✅ File header comment on all .cs files
- ✅ Microsoft.Extensions.Logging (no Console.WriteLine)
- ✅ Result pattern preferred over exceptions
- ✅ Async/await for all I/O operations
- ✅ Never return or pass null (use string.Empty, empty lists)
- ✅ var only when type is obvious
- ✅ Methods start with verbs
- ✅ Maximum 3 parameters (use DTOs)
- ✅ Private or private readonly fields
- ✅ Maximum 2 levels of if/else nesting
- ✅ No flag parameters
- ✅ LINQ for collection operations
- ✅ Dependency injection throughout

**API RULES.md:**
- ✅ Routes in ./routes folder
- ✅ DTOs for all requests (located in Application)
- ✅ FluentValidation for request validation
- ✅ Standard response format (error[], message, statusCode)
- ✅ No sensitive data in code or launchSettings.json

**Domain RULES.md:**
- ✅ BaseEntity with Id (int), CreatedAt, UpdatedAt
- ✅ All entities inherit from BaseEntity
- ✅ One entity per table
- ✅ String properties default to string.Empty

**Repository RULES.md:**
- ✅ Entity Framework Core for data access
- ✅ PostgreSQL database
- ✅ Migrations for schema management
- ✅ Entity configurations with Fluent API
- ✅ Repository per entity
- ✅ No abstractions in Repository (abstractions in Domain)

### Relevant and Dependent Files

**Existing Files:**
- `/src/Domain/Class1.cs` - Remove (replaced by entity models)
- `/src/Application/Class1.cs` - Remove (replaced by services/DTOs)
- `/src/Repository/Class1.cs` - Remove (replaced by repositories)
- `/src/API/Program.cs` - Modify (add DI, middleware, routes)
- `/RULES.md` - Reference for coding standards
- `/src/API/RULES.md` - Reference for API standards
- `/src/Domain/RULES.md` - Reference for domain standards
- `/src/Repository/RULES.md` - Reference for repository standards

**Files to Create:**

**Domain Layer:**
- `/src/Domain/Entities/BaseEntity.cs`
- `/src/Domain/Entities/Expense.cs`
- `/src/Domain/Entities/Income.cs`
- `/src/Domain/Entities/Category.cs`
- `/src/Domain/Entities/Budget.cs`
- `/src/Domain/Entities/Account.cs`
- `/src/Domain/Entities/ExpenseItem.cs`
- `/src/Domain/Enums/TransactionType.cs`
- `/src/Domain/Enums/AccountType.cs`
- `/src/Domain/Interfaces/IAggregateRoot.cs`
- `/src/Domain/Interfaces/IRepository.cs`
- `/src/Domain/Common/Result.cs`

**Repository Layer:**
- `/src/Repository/Data/AppDbContext.cs`
- `/src/Repository/Configurations/ExpenseConfiguration.cs`
- `/src/Repository/Configurations/IncomeConfiguration.cs`
- `/src/Repository/Configurations/CategoryConfiguration.cs`
- `/src/Repository/Configurations/BudgetConfiguration.cs`
- `/src/Repository/Configurations/AccountConfiguration.cs`
- `/src/Repository/Configurations/ExpenseItemConfiguration.cs`
- `/src/Repository/Repositories/Repository.cs`
- `/src/Repository/Repositories/ExpenseRepository.cs`
- `/src/Repository/Repositories/IncomeRepository.cs`
- `/src/Repository/Repositories/CategoryRepository.cs`
- `/src/Repository/Repositories/BudgetRepository.cs`
- `/src/Repository/Repositories/AccountRepository.cs`
- `/src/Repository/Repositories/ExpenseItemRepository.cs`

**Application Layer:**
- `/src/Application/Services/IExpenseService.cs`
- `/src/Application/Services/ExpenseService.cs`
- `/src/Application/Services/IIncomeService.cs`
- `/src/Application/Services/IncomeService.cs`
- `/src/Application/Services/ICategoryService.cs`
- `/src/Application/Services/CategoryService.cs`
- `/src/Application/Services/IBudgetService.cs`
- `/src/Application/Services/BudgetService.cs`
- `/src/Application/Services/IAccountService.cs`
- `/src/Application/Services/AccountService.cs`
- `/src/Application/Services/IExpenseItemService.cs`
- `/src/Application/Services/ExpenseItemService.cs`
- `/src/Application/DTOs/Expenses/CreateExpenseRequest.cs`
- `/src/Application/DTOs/Expenses/UpdateExpenseRequest.cs`
- `/src/Application/DTOs/Expenses/ExpenseResponse.cs`
- `/src/Application/DTOs/Expenses/ExpenseFilterParams.cs`
- `/src/Application/DTOs/Incomes/CreateIncomeRequest.cs`
- `/src/Application/DTOs/Incomes/UpdateIncomeRequest.cs`
- `/src/Application/DTOs/Incomes/IncomeResponse.cs`
- `/src/Application/DTOs/Incomes/IncomeFilterParams.cs`
- `/src/Application/DTOs/Categories/CreateCategoryRequest.cs`
- `/src/Application/DTOs/Categories/UpdateCategoryRequest.cs`
- `/src/Application/DTOs/Categories/CategoryResponse.cs`
- `/src/Application/DTOs/Budgets/CreateBudgetRequest.cs`
- `/src/Application/DTOs/Budgets/UpdateBudgetRequest.cs`
- `/src/Application/DTOs/Budgets/BudgetResponse.cs`
- `/src/Application/DTOs/Accounts/CreateAccountRequest.cs`
- `/src/Application/DTOs/Accounts/UpdateAccountRequest.cs`
- `/src/Application/DTOs/Accounts/AccountResponse.cs`
- `/src/Application/DTOs/ExpenseItems/CreateExpenseItemRequest.cs`
- `/src/Application/DTOs/ExpenseItems/UpdateExpenseItemRequest.cs`
- `/src/Application/DTOs/ExpenseItems/ExpenseItemResponse.cs`
- `/src/Application/DTOs/Common/PaginatedList.cs`
- `/src/Application/Validators/CreateExpenseRequestValidator.cs`
- `/src/Application/Validators/UpdateExpenseRequestValidator.cs`
- `/src/Application/Validators/CreateIncomeRequestValidator.cs`
- `/src/Application/Validators/UpdateIncomeRequestValidator.cs`
- `/src/Application/Validators/CreateCategoryRequestValidator.cs`
- `/src/Application/Validators/UpdateCategoryRequestValidator.cs`
- `/src/Application/Validators/CreateBudgetRequestValidator.cs`
- `/src/Application/Validators/UpdateBudgetRequestValidator.cs`
- `/src/Application/Validators/CreateAccountRequestValidator.cs`
- `/src/Application/Validators/UpdateAccountRequestValidator.cs`
- `/src/Application/Validators/CreateExpenseItemRequestValidator.cs`
- `/src/Application/Validators/UpdateExpenseItemRequestValidator.cs`

**API Layer:**
- `/src/API/Routes/ExpenseRoutes.cs`
- `/src/API/Routes/IncomeRoutes.cs`
- `/src/API/Routes/CategoryRoutes.cs`
- `/src/API/Routes/BudgetRoutes.cs`
- `/src/API/Routes/AccountRoutes.cs`
- `/src/API/Routes/ExpenseItemRoutes.cs`
- `/src/API/Middleware/ResponseWrapperMiddleware.cs`
- `/src/API/Middleware/ExceptionHandlerMiddleware.cs`
- `/src/API/Models/StandardApiResponse.cs`
