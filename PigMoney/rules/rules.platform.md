# Rules: Platform and Build

## Package Manager

Use the `dotnet CLI` as the standard tool for managing dependencies and running scripts.

**Example:**
```bash
# Restore dependencies
dotnet restore

# Add a new dependency
dotnet add package Dapper

# Remove a dependency
dotnet remove package Dapper

# Run the project
dotnet run

# Build
dotnet build

# Tests
dotnet test
```

## Build Validation

Before finishing a task, always validate that the build and typing are correct.

**Example:**
```bash
# Check that it compiles without errors
dotnet build --no-incremental

# Run tests
dotnet test

# Treat warnings as errors (configure in .csproj)
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

## Namespaces and Usings

Use file-scoped namespaces. Organize `using` directives in order: system, third-party, project. Remove unused usings.

**Example:**
```csharp
// ❌ Avoid
using MyApp.Services;
using System;
using Dapper;
using System.Collections.Generic;

namespace MyApp.Controllers
{
    public class UserController { }
}

// ✅ Prefer
using System;
using System.Collections.Generic;
using Dapper;
using MyApp.Services;

namespace MyApp.Controllers;

public class UserController { }
```
