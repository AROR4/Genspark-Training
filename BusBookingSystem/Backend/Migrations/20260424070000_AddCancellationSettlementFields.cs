using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationSettlementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "cancellation_loss",
                table: "bookings",
                newName: "operator_loss");

            migrationBuilder.RenameColumn(
                name: "cancellation_reason",
                table: "bookings",
                newName: "cancellation_type");

            migrationBuilder.AlterColumn<string>(
                name: "cancellation_type",
                table: "bookings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "admin_revenue",
                table: "bookings",
                type: "numeric(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "admin_revenue",
                table: "bookings");

            migrationBuilder.AlterColumn<string>(
                name: "cancellation_type",
                table: "bookings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "operator_loss",
                table: "bookings",
                newName: "cancellation_loss");

            migrationBuilder.RenameColumn(
                name: "cancellation_type",
                table: "bookings",
                newName: "cancellation_reason");
        }
    }
}
