# Technical Specification
# Personal Finance Management API

## Executive Summary

The Personal Finance Management API is built on .NET 10 ASP.NET Core following a 4-layer Clean Architecture: **Domain** (entities, enums, interfaces), **Application** (services, DTOs, validators), **Repository** (EF Core + PostgreSQL), and **API** (MVC controllers, middleware, DI). All cross-cutting concerns — response envelope, exception handling, and FluentValidation — are wired at the API layer. Business logic lives exclusively in the Application layer, with the `Result<T>` pattern propagating errors throughout to avoid exception-driven control flow. Each of the 6 domain resources (Account, Category, Income, Expense, ExpenseItem, Budget) follows the same vertical slice: entity → EF configuration → repository → service → controller.

---

## System Architecture

### Components Overview

| Layer | Project | Responsibility |
|---|---|---|
| **Domain** | `Domain.csproj` | `BaseEntity`, 6 entities, `AccountType` enum, `Result<T>`, `IRepository<T>` + 6 resource interfaces |
| **Application** | `Application.csproj` | 6 service interfaces + implementations, Request/Response DTOs, FluentValidation validators, `PaginatedList<T>` |
| **Repository** | `Repository.csproj` | `AppDbContext`, `IEntityTypeConfiguration<T>` per entity, 6 repository implementations, EF migrations |
| **API** | `API.csproj` | 6 MVC controllers (`[ApiController]`, `/api/v1/`), `ResponseWrapperMiddleware`, `ExceptionHandlerMiddleware`, `Program.cs` (DI + pipeline) |

**Data flow:** `Controller` → `IService` (Application) → `IRepository` (Repository) → EF Core → PostgreSQL. Responses flow back as `Result<T>`, mapped to HTTP codes at the controller.

---

## Implementation Design

### Main Interfaces

```csharp
// Domain/Interfaces/IRepository.cs
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
    Task<int> CountAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
}

// Application/Services/IAccountService.cs  (pattern repeated for all 6 resources)
public interface IAccountService
{
    Task<Result<PaginatedList<AccountResponse>>> GetAllAsync(int page, int pageSize);
    Task<Result<AccountResponse>> GetByIdAsync(int id);
    Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request);
    Task<Result<AccountResponse>> UpdateAsync(int id, UpdateAccountRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
```

Resource-specific repository interfaces (e.g., `IIncomeRepository`, `IExpenseRepository`) extend `IRepository<T>` and add filtered query methods:

```csharp
// Domain/Interfaces/IIncomeRepository.cs
public interface IIncomeRepository : IRepository<Income>
{
    Task<IEnumerable<Income>> GetFilteredAsync(IncomeFilterParams filters, int page, int pageSize);
    Task<int> CountFilteredAsync(IncomeFilterParams filters);
}
```

### Data Models

**BaseEntity — `Domain/Common/BaseEntity.cs`**
```
Id          int         identity column, DB-generated
CreatedAt   DateTime    set on insert (UTC)
UpdatedAt   DateTime    updated on every save
```

**Domain Entities**

| Entity | Notable Properties |
|---|---|
| `Account` | `string Name`, `AccountType Type`, `decimal Balance`, `List<Income> Incomes = []`, `List<Expense> Expenses = []` |
| `Category` | `string Name`, `string Description = string.Empty`, `List<Expense> Expenses = []`, `List<Budget> Budgets = []` |
| `Income` | `decimal Amount`, `DateTime Date`, `string Description`, `int AccountId`, `Account Account` |
| `Expense` | `decimal Amount`, `DateTime Date`, `string Description`, `int AccountId`, `int CategoryId`, `List<ExpenseItem> Items = []` |
| `ExpenseItem` | `string Name`, `decimal Quantity`, `decimal UnitPrice`, `int ExpenseId` |
| `Budget` | `int CategoryId`, `DateTime StartDate`, `DateTime EndDate`, `decimal LimitAmount` |

**Enum — `Domain/Enums/AccountType.cs`**
```
Checking | Savings | Cash | Credit
```

**Result Pattern — `Domain/Common/Result.cs`**
```csharp
public record Result<T>
{
    public T? Value { get; init; }
    public string Error { get; init; } = string.Empty;
    public bool IsSuccess => string.Equals(Error, string.Empty, StringComparison.Ordinal);
    public static Result<T> Success(T value) => new() { Value = value };
    public static Result<T> Failure(string error) => new() { Error = error };
}
```

**PaginatedList — `Application/DTOs/Common/PaginatedList.cs`**
```csharp
public record PaginatedList<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);
```

**Standard Response — `API/Models/StandardApiResponse.cs`**
```csharp
public record StandardApiResponse<T>(int StatusCode, string Message, IEnumerable<string> Error, T? Data);
```

### API Endpoints

All routes are prefixed `/api/v1`. Controllers reside in `src/API/Controllers/`.

**Accounts** — `[Route("api/v1/accounts")]`
| Method | Path | Description |
|---|---|---|
| GET | `/api/v1/accounts?page=1&pageSize=10` | Paginated list |
| GET | `/api/v1/accounts/{id}` | Single by ID |
| POST | `/api/v1/accounts` | Create; body: `CreateAccountRequest` |
| PUT | `/api/v1/accounts/{id}` | Update; body: `UpdateAccountRequest` |
| DELETE | `/api/v1/accounts/{id}` | Delete; 409 if has incomes or expenses |

**Categories** — `[Route("api/v1/categories")]`
| Method | Path | Description |
|---|---|---|
| GET | `/api/v1/categories?page=1&pageSize=10` | Paginated list |
| GET | `/api/v1/categories/{id}` | Single by ID |
| POST | `/api/v1/categories` | Create |
| PUT | `/api/v1/categories/{id}` | Update |
| DELETE | `/api/v1/categories/{id}` | Delete; 409 if has expenses or budgets |

**Incomes** — `[Route("api/v1/incomes")]`
| Method | Path | Description |
|---|---|---|
| GET | `/api/v1/incomes?page=1&pageSize=10&startDate=&endDate=&accountId=` | Filtered paginated list |
| GET | `/api/v1/incomes/{id}` | Single by ID |
| POST | `/api/v1/incomes` | Create; body: `CreateIncomeRequest` |
| PUT | `/api/v1/incomes/{id}` | Update |
| DELETE | `/api/v1/incomes/{id}` | Delete |

**Expenses** — `[Route("api/v1/expenses")]`
| Method | Path | Description |
|---|---|---|
| GET | `/api/v1/expenses?page=1&pageSize=10&startDate=&endDate=&accountId=&categoryId=` | Filtered paginated list |
| GET | `/api/v1/expenses/{id}` | Single by ID |
| POST | `/api/v1/expenses` | Create |
| PUT | `/api/v1/expenses/{id}` | Update |
| DELETE | `/api/v1/expenses/{id}` | Delete; 409 if has items |

**Expense Items** — mixed routes
| Method | Path | Description |
|---|---|---|
| GET | `/api/v1/expenses/{expenseId}/items?page=1&pageSize=10` | Items by expense |
| GET | `/api/v1/expense-items/{id}` | Single by ID |
| POST | `/api/v1/expenses/{expenseId}/items` | Add item to expense |
| PUT | `/api/v1/expense-items/{id}` | Update |
| DELETE | `/api/v1/expense-items/{id}` | Delete |

**Budgets** — `[Route("api/v1/budgets")]`
| Method | Path | Description |
|---|---|---|
| GET | `/api/v1/budgets?page=1&pageSize=10&categoryId=&startDate=&endDate=` | Filtered paginated list |
| GET | `/api/v1/budgets/{id}` | Single by ID |
| POST | `/api/v1/budgets` | Create |
| PUT | `/api/v1/budgets/{id}` | Update |
| DELETE | `/api/v1/budgets/{id}` | Delete |

**HTTP Status Code Map**

| Condition | Code |
|---|---|
| Success (read) | 200 |
| Success (create) | 201 |
| Validation failure | 400 |
| Not found | 404 |
| Delete blocked by dependents | 409 |
| Unhandled exception | 500 |

---

## Integration Points

No external integrations. All persistence is internal via:
- `Npgsql.EntityFrameworkCore.PostgreSQL` provider targeting the PostgreSQL instance defined in `appsettings.json` (`ConnectionStrings:DefaultConnection`).
- Connection string must **never** appear in `launchSettings.json`.

---

## Testing Approach

### Unit Tests

**`Application.Tests` project**
- Test each service class with mocked `IRepository<T>` (no real DB).
- Critical scenarios per resource: successful create, update, delete; `Result.Failure` on not-found; `Result.Failure` on blocked-delete (has dependents); pagination with zero results.
- Validator tests using FluentValidation's `TestValidate()`: required fields, decimal ranges (`Amount > 0`), date ordering (`StartDate < EndDate` for Budget), max-length string rules.

