using MottuApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Oracle.EntityFrameworkCore;
using MottuApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Registrar serviços
builder.Services.AddScoped<AuthService>();
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

app.UseAuthorization();

app.MapControllers();

// Popular banco com dados de exemplo em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MottuDbContext>();
        await SeedData.SeedAsync(context);
    }
}

app.Run();
