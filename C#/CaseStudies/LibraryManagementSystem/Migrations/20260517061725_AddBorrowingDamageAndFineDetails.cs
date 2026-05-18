using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBorrowingDamageAndFineDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FineAmount",
                table: "Borrowings",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "NewDamagePercentage",
                table: "Borrowings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldDamagePercentage",
                table: "Borrowings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Borrowings_NewDamagePercentage",
                table: "Borrowings",
                sql: "\"NewDamagePercentage\" IS NULL OR (\"NewDamagePercentage\" >= 0 AND \"NewDamagePercentage\" <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Borrowings_OldDamagePercentage",
                table: "Borrowings",
                sql: "\"OldDamagePercentage\" >= 0 AND \"OldDamagePercentage\" <= 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Borrowings_NewDamagePercentage",
                table: "Borrowings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Borrowings_OldDamagePercentage",
                table: "Borrowings");

            migrationBuilder.DropColumn(
                name: "FineAmount",
                table: "Borrowings");

            migrationBuilder.DropColumn(
                name: "NewDamagePercentage",
                table: "Borrowings");

            migrationBuilder.DropColumn(
                name: "OldDamagePercentage",
                table: "Borrowings");
        }
    }
}
