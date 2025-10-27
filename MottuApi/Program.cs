using MottuApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Oracle.EntityFrameworkCore;
using MottuApi.Services;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Adicionar Health Checks
builder.Services.AddHealthChecks();

// Configurar API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configurar autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MottuApiSecretKeyForDevelopmentAndProduction2025!@#$%";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MottuApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MottuApiUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Registrar serviços
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<PatioPrevisaoService>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<MottuDbContext>(options =>
        options.UseInMemoryDatabase("MottuDb"));
}
else
{
    builder.Services.AddDbContext<MottuDbContext>(options =>
        options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Mottu - Gestão de Funcionários e Pátios",
        Version = "v1",
        Description = "API RESTful para cadastro e login de funcionários da Mottu, com gestão de pátios e gerentes.",
        Contact = new OpenApiContact
        {
            Name = "Equipe de Desenvolvimento",
            Email = "equipe@mottu.com"
        }
    });

    // Configurar autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Mottu v1");
    c.RoutePrefix = "swagger"; // Mover para /swagger
});

app.UseDefaultFiles();
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Mapear Health Check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Popular banco com dados de exemplo em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MottuDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        await SeedData.SeedAsync(context, configuration);
    }
}

app.Run();

// Expor Program para testes de integração
public partial class Program { }
