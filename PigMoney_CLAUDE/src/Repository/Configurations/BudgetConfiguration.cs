//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budget");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityColumn();

        builder.Property(x => x.CategoryId)
            .IsRequired()
            .HasColumnName("CategoryId");

        builder.Property(x => x.StartDate)
            .IsRequired()
            .HasColumnName("StartDate");

        builder.Property(x => x.EndDate)
            .IsRequired()
            .HasColumnName("EndDate");

        builder.Property(x => x.LimitAmount)
            .IsRequired()
            .HasColumnName("LimitAmount")
            .HasColumnType("DECIMAL(18,2)");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnName("UpdatedAt");

        builder.HasOne(x => x.Category)
            .WithMany(c => c.Budgets)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
