using System.Text;
using InventorySystem.API.Middleware;
using InventorySystem.Application;
using InventorySystem.Domain.Interfaces;
using InventorySystem.Infrastructure.Persistence;
using InventorySystem.Infrastructure.Persistence.Repositories;
using InventorySystem.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the token returned by POST /api/auth/login (no \"Bearer \" prefix needed here).",
    });
    options.AddSecurityRequirement(document =>
    {
        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", document);
        return new OpenApiSecurityRequirement { [schemeRef] = [] };
    });
});

builder.Services.AddApplication();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Configured against IOptions<JwtSettings> (resolved lazily, on first use)
// rather than reading configuration directly here. WebApplicationFactory-based
// integration tests inject config overrides around the builder.Build() call,
// so anything read from builder.Configuration before Build() - as a direct
// Get<JwtSettings>() call here would do - never sees those overrides.
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((bearerOptions, jwtSettingsOptions) =>
    {
        var jwtSettings = jwtSettingsOptions.Value;
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

// Every endpoint requires an authenticated caller by default; [AllowAnonymous]
// (used on AuthController) opts a specific endpoint back out.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await AppDbContextSeed.SeedAsync(db, passwordHasher);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes the top-level-statement entry point so WebApplicationFactory<Program>
// in the integration test project can find it.
public partial class Program
{
}
