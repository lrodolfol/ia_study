# Rules: Style and Flow

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

## LINQ

Prefer using LINQ (`Where`, `Select`, `FirstOrDefault`, `Aggregate`) over `for` and `while` loops for collection manipulation.

**Example:**
```csharp
var users = new List<User>
{
    new("1", "John", 30),
    new("2", "Jane", 25),
    new("3", "Bob", 35)
};

// ❌ Avoid
var result = new List<string>();
for (int i = 0; i < users.Count; i++)
{
    if (users[i].Age > 25)
        result.Add(users[i].Name);
}

// ✅ Prefer
var result = users
    .Where(u => u.Age > 25)
    .Select(u => u.Name)
    .ToList();

// FirstOrDefault
var user = users.FirstOrDefault(u => u.Id == "2");

// Aggregate (equivalent to reduce)
var totalAge = users.Aggregate(0, (sum, u) => sum + u.Age);
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
