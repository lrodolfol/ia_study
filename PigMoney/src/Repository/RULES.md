## Rules

- Only files relevant to data persistence can be create into this cs project:
    repositories files
    database mappers files
- Each entitie(model) has a repository and each repository has a data persistence
- You must use the Dapper library for data manager
- Don't create abstraction here, all abstraction must be created into Core Project, 
located in `../Core`