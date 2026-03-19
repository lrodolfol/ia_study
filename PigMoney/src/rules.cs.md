# Coding Standards

## Language

All source code must be written in English, including variable names, methods, classes, comments, and documentation.

**Example:**
```csharp
// ❌ Avoid
var nomeDoProduto = "Laptop";
decimal calcularPreco() => 0;

// ✅ Prefer
var productName = "Laptop";
decimal calculatePrice() => 0;
```

## Naming Conventions

### camelCase
Use for local variables and method parameters.

**Example:**
```csharp
var userName = "John";
var isActive = true;
void getUserById(Guid userId) { }
```

### PascalCase
Use for classes, interfaces, records, enums, properties, and public methods.

**Example:**
```csharp
public class UserRepository { }
public interface IPaymentGateway { }
public record UserProfile(string Name);
public enum PaymentStatus { Pending, Paid }
```

### _camelCase
Use for private class fields.

**Example:**
```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
}
```

## Clear Naming

Avoid abbreviations, but also avoid very long names (more than 30 characters).

**Example:**
```csharp
// ❌ Avoid
var usrNm = "John"; // too abbreviated
var userNameFromDatabaseQueryResult = "John"; // too long

// ✅ Prefer
var userName = "John";
var dbUserName = "John";
```

## Constants and Magic Numbers

Declare constants to represent magic numbers with better readability.

**Example:**
```csharp
// ❌ Avoid
if (user.Age >= 18) { }
await Task.Delay(3600000);

// ✅ Prefer
const int MinimumAge = 18;
const int OneHourInMs = 60 * 60 * 1000;

if (user.Age >= MinimumAge) { }
await Task.Delay(OneHourInMs);
```

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

## Side Effects

Avoid side effects. In general, a method or function should do either a mutation OR a query; never allow a query to have side effects.

**Example:**
```csharp
// ❌ Avoid
IEnumerable<User> GetUsers()
{
    var users = database.QueryUsers();
    logger.LogInformation("Users fetched"); // side effect
    cache.Set("users", users); // side effect
    return users;
}

// ✅ Prefer
IEnumerable<User> GetUsers()
{
    return database.QueryUsers();
}

IEnumerable<User> FetchAndCacheUsers()
{
    var users = GetUsers();
    logger.LogInformation("Users fetched");
    cache.Set("users", users);
    return users;
}
```

## Conditional Structures

Never nest more than two `if/else` levels. Always prefer early returns.

**Example:**
```csharp
// ❌ Avoid
PaymentResult? ProcessPayment(User? user, decimal amount)
{
    if (user is not null)
    {
        if (user.IsActive)
        {
            if (amount > 0)
            {
                if (user.Balance >= amount)
                {
                    return CompletePayment(user, amount);
                }
            }
        }
    }
    return null;
}

// ✅ Prefer
PaymentResult? ProcessPayment(User? user, decimal amount)
{
    if (user is null) return null;
    if (!user.IsActive) return null;
    if (amount <= 0) return null;
    if (user.Balance < amount) return null;
    return CompletePayment(user, amount);
}
```

## Flag Parameters

Never use flag parameters to switch method/function behavior. In these cases, extract specific methods/functions for each behavior.

**Example:**
```csharp
// ❌ Avoid
User GetUser(Guid id, bool includeOrders)
{
    var user = database.GetUser(id);
    if (includeOrders)
    {
        user.Orders = database.GetOrders(id).ToList();
    }
    return user;
}

// ✅ Prefer
User GetUser(Guid id)
{
    return database.GetUser(id);
}

User GetUserWithOrders(Guid id)
{
    var user = GetUser(id);
    user.Orders = database.GetOrders(id).ToList();
    return user;
}
```

## Method and Class Size

- Avoid long methods (more than 50 lines)
- Avoid long classes (more than 300 lines)

**Example:**
```csharp
// ❌ Avoid
public class UserService
{
    // 500 lines of code
}

// ✅ Prefer
public class UserAuthService { }
public class UserProfileService { }
public class UserNotificationService { }
```

## Formatting

Avoid blank lines inside methods and functions.

**Example:**
```csharp
// ❌ Avoid
decimal CalculateTotal(IEnumerable<Item> items)
{
    var subtotal = items.Sum(item => item.Price);

    var tax = subtotal * 0.1m;

    return subtotal + tax;
}

// ✅ Prefer
decimal CalculateTotal(IEnumerable<Item> items)
{
    var subtotal = items.Sum(item => item.Price);
    var tax = subtotal * 0.1m;
    return subtotal + tax;
}
```

## Comments

Avoid comments whenever possible. The code should be self-explanatory.

**Example:**
```csharp
// ❌ Avoid
// Check if user is adult
if (user.Age >= 18) { }

// ✅ Prefer
const int MinimumLegalAge = 18;
var isAdult = user.Age >= MinimumLegalAge;
if (isAdult) { }
```

## Variable Declaration

Never declare more than one variable on the same line.

**Example:**
```csharp
// ❌ Avoid
var name = "John"; var age = 30; var email = "john@example.com";

// ✅ Prefer
var name = "John";
var age = 30;
var email = "john@example.com";
```

## Variable Scope

Declare variables as close as possible to where they are used.

**Example:**
```csharp
// ❌ Avoid
void ProcessOrder(Guid orderId)
{
    var user = GetUser();
    var product = GetProduct();
    var discount = CalculateDiscount();
    ValidateOrder(orderId);
    CheckInventory(orderId);
    NotifyUser(user);
}

// ✅ Prefer
void ProcessOrder(Guid orderId)
{
    ValidateOrder(orderId);
    CheckInventory(orderId);
    var user = GetUser();
    NotifyUser(user);
}
```
