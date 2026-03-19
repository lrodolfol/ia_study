# Rules: Architecture

## Class Properties

Always declare class fields as `private` or `private readonly`. Use properties with accessors to expose data publicly.

**Example:**
```csharp
// ❌ Avoid
public class UserService
{
    public Database Database;
    public Config Config;
}

// ✅ Prefer
public class UserService
{
    private readonly IDatabase _database;
    private readonly IConfig _config;

    public UserService(IDatabase database, IConfig config)
    {
        _database = database;
        _config = config;
    }

    public User GetUser(Guid id)
    {
        return _database.FindUser(id);
    }
}
```

## Async/Await

Always use `async/await` for asynchronous operations. Avoid `.Result` and `.Wait()`. Suffix async methods with `Async`.

**Example:**
```csharp
// ❌ Avoid
public User GetUser(Guid id)
    return _repository.FindUserAsync(id).Result; // can cause deadlock

// ✅ Prefer
public async Task<User> GetUserAsync(Guid id)
    return await _repository.FindUserAsync(id);


// Usage
try
{
    var user = await GetUserAsync(Guid.NewGuid());
    Console.WriteLine(user.Name);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error fetching user");
}
```

## Strong Typing

Never use `object` or `dynamic` where the type can be defined. Always create types, records, or interfaces to represent data.

**Example:**
```csharp
// ❌ Avoid
public object ProcessData(object data)
    return data;

// ✅ Prefer
public record DataItem(string Id, decimal Value);

public IEnumerable<decimal> ProcessData(IEnumerable<DataItem> data)
    return data.Select(item => item.Value);

// For unknown cases, use generics
public T ParseJson<T>(string json)
{
    return JsonSerializer.Deserialize<T>(json)
        ?? throw new InvalidOperationException("Failed to deserialize JSON.");
}
```

## One Type per File

Each file should contain only one public type (class, interface, record, enum). The file name must match the type name.

**Example:**
```csharp
// ❌ Avoid
// Models.cs
public class User { }
public class Order { }
public enum Status { }

// ✅ Prefer
// User.cs
public class User { }

// Order.cs
public class Order { }

// Status.cs
public enum Status { Active, Inactive }
```

## Dependency Injection

Avoid circular dependencies. Use constructor dependency injection and program to interfaces, not concrete implementations.

**Example:**
```csharp
// ❌ Avoid
public class UserService
{
    private readonly OrderService _orderService; // concrete dependency
}

public class OrderService
{
    private readonly UserService _userService; // circular dependency
}

// ✅ Prefer
public interface IUserService
{
    Task<User> GetUserAsync(Guid id);
}

public interface IOrderService
{
    Task<IEnumerable<Order>> GetOrdersAsync(Guid userId);
}

public class UserService : IUserService
{
    public async Task<User> GetUserAsync(Guid id) { ... }
}

public class OrderService : IOrderService
{
    public async Task<IEnumerable<Order>> GetOrdersAsync(Guid userId) { ... }
}

// Composite service
public class UserOrderService
{
    private readonly IUserService _userService;
    private readonly IOrderService _orderService;

    public UserOrderService(IUserService userService, IOrderService orderService)
    {
        _userService = userService;
        _orderService = orderService;
    }

    public async Task<UserWithOrders> GetUserWithOrdersAsync(Guid userId)
    {
        var user = await _userService.GetUserAsync(userId);
        var orders = await _orderService.GetOrdersAsync(userId);
        return new UserWithOrders(user, orders);
    }
}
```

## Dependency Injection II

Always use dependency injection and never instantiate services manually.

**Example:**
```csharp
// ❌ Avoid
public class PaymentController
{
    public void Pay(PaymentRequest request)
    {
        // ❌ Service created manually
        var paymentService = new PaymentService();

        paymentService.Process(request);
    }
}

// ✅ Prefer
public class PaymentController(IPaymentRequest paymentService)
{
    public void Pay()
    {
        paymentService.Process(request);
    }
}
```

## Utility Types and Generics

Use generics, records, and C# utility types where appropriate to avoid code duplication.

**Example:**
```csharp
public record User(int Id, string Name, string Email, string Password);

// Equivalent to Omit<User, 'id'>
public record CreateUserRequest(string Name, string Email, string Password);

// Equivalent to Pick<User, 'id' | 'name' | 'email'>
public record UserPublic(int Id, string Name, string Email);

// Equivalent to Partial<User> - using nullable
public record UpdateUserRequest(string? Name, string? Email, string? Password);

// Result type to avoid exceptions in normal flow
public record Result<T>(T? Value, string? Error)
{
    public bool IsSuccess => Error is null;
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(string error) => new(default, error);
}
```

## Null Safety and Default Values

Never return `null` from methods and never pass `null` to method parameters. Always use safe default values when properties are missing, and validate data before calling methods that depend on those properties.

**Example:**
```csharp
// ❌ Avoid
public ListUser GetUsers()
{
    List<User> users;
    ...

    if (users.Count <= 0) return null;
}

// ✅ Prefer
public ListUser GetUsers()
{
    List<User> users;
    ...

    if (users.Count <= 0){
        users = new List<User>();
    }

    return users;
}
```
**Example:**
```csharp
// ❌ Avoid
public void Build()
{
    string address;
    ..

    SendEmail(address) //address is null here, don't do this
}

// ✅ Prefer
public void Build()
{
    string address;
    ..

    if(! address is null)
        SendEmail(address) //check if address is null before call other method
}
```
