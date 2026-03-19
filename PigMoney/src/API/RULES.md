# Rules

- Only APIS instructions and projects files loaders can be create into this cs project:
    - API Configurations like launchSettings.json, CORS configurations
    - API Routes
    - API Documentation
    - Dependencies injections

- Never use models as parameters for endpoints, create DTO for this. DTO are located into `../Application`.
- The ./programs must have only loaders from this project.
- All endpoints must be create into path ./routes 
- NEVER, NEVER PUT PASSWORD OR SENSITIVE INFORMATION INTO LAUNCHSETTINGS.JSON
- DON'T use ./program.cs for create endpoints or business code.