using System.Text;
using System.Text.Json.Serialization;
using BusBookingApp.Data;
using BusBookingApp.Middleware;
using BusBookingApp.Models;
using BusBookingApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
const string LocalFrontendCorsPolicy = "LocalFrontend";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalFrontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be configured and at least 32 bytes long.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BusBookingSystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BusBookingSystem";

builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.IterationCount = 210_000;
});
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<BookingEmailService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<SeatLayoutService>();
builder.Services.AddScoped<CancellationService>();
builder.Services.AddHostedService<SeatHoldCleanupService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.ExecuteSqlRaw(
        """
        DO $$
        BEGIN
            -- Fix users table password column
            IF EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'users'
                  AND column_name = 'password'
            ) AND NOT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'users'
                  AND column_name = 'password_hash'
            ) THEN
                ALTER TABLE public.users RENAME COLUMN password TO password_hash;
            END IF;

            -- Fix bookings table missing columns
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'bookings') THEN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'contact_email') THEN
                    ALTER TABLE public.bookings ADD COLUMN contact_email character varying(150);
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'contact_phone') THEN
                    ALTER TABLE public.bookings ADD COLUMN contact_phone character varying(20);
                END IF;
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'cancellation_loss')
                   AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'operator_loss') THEN
                    ALTER TABLE public.bookings RENAME COLUMN cancellation_loss TO operator_loss;
                END IF;
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'cancellation_reason')
                   AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'cancellation_type') THEN
                    ALTER TABLE public.bookings RENAME COLUMN cancellation_reason TO cancellation_type;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'refund_amount') THEN
                    ALTER TABLE public.bookings ADD COLUMN refund_amount numeric(10,2);
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'operator_loss') THEN
                    ALTER TABLE public.bookings ADD COLUMN operator_loss numeric(10,2);
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'admin_revenue') THEN
                    ALTER TABLE public.bookings ADD COLUMN admin_revenue numeric(10,2);
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'cancelled_at') THEN
                    ALTER TABLE public.bookings ADD COLUMN cancelled_at timestamp;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'bookings' AND column_name = 'cancellation_type') THEN
                    ALTER TABLE public.bookings ADD COLUMN cancellation_type character varying(30);
                END IF;
            END IF;

            -- Fix legacy buses columns created with quoted PascalCase names
            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'buses') THEN
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'buses' AND column_name = 'Company')
                   AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'buses' AND column_name = 'company') THEN
                    ALTER TABLE public.buses RENAME COLUMN "Company" TO company;
                END IF;

                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'buses' AND column_name = 'RegistrationNumber')
                   AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'buses' AND column_name = 'registration_number') THEN
                    ALTER TABLE public.buses RENAME COLUMN "RegistrationNumber" TO registration_number;
                END IF;

                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'buses' AND column_name = 'Type')
                   AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'buses' AND column_name = 'type') THEN
                    ALTER TABLE public.buses RENAME COLUMN "Type" TO type;
                END IF;

                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'buses' AND column_name = 'company') THEN
                    ALTER TABLE public.buses ADD COLUMN company character varying(150) NOT NULL DEFAULT '';
                END IF;

                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'buses' AND column_name = 'registration_number') THEN
                    ALTER TABLE public.buses ADD COLUMN registration_number character varying(100) NOT NULL DEFAULT '';
                END IF;

                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'buses' AND column_name = 'type') THEN
                    ALTER TABLE public.buses ADD COLUMN type character varying(50) NOT NULL DEFAULT 'ACSeater';
                END IF;

                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'buses'
                      AND column_name = 'type'
                      AND data_type IN ('integer', 'smallint', 'bigint')
                ) THEN
                    ALTER TABLE public.buses
                    ALTER COLUMN type TYPE character varying(50)
                    USING CASE type
                        WHEN 1 THEN 'ACSeater'
                        WHEN 2 THEN 'ACSleeper'
                        WHEN 3 THEN 'NonACSeater'
                        WHEN 4 THEN 'NonACSleeper'
                        ELSE 'ACSeater'
                    END;
                END IF;
            END IF;
        END $$;
        """);
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(LocalFrontendCorsPolicy);

app.UseAuthentication();   // ✅ MUST
app.UseAuthorization();    // ✅ MUST

app.MapControllers();

app.Run();
