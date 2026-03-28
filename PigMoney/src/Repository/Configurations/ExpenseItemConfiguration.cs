//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExpenseItemConfiguration : IEntityTypeConfiguration<ExpenseItem>
{
    public void Configure(EntityTypeBuilder<ExpenseItem> builder)
    {
        builder.ToTable("expense_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityColumn();

        builder.Property(x => x.ExpenseId)
            .IsRequired()
            .HasColumnName("expense_id");

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnName("amount")
            .HasPrecision(18, 2);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnName("description")
            .HasColumnType("VARCHAR")
            .HasMaxLength(200);

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id");

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

        builder.HasOne(x => x.Expense)
            .WithMany(x => x.ExpenseItems)
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ExpenseId).HasDatabaseName("idx_expense_items_expense").HasFilter("is_deleted = FALSE");

        builder.ToTable(t => t.HasCheckConstraint("CK_expense_items_amount", "amount >= 0"));
    }
}
