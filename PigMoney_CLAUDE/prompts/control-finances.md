<role>
    You are a senior software engineer specialized in .NET with CSharp, and you are creating a complete API for personal financial expense management.
</role>

<task>API Creation</task>

<requirements>
    ### Business

    - Create a complete API using SOLID principles
    - Create CRUD for the models

    ### Technical

    - Implement in the existing projects within ./src
    - Pay attention to the RULES.md inside each directory. They will be the guide for the API construction

    ### Architecture

    - Follow SOLID principles to build the API
    - Use the Repository pattern for data access
    - Avoid circular dependencies at all costs
    - **Do not use MediatR**
    - Do not use Console.WriteLine(). Use Microsoft.Extensions.Logging.
    - All API responses must follow this standardization:
        ```json
        {
            "error": [],
            "message": "",
            "statusCode": 201
        }
        ```
</requirements>

<endpoints>
    ### Backend

    GET /api/weather?city=<city> 200

    Status Code:

    201: Created
    200: Success
    400: Missing data
    404: Not found
    500: Server failure

    Request Payload: DTO for each model
    Response Payload: All API responses must follow this standardization:
        ```json
        {
            "error": [],
            "message": "",
            "statusCode": 201
        }
        ```
    Pay attention to the other details contained in `../src/API/RULES.md`
</endpoints>

<repository>
    ### Data Persistence

    Persistence will be in Postgres
    Pay attention to the details contained in `../src/Repository/RULES.md`
</repository>

<tests>
    ### Endpoint Validation

    - Create a curl command to test each of the endpoints
</tests>

<critical>
    ### Out of Scope

    - **DO NOT** implement user authentication and authorization
    - **DO NOT** create comments in the code
    - **DO NOT** put passwords and sensitive data directly in the code
</critical>