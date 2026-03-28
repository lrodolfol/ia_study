using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class SecondCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tnn");

            migrationBuilder.RenameTable(
                name: "incomes",
                newName: "incomes",
                newSchema: "tnn");

            migrationBuilder.RenameTable(
                name: "expenses",
                newName: "expenses",
                newSchema: "tnn");

            migrationBuilder.RenameTable(
                name: "expense_items",
                newName: "expense_items",
                newSchema: "tnn");

            migrationBuilder.RenameTable(
                name: "categories",
                newName: "categories",
                newSchema: "tnn");

            migrationBuilder.RenameTable(
                name: "budgets",
                newName: "budgets",
                newSchema: "tnn");

            migrationBuilder.RenameTable(
                name: "accounts",
                newName: "accounts",
                newSchema: "tnn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "incomes",
                schema: "tnn",
                newName: "incomes");

            migrationBuilder.RenameTable(
                name: "expenses",
                schema: "tnn",
                newName: "expenses");

            migrationBuilder.RenameTable(
                name: "expense_items",
                schema: "tnn",
                newName: "expense_items");

            migrationBuilder.RenameTable(
                name: "categories",
                schema: "tnn",
                newName: "categories");

            migrationBuilder.RenameTable(
                name: "budgets",
                schema: "tnn",
                newName: "budgets");

            migrationBuilder.RenameTable(
                name: "accounts",
                schema: "tnn",
                newName: "accounts");
        }
    }
}
