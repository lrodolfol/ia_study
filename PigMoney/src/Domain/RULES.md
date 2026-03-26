# Rules

- Only entites(models), enums, aggregates and abstractions (abstract class and interfaces) can be create into this cs project
- Use paths for sepate the entities, enums and aggregates
- All entities(models) will inherit from other abstration class with this properties:
    propertie: ID
    type: int

    propertie: CreatedAt
    type: DateTime

    propertie: UpdatedAt
    type: DateTime
- All entitie class represent a table from the database
- All string propertie are equals string.Empty by default
