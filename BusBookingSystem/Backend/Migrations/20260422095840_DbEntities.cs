using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class DbEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("ck_users_role", "role IN ('USER','OPERATOR','ADMIN')");
                });

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_city_id = table.Column<int>(type: "integer", nullable: true),
                    destination_city_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.id);
                    table.ForeignKey(
                        name: "FK_routes_cities_destination_city_id",
                        column: x => x.destination_city_id,
                        principalTable: "cities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_routes_cities_source_city_id",
                        column: x => x.source_city_id,
                        principalTable: "cities",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "bus_operators",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    company_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bus_operators", x => x.id);
                    table.ForeignKey(
                        name: "FK_bus_operators_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "buses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operator_id = table.Column<int>(type: "integer", nullable: true),
                    total_seats = table.Column<int>(type: "integer", nullable: false),
                    layout_json = table.Column<string>(type: "jsonb", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buses", x => x.id);
                    table.ForeignKey(
                        name: "FK_buses_bus_operators_operator_id",
                        column: x => x.operator_id,
                        principalTable: "bus_operators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operator_offices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operator_id = table.Column<int>(type: "integer", nullable: true),
                    city_id = table.Column<int>(type: "integer", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operator_offices", x => x.id);
                    table.ForeignKey(
                        name: "FK_operator_offices_bus_operators_operator_id",
                        column: x => x.operator_id,
                        principalTable: "bus_operators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_operator_offices_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "bus_schedules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bus_id = table.Column<int>(type: "integer", nullable: true),
                    route_id = table.Column<int>(type: "integer", nullable: true),
                    travel_date = table.Column<DateOnly>(type: "date", nullable: false),
                    departure_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    arrival_date = table.Column<DateOnly>(type: "date", nullable: false),
                    arrival_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    base_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bus_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_bus_schedules_buses_bus_id",
                        column: x => x.bus_id,
                        principalTable: "buses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bus_schedules_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "routes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "seats",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bus_id = table.Column<int>(type: "integer", nullable: true),
                    seat_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seats", x => x.id);
                    table.ForeignKey(
                        name: "FK_seats_buses_bus_id",
                        column: x => x.bus_id,
                        principalTable: "buses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    schedule_id = table.Column<int>(type: "integer", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    refund_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.id);
                    table.CheckConstraint("ck_bookings_status", "status IN ('PENDING','CONFIRMED','CANCELLED','EXPIRED')");
                    table.ForeignKey(
                        name: "FK_bookings_bus_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "bus_schedules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_bookings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "booking_passengers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    age = table.Column<int>(type: "integer", nullable: true),
                    gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    seat_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_passengers", x => x.id);
                    table.ForeignKey(
                        name: "FK_booking_passengers_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_passengers_seats_seat_id",
                        column: x => x.seat_id,
                        principalTable: "seats",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    refund_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    refunded_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.CheckConstraint("ck_payments_refund_status", "refund_status IN ('NONE','INITIATED','COMPLETED')");
                    table.CheckConstraint("ck_payments_status", "status IN ('SUCCESS','FAILED','PENDING')");
                    table.ForeignKey(
                        name: "FK_payments_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_booking_passengers_booking_id",
                table: "booking_passengers",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_passengers_seat_id",
                table: "booking_passengers",
                column: "seat_id");

            migrationBuilder.CreateIndex(
                name: "idx_booking_schedule",
                table: "bookings",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "idx_booking_user",
                table: "bookings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_bus_operators_user_id",
                table: "bus_operators",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_schedule_bus_date",
                table: "bus_schedules",
                columns: new[] { "bus_id", "travel_date" });

            migrationBuilder.CreateIndex(
                name: "IX_bus_schedules_route_id",
                table: "bus_schedules",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_buses_operator_id",
                table: "buses",
                column: "operator_id");

            migrationBuilder.CreateIndex(
                name: "IX_cities_name",
                table: "cities",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operator_offices_city_id",
                table: "operator_offices",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "IX_operator_offices_operator_id_city_id",
                table: "operator_offices",
                columns: new[] { "operator_id", "city_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_booking_id",
                table: "payments",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_routes_destination_city_id",
                table: "routes",
                column: "destination_city_id");

            migrationBuilder.CreateIndex(
                name: "IX_routes_source_city_id",
                table: "routes",
                column: "source_city_id");

            migrationBuilder.CreateIndex(
                name: "idx_seat_bus",
                table: "seats",
                column: "bus_id");

            migrationBuilder.CreateIndex(
                name: "IX_seats_bus_id_seat_number",
                table: "seats",
                columns: new[] { "bus_id", "seat_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_passengers");

            migrationBuilder.DropTable(
                name: "operator_offices");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "seats");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "bus_schedules");

            migrationBuilder.DropTable(
                name: "buses");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "bus_operators");

            migrationBuilder.DropTable(
                name: "cities");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
