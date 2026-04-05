//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configurations;

public class ExpenseItemConfiguration : IEntityTypeConfiguration<ExpenseItem>
{
    public void Configure(EntityTypeBuilder<ExpenseItem> builder)
    {
        builder.ToTable("ExpenseItem");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityColumn();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasColumnName("Name")
            .HasColumnType("VARCHAR")
            .HasMaxLength(200);

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasColumnName("Quantity")
            .HasColumnType("DECIMAL(18,4)");

        builder.Property(x => x.UnitPrice)
            .IsRequired()
            .HasColumnName("UnitPrice")
            .HasColumnType("DECIMAL(18,2)");

        builder.Property(x => x.ExpenseId)
            .IsRequired()
            .HasColumnName("ExpenseId");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnName("UpdatedAt");

        builder.HasOne(x => x.Expense)
            .WithMany(e => e.Items)
            .HasForeignKey(x => x.ExpenseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
