## Rules

- Only files relevant to data persistence can be create into this cs project:
    - repositories files
    - database mappers files
- Each entitie(model) has a repository and each repository has a data persistence
- You must use the Entity Framework Core library for data manager
- The database will be PostgreSQL
- After create the models, you need create the migrations for database
- You need create the database mappers for the models, like this:
```csharp
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Post");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityColumn();

        builder.Property(x => x.Active)
            .HasColumnName("Active")
            .HasColumnType("BIT")
            .HasDefaultValue(true);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasColumnName("Name")
            .HasColumnType("VARCHAR")
            .HasMaxLength(30);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnName("Description")
            .HasColumnType("VARCHAR")
            .HasMaxLength(100);

        InsertDataTemp(builder);
    }
```
- Don't create abstraction here, all abstraction must be created into Core Project, located in `../Domain`