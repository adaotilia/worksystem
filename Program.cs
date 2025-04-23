using worksystem.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using worksystem.Services;
using worksystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

// Adatbázis kapcsolat konfiguráció
var dbPassword = builder.Configuration["DB_PASSWORD"];
if (string.IsNullOrEmpty(dbPassword))
{
    throw new Exception("DB_PASSWORD environment variable is missing");
}

Console.WriteLine($"DB_PASSWORD environment variable: {dbPassword}"); 

var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(rawConnectionString))
{
    throw new Exception("DefaultConnection is missing in appsettings.json");
}

var connectionString = rawConnectionString.Replace("${DB_PASSWORD}", dbPassword);
Console.WriteLine($"Final connection string: {connectionString}");

// DbContext regisztráció
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//Enum-ek JSON-ba alakítása szöveges megjelenítéshez és DateOnly, TimeOnly adattípusok JSON-ba alakítása.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });

//JWT authentication beállítása.
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("JWT Key is missing"));

// Több elfogadott Issuer és Audience a konfigurációból
var validIssuers = builder.Configuration.GetSection("Jwt:Issuer").Get<string[]>();
var validAudiences = builder.Configuration.GetSection("Jwt:Audience").Get<string[]>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuers = validIssuers,
            ValidateAudience = true,
            ValidAudiences = validAudiences,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

//Authorization beállítása.
builder.Services.AddAuthorization(options =>
{
    // Alap authentikációs policy
    options.AddPolicy("Authenticated", new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build());

    // Szerepkör alapú policy-k
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));

    options.AddPolicy("ManagerOnly", policy => 
        policy.RequireAuthenticatedUser()
              .RequireRole("Manager"));

    options.AddPolicy("ManagerOrAbove", policy => 
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin", "Manager"));

    options.AddPolicy("EmployeeOrAbove", policy => 
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin", "Manager", "Employee"));
});

//Bcrypt jelszó hash-olás implementálása.
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

//Services beállítása.
builder.Services.RegisterAppServices();

//Automapper beállítása
builder.Services.AddAutoMapper(typeof(Program));

var allowedOrigins = builder.Configuration.GetSection("FrontendUrls").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger konfiguráció
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
var app = builder.Build();

// Swagger engedélyezése Production környezetben is
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();