using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "VARCHAR", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    initial_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "VARCHAR", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "budgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    limit_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budgets", x => x.Id);
                    table.CheckConstraint("CK_budgets_dates", "end_date > start_date");
                    table.CheckConstraint("CK_budgets_limit_amount", "limit_amount >= 0");
                    table.ForeignKey(
                        name: "FK_budgets_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "VARCHAR", maxLength: 500, nullable: false, defaultValue: ""),
                    notes = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.Id);
                    table.CheckConstraint("CK_expenses_amount", "amount >= 0");
                    table.ForeignKey(
                        name: "FK_expenses_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expenses_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "incomes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "VARCHAR", maxLength: 500, nullable: false, defaultValue: ""),
                    notes = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incomes", x => x.Id);
                    table.CheckConstraint("CK_incomes_amount", "amount >= 0");
                    table.ForeignKey(
                        name: "FK_incomes_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_incomes_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expense_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    expense_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    description = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_items", x => x.Id);
                    table.CheckConstraint("CK_expense_items_amount", "amount >= 0");
                    table.ForeignKey(
                        name: "FK_expense_items_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expense_items_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_budgets_category",
                table: "budgets",
                column: "category_id",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "idx_budgets_dates",
                table: "budgets",
                columns: new[] { "start_date", "end_date" },
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "idx_expense_items_expense",
                table: "expense_items",
                column: "expense_id",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_expense_items_category_id",
                table: "expense_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_expenses_account",
                table: "expenses",
                column: "account_id",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "idx_expenses_category",
                table: "expenses",
                column: "category_id",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "idx_expenses_date",
                table: "expenses",
                column: "date",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "idx_incomes_account",
                table: "incomes",
                column: "account_id",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "idx_incomes_category",
                table: "incomes",
                column: "category_id",
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "idx_incomes_date",
                table: "incomes",
                column: "date",
                filter: "is_deleted = FALSE");

            migrationBuilder.Sql(@"
                CREATE FUNCTION check_expense_items_sum()
                RETURNS TRIGGER AS $$
                DECLARE
                    expense_amount DECIMAL(18, 2);
                    items_sum DECIMAL(18, 2);
                BEGIN
                    SELECT amount INTO expense_amount FROM expenses WHERE ""Id"" = NEW.expense_id AND is_deleted = FALSE;
                    SELECT COALESCE(SUM(amount), 0) INTO items_sum FROM expense_items WHERE expense_id = NEW.expense_id AND is_deleted = FALSE;
                    
                    IF items_sum > expense_amount THEN
                        RAISE EXCEPTION 'Sum of expense items cannot exceed parent expense amount';
                    END IF;
                    
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER validate_expense_items_sum
                AFTER INSERT OR UPDATE ON expense_items
                FOR EACH ROW
                EXECUTE FUNCTION check_expense_items_sum();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS validate_expense_items_sum ON expense_items;
                DROP FUNCTION IF EXISTS check_expense_items_sum();
            ");

            migrationBuilder.DropTable(
                name: "budgets");

            migrationBuilder.DropTable(
                name: "expense_items");

            migrationBuilder.DropTable(
                name: "incomes");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
