using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminRouteAndDisableFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_bus_operators_approval_status",
                table: "bus_operators");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "routes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_cancelled",
                table: "bus_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "bus_operators",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(@"
                INSERT INTO users (id, created_at, email, is_approved, name, password_hash, phone_number, role)
                VALUES (1, TIMESTAMP '2026-01-01 00:00:00', 'admin@bus.com', TRUE, 'Admin', 'AQAAAAIAAYagAAAAEHh3VDh4vhphjjdi+hxd+ftJk3d9f4La+cRv1Uvu499Lu5aXf+HhPsRS0eZ7HOxXUw==', NULL, 'ADMIN')
                ON CONFLICT (id) DO NOTHING;
            ");

            migrationBuilder.AddCheckConstraint(
                name: "ck_bus_operators_approval_status",
                table: "bus_operators",
                sql: "approval_status IN ('PENDING','APPROVED','REJECTED','DISABLED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_bus_operators_approval_status",
                table: "bus_operators");

            migrationBuilder.Sql("DELETE FROM users WHERE id = 1 AND email = 'admin@bus.com';");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "is_cancelled",
                table: "bus_schedules");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "bus_operators");

            migrationBuilder.AddCheckConstraint(
                name: "ck_bus_operators_approval_status",
                table: "bus_operators",
                sql: "approval_status IN ('PENDING','APPROVED','REJECTED')");
        }
    }
}
