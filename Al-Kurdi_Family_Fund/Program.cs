using Al_Kurdi_Family_Fund.Data;
using Al_Kurdi_Family_Fund.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── 1. Database ─────────────────────────────────────────
// Tell ASP.NET: "use PostgreSQL with our connection string"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// ADD THIS LINE ↓
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<MemberService>();

builder.Services.AddScoped<TransactionService>();

// ─── 2. JWT Authentication ────────────────────────────────
// Tell ASP.NET how to validate the tokens your JS frontend sends
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// ─── 3. Controllers ──────────────────────────────────────
builder.Services.AddControllers();

// ─── 4. CORS ─────────────────────────────────────────────
// Allow your HTML frontend to call the API
// (browser blocks requests between different origins by default)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5500",   // VS Code Live Server
                "http://127.0.0.1:5500",   // VS Code Live Server alt
                "http://localhost:3000",   // any local dev server
                "https://your-railway-app.up.railway.app" // production later
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ─── 5. Swagger (API testing tool) ───────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Al-Kurdi Family Fund API", Version = "v1" });

    // Tell Swagger that this API uses Bearer tokens
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ──────────────────────────────────
// The ORDER here matters — each request passes through these in sequence

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Visit /swagger to test your APIs visually
}

app.UseCors("AllowFrontend");        // 1. Handle CORS first
app.UseAuthentication();             // 2. Check the JWT token
app.UseAuthorization();              // 3. Check the role (Admin/Member)
app.MapControllers();                // 4. Route to the right controller

app.Run();