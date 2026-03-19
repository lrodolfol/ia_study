# Rules: Naming

## Language

All code must be written in English, including variable names, methods, classes, comments, and documentation.

## Methods and Functions

Methods and functions should perform one clear action, and that should be reflected in the name, which must start with a verb, never a noun.

**Example:**
```csharp
// ❌ Avoid
User user(Guid id) => new();
object userData() => new();

// ✅ Prefer
User getUser(Guid id) => new();
Task<UserData> fetchUserDataAsync() => Task.FromResult(new UserData());
Task createUserAsync(UserData data) => Task.CompletedTask;
Task updateUserEmailAsync(Guid id, string email) => Task.CompletedTask;
```

## Parameters

Whenever possible, avoid passing more than 3 parameters. Prefer using objects when needed.

**Example:**
```csharp
// ❌ Avoid
Task CreateUserAsync(string name, string email, int age, string address, string phone) => Task.CompletedTask;

// ✅ Prefer
public record CreateUserParams(
    string Name,
    string Email,
    int Age,
    string Address,
    string Phone
);

Task CreateUserAsync(CreateUserParams parameters) => Task.CompletedTask;
```

## Variable Declaration

Use `var` only when the type is obvious from the initialization. Never use `dynamic` without a clear justification.

**Example:**
```csharp
// ❌ Avoid
dynamic userName = "John";
var x = GetUser(); // type is not obvious

// ✅ Prefer
var userName = "John";           // obvious type: string
var users = new List<User>();    // obvious type: List<User>
User user = GetUser();           // explicit type when not obvious

// Constants
const int MaxRetries = 3;
const string DefaultRole = "user";
```
