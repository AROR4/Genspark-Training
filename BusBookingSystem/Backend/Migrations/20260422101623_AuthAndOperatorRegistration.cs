using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AuthAndOperatorRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bus_operators_user_id",
                table: "bus_operators");

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "admin_notes",
                table: "bus_operators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "approval_status",
                table: "bus_operators",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "PENDING");

            migrationBuilder.AddColumn<string>(
                name: "contact_email",
                table: "bus_operators",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_phone",
                table: "bus_operators",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "legal_name",
                table: "bus_operators",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "license_number",
                table: "bus_operators",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "owner_name",
                table: "bus_operators",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "registration_number",
                table: "bus_operators",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_number",
                table: "bus_operators",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_bus_operators_user_id",
                table: "bus_operators",
                column: "user_id",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_bus_operators_approval_status",
                table: "bus_operators",
                sql: "approval_status IN ('PENDING','APPROVED','REJECTED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bus_operators_user_id",
                table: "bus_operators");

            migrationBuilder.DropCheckConstraint(
                name: "ck_bus_operators_approval_status",
                table: "bus_operators");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "users");

            migrationBuilder.DropColumn(
                name: "admin_notes",
                table: "bus_operators");

            migrationBuilder.DropColumn(
                name: "approval_status",
                table: "bus_operators");

            migrationBuilder.DropColumn(
                name: "contact_email",
                table: "bus_operators");

            migrationBuilder.DropColumn(
                name: "contact_phone",
                table: "bus_operators");

            migrationBuilder.DropColumn(
                name: "legal_name",
                table: "bus_operators");

            migrationBuilder.DropColumn(
                name: "license_number",
                table: "bus_operators");

            migrationBuilder.DropColumn(
                name: "owner_name",
                table: "bus_operators");

            migrationBuilder.DropColumn(
                name: "registration_number",
                table: "bus_operators");

            migrationBuilder.DropColumn(
                name: "tax_number",
                table: "bus_operators");

            migrationBuilder.CreateIndex(
                name: "IX_bus_operators_user_id",
                table: "bus_operators",
                column: "user_id");
        }
    }
}
