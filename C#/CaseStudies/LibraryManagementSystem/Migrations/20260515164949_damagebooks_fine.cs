using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class damagebooks_fine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Members_MemberId",
                table: "User");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BookCopies_Status",
                table: "BookCopies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_User_MemberId",
                table: "Users",
                newName: "IX_Users_MemberId");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Books",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DamagePercentage",
                table: "BookCopies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BookCopies_DamagePercentage",
                table: "BookCopies",
                sql: "\"DamagePercentage\" >= 0 AND \"DamagePercentage\" <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BookCopies_Status",
                table: "BookCopies",
                sql: "\"Status\" IN ('Available', 'Borrowed', 'Lost')");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Members_MemberId",
                table: "Users",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Members_MemberId",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BookCopies_DamagePercentage",
                table: "BookCopies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BookCopies_Status",
                table: "BookCopies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "DamagePercentage",
                table: "BookCopies");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameIndex(
                name: "IX_Users_MemberId",
                table: "User",
                newName: "IX_User_MemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "UserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BookCopies_Status",
                table: "BookCopies",
                sql: "\"Status\" IN ('Available', 'Borrowed', 'Damaged', 'Lost')");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Members_MemberId",
                table: "User",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
