# Rules

- Only APIS instructions and projects files loaders can be create into this cs project:
    - API Configurations like launchSettings.json, CORS configurations
    - API Routes
    - API Documentation
    - Dependencies injections

- The ./programs must have only loaders from this project.
- DON'T use ./program.cs for create endpoints or business code.
- Never use models as parameters for endpoints, create DTO for this. DTO are located into `../Application`.
- All endpoints must be create into path ./routes 
- Use the library FluentValidation for validate the request paramenters
- All responses need follow this pattern:
    ```json{
        error: [],
        message: "",
        statusCode: 201,
    }
    ```
- **NEVER, NEVER PUT PASSWORD OR SENSITIVE INFORMATION INTO LAUNCHSETTINGS.JSON**