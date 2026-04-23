using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentGatewayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gateway_order_id",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gateway_payment_id",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "paid_at",
                table: "payments",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_reference",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "failure_reason",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "gateway_order_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "gateway_payment_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "paid_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "payment_reference",
                table: "payments");
        }
    }
}
