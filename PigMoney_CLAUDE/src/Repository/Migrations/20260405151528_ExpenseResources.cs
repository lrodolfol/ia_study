using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class ExpenseResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseItem_Expense_ExpenseId",
                table: "ExpenseItem");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "ExpenseItem",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ExpenseItem",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExpenseItem",
                type: "VARCHAR",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Expense",
                type: "VARCHAR",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Expense",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseItem_Expense_ExpenseId",
                table: "ExpenseItem",
                column: "ExpenseId",
                principalTable: "Expense",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseItem_Expense_ExpenseId",
                table: "ExpenseItem");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "ExpenseItem",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ExpenseItem",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ExpenseItem",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Expense",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Expense",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseItem_Expense_ExpenseId",
                table: "ExpenseItem",
                column: "ExpenseId",
                principalTable: "Expense",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
