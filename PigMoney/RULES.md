# RULES

**This is a .NET project. All code must be written in C# with strong typing and in English, including variable names, methods, and classes. Use modern language features (C# 12+). Avoid comments whenever possible. The code should be self-explanatory.**

- All classes must use primary constructors.
- All string properties from classes must use `string.Equals` by default.
- All list properties must be instantiated in the declaration line.
- Use the `dotnet CLI` as the standard package manager and execution tool.
- Before finishing a task, validate build and typing.
- Use `var` only when the type is obvious; avoid `dynamic` unless justified.
- Methods/functions must represent one clear action and start with a verb.
- Avoid passing more than 3 parameters; prefer parameter objects.
- Class fields must be `private` or `private readonly`.
- Never nest more than two `if/else` levels; prefer early returns.
- Never use flag parameters to switch behavior.
- Prefer LINQ over `for`/`while` for collection manipulation.
- Always use `async/await` for asynchronous operations; avoid `.Result` and `.Wait()`.
- Never use `object` or `dynamic` where a concrete type can be defined.
- Use file-scoped namespaces and keep `using` directives organized.
- Keep one public type per file.
- Use dependency injection and avoid circular dependencies.
- Remenber: High-level layers do not depend on low-level layers.
- Never instantiate services manually when DI is available.
- Use generics/records/utility types to reduce duplication.
- Never return `null` and never pass `null` to methods; always apply default values and validate properties before calling dependent methods.
- Do not install unnecessary dependencies in projects. For example, the web-api project does not need the Dapper library; only the Repository project needs this library.
- ** WRITE THIS COMMENT ON FIRST LINE INTO ALL .cs FILES:
    //created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
    **
- ** NEVE PUT `System.Console.WriteLine()` INTO THE CODE. USE THE Microsoft.Extensions.Logging LIBRARY FOR LOGS. **

## Detailed Rules

- Naming rules: `rules/rules.naming.md`
- Architecture rules: `rules/rules.architecture.md`
- Style and flow rules: `rules/rules.style.md`
- Platform/build rules: `rules/rules.platform.md`
