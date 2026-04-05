using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class BudgetResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "LimitAmount",
                table: "Budget",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "LimitAmount",
                table: "Budget",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");
        }
    }
}
