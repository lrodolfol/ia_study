//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityColumn();

        builder.Property(x => x.CategoryId)
            .IsRequired()
            .HasColumnName("category_id");

        builder.Property(x => x.LimitAmount)
            .IsRequired()
            .HasColumnName("limit_amount")
            .HasPrecision(18, 2);

        builder.Property(x => x.StartDate)
            .IsRequired()
            .HasColumnName("start_date");

        builder.Property(x => x.EndDate)
            .IsRequired()
            .HasColumnName("end_date");

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
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CategoryId).HasDatabaseName("idx_budgets_category").HasFilter("is_deleted = FALSE");
        builder.HasIndex(x => new { x.StartDate, x.EndDate }).HasDatabaseName("idx_budgets_dates").HasFilter("is_deleted = FALSE");

        builder.ToTable(t => t.HasCheckConstraint("CK_budgets_limit_amount", "limit_amount >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_budgets_dates", "end_date > start_date"));
    }
}
