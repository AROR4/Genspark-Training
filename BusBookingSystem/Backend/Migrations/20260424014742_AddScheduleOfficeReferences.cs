using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleOfficeReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "destination_office_id",
                table: "bus_schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source_office_id",
                table: "bus_schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_schedule_destination_office",
                table: "bus_schedules",
                column: "destination_office_id");

            migrationBuilder.CreateIndex(
                name: "idx_schedule_source_office",
                table: "bus_schedules",
                column: "source_office_id");

            migrationBuilder.AddForeignKey(
                name: "FK_bus_schedules_operator_offices_destination_office_id",
                table: "bus_schedules",
                column: "destination_office_id",
                principalTable: "operator_offices",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_bus_schedules_operator_offices_source_office_id",
                table: "bus_schedules",
                column: "source_office_id",
                principalTable: "operator_offices",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bus_schedules_operator_offices_destination_office_id",
                table: "bus_schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_bus_schedules_operator_offices_source_office_id",
                table: "bus_schedules");

            migrationBuilder.DropIndex(
                name: "idx_schedule_destination_office",
                table: "bus_schedules");

            migrationBuilder.DropIndex(
                name: "idx_schedule_source_office",
                table: "bus_schedules");

            migrationBuilder.DropColumn(
                name: "destination_office_id",
                table: "bus_schedules");

            migrationBuilder.DropColumn(
                name: "source_office_id",
                table: "bus_schedules");
        }
    }
}
