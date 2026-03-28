//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityColumn();

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnName("amount")
            .HasPrecision(18, 2);

        builder.Property(x => x.Date)
            .IsRequired()
            .HasColumnName("date");

        builder.Property(x => x.CategoryId)
            .IsRequired()
            .HasColumnName("category_id");

        builder.Property(x => x.AccountId)
            .IsRequired()
            .HasColumnName("account_id");

        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnName("description")
            .HasColumnType("VARCHAR")
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Notes)
            .IsRequired()
            .HasColumnName("notes")
            .HasColumnType("TEXT")
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Expenses)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.Expenses)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ExpenseItems)
            .WithOne(x => x.Expense)
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Date).HasDatabaseName("idx_expenses_date").HasFilter("is_deleted = FALSE");
        builder.HasIndex(x => x.CategoryId).HasDatabaseName("idx_expenses_category").HasFilter("is_deleted = FALSE");
        builder.HasIndex(x => x.AccountId).HasDatabaseName("idx_expenses_account").HasFilter("is_deleted = FALSE");

        builder.ToTable(t => t.HasCheckConstraint("CK_expenses_amount", "amount >= 0"));
    }
}
