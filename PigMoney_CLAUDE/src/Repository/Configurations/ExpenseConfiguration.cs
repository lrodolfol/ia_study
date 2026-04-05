//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expense");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityColumn();

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnName("Amount")
            .HasColumnType("DECIMAL(18,2)");

        builder.Property(x => x.Date)
            .IsRequired()
            .HasColumnName("Date");

        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnName("Description")
            .HasColumnType("VARCHAR")
            .HasMaxLength(500);

        builder.Property(x => x.AccountId)
            .IsRequired()
            .HasColumnName("AccountId");

        builder.Property(x => x.CategoryId)
            .IsRequired()
            .HasColumnName("CategoryId");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnName("UpdatedAt");

        builder.HasOne(x => x.Account)
            .WithMany(a => a.Expenses)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