### Integration Tests

**`API.Tests` project**
- Use `WebApplicationFactory<Program>` with a test PostgreSQL database (separate connection string via environment variable).
- Cover full HTTP request → response per resource: response envelope shape (`statusCode`, `message`, `error`, `data`), 400 on missing required fields, 404 on unknown ID, 409 on blocked delete, 201 with location header on create.
- Pagination: verify `TotalCount`, `Page`, `PageSize` fields on list responses.

---

## Development Sequencing

### Technical Dependencies

1. **Foundation**: `BaseEntity`, `Result<T>`, `IRepository<T>`, `AppDbContext`, EF Core + Npgsql packages, `Program.cs` baseline (add controllers, DI stubs).
2. **Per-resource vertical slice** (in dependency order):
   - `Category` (no FK dependencies)
   - `Account` (no FK dependencies)
   - `Income` (depends on Account)
   - `Expense` (depends on Account + Category)
   - `ExpenseItem` (depends on Expense)
   - `Budget` (depends on Category)
3. **Cross-cutting last**: `ResponseWrapperMiddleware`, `ExceptionHandlerMiddleware`, Swagger/OpenAPI setup.
4. **Migrations**: One EF migration per resource group after configurations are complete. Run `dotnet ef migrations add` from the Repository project.

---

## Monitoring and Observability

- `ILogger<T>` injected via constructor into all 6 service implementations.
- Log levels:
  - `Information`: create, update, delete operations with `entityType` and `entityId`.
  - `Warning`: blocked delete attempts (log dependent count).
  - `Error`: unhandled exceptions caught by `ExceptionHandlerMiddleware`.
- Structured log fields: `{ entityType, entityId, operation, elapsedMs }`.
- No external metrics sink required; built-in `Microsoft.Extensions.Logging` with console provider is sufficient.

---

## Technical Considerations

### Key Decisions

| Decision | Choice | Justification |
|---|---|---|
| API style | MVC Controllers | User-specified; `[ApiController]` provides automatic model binding and 400 ProblemDetails |
| Entity IDs | `int` identity | Follows `Domain/CLAUDE.md`; PostgreSQL identity column performs well at personal-finance data volumes |
| Delete strategy | Block (409 Conflict) | Prevents orphaned data; `HasDependentsAsync` check surfaced via `Result.Failure` in service |
| URL versioning | `/api/v1/` hard-coded in `[Route]` | No versioning library needed; straightforward for single-consumer API |
| Validation | FluentValidation validators in Application layer | Supports async DB checks (e.g., FK existence), cross-field rules; keeps controllers thin |
| Error propagation | `Result<T>` pattern | Avoids exceptions in normal flows per `CLAUDE.md`; all error paths are explicit |
| Response envelope | `ResponseWrapperMiddleware` | Centralizes envelope wrapping; controllers return `IActionResult` with raw data |

### Known Risks

- **Razor Pages residue**: `Program.cs` currently scaffolded for Razor Pages; must be fully replaced with Web API setup (remove `AddRazorPages`, `MapRazorPages`, add `AddControllers`, `MapControllers`).
- **Controller path deviation**: `API/CLAUDE.md` mandates `./routes` for endpoints; controllers in `./Controllers/` deviates per user instruction. Annotate in `API/CLAUDE.md` or team wiki.
- **Concurrent balance updates**: No optimistic concurrency on `Account.Balance`; acceptable for single-user scope per PRD, but revisit if multi-user is added later.
- **Migration sequencing**: Dropping or renaming entity properties after the first migration requires manual migration scripts; enforce a no-drop policy during development.

### Compliance with Standards

