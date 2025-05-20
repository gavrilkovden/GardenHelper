using DataInputService.Domain;
using DataInputService.Mappings;
using DataInputService.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<PlantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddScoped<IPlantService, PlantService>();

var app = builder.Build();

// Инициализация RabbitMQ
var rabbitMqPublisher = app.Services.GetRequiredService<IRabbitMqPublisher>();
await rabbitMqPublisher.InitializeAsync("rabbitmq");  // Убедитесь, что используете правильное имя хоста

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//для применения миграций и создания базы данных в докере, если ее еще нет
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PlantDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
