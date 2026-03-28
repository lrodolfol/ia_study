//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityColumn();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasColumnName("name")
            .HasColumnType("VARCHAR")
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasColumnName("type")
            .HasColumnType("INTEGER");

        builder.Property(x => x.InitialBalance)
            .IsRequired()
            .HasColumnName("initial_balance")
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

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

        builder.HasMany(x => x.Expenses)
            .WithOne(x => x.Account)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Incomes)
            .WithOne(x => x.Account)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