| Rule Source | Applied Rules |
|---|---|
| `CLAUDE.md` | Primary constructors on all classes; `string.Empty` defaults; list props initialized in declaration; `async/await` throughout; `Result<T>` over exceptions; file-scoped namespaces; `//created by:` header on every `.cs` file; `ILogger` for all logs |
| `rules.architecture.md` | Private `readonly` fields; DI via constructor; no circular deps; one public type per file; `Result<T>` and `PaginatedList<T>` as generics |
| `rules.naming.md` | All methods start with a verb (`GetAllAsync`, `CreateAsync`, `DeleteAsync`); parameter objects for DTOs (>3 params) |
| `rules.style.md` | Early returns in service methods; LINQ for response mapping; no flag parameters; methods ≤50 lines |
| `rules.platform.md` | `dotnet CLI` for all package management; `dotnet build` passes before task completion; file-scoped namespaces |
| `Domain/CLAUDE.md` | All entities inherit `BaseEntity` (`int Id`, `DateTime CreatedAt`, `DateTime UpdatedAt`); string props default `string.Empty` |
| `Repository/CLAUDE.md` | EF Core for all data access; `IEntityTypeConfiguration<T>` per entity; migrations required after model changes |
| `API/CLAUDE.md` | DTOs in `Application`; all responses use standard envelope; FluentValidation for request validation; no business code in `Program.cs` |

### Relevant and Dependent Files

**New files (to be created):**
```
src/Domain/Common/BaseEntity.cs
src/Domain/Common/Result.cs
src/Domain/Entities/Account.cs
src/Domain/Entities/Category.cs
src/Domain/Entities/Income.cs
src/Domain/Entities/Expense.cs
src/Domain/Entities/ExpenseItem.cs
src/Domain/Entities/Budget.cs
src/Domain/Enums/AccountType.cs
src/Domain/Interfaces/IRepository.cs
src/Domain/Interfaces/IAccountRepository.cs
src/Domain/Interfaces/ICategoryRepository.cs
src/Domain/Interfaces/IIncomeRepository.cs
src/Domain/Interfaces/IExpenseRepository.cs
src/Domain/Interfaces/IExpenseItemRepository.cs
src/Domain/Interfaces/IBudgetRepository.cs
src/Application/DTOs/Common/PaginatedList.cs
src/Application/DTOs/Accounts/{CreateAccountRequest,UpdateAccountRequest,AccountResponse}.cs
src/Application/DTOs/Categories/{CreateCategoryRequest,UpdateCategoryRequest,CategoryResponse}.cs
src/Application/DTOs/Incomes/{CreateIncomeRequest,UpdateIncomeRequest,IncomeResponse,IncomeFilterParams}.cs
src/Application/DTOs/Expenses/{CreateExpenseRequest,UpdateExpenseRequest,ExpenseResponse,ExpenseFilterParams}.cs
src/Application/DTOs/ExpenseItems/{CreateExpenseItemRequest,UpdateExpenseItemRequest,ExpenseItemResponse}.cs
src/Application/DTOs/Budgets/{CreateBudgetRequest,UpdateBudgetRequest,BudgetResponse}.cs
src/Application/Services/IAccountService.cs  +  AccountService.cs
src/Application/Services/ICategoryService.cs  +  CategoryService.cs
src/Application/Services/IIncomeService.cs  +  IncomeService.cs
src/Application/Services/IExpenseService.cs  +  ExpenseService.cs
src/Application/Services/IExpenseItemService.cs  +  ExpenseItemService.cs
src/Application/Services/IBudgetService.cs  +  BudgetService.cs
src/Application/Validators/Create{Entity}RequestValidator.cs  (×6)
src/Application/Validators/Update{Entity}RequestValidator.cs  (×6)
src/Repository/Data/AppDbContext.cs
src/Repository/Configurations/{Account,Category,Income,Expense,ExpenseItem,Budget}Configuration.cs
src/Repository/Repositories/Repository.cs  (generic base)
src/Repository/Repositories/{Account,Category,Income,Expense,ExpenseItem,Budget}Repository.cs
src/API/Controllers/{Accounts,Categories,Incomes,Expenses,ExpenseItems,Budgets}Controller.cs
src/API/Middleware/ExceptionHandlerMiddleware.cs
src/API/Middleware/ResponseWrapperMiddleware.cs
src/API/Models/StandardApiResponse.cs
```

**Modified files:**
```
src/API/Program.cs             — replace Razor Pages with Web API pipeline + DI registrations
src/API/API.csproj             — add: FluentValidation, Npgsql.EntityFrameworkCore.PostgreSQL, Swashbuckle
src/Application/Application.csproj  — add: FluentValidation, project ref to Domain
src/Repository/Repository.csproj    — add: EF Core, Npgsql provider, project ref to Domain
src/Domain/Domain.csproj       — no new packages; add project refs if needed
pigMoney.slnx                  — verify all 4 src projects are registered
```
