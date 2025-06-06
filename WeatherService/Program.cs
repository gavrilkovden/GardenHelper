using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Globalization;
using WeatherService.Domain;
using WeatherService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient();

builder.Services.AddScoped<IWeatherService, WeatherService.Services.WeatherService>();

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ������������� RabbitMQ
var rabbitMqPublisher = app.Services.GetRequiredService<IRabbitMqPublisher>();
await rabbitMqPublisher.InitializeAsync("rabbitmq");  // ���������, ��� ����������� ���������� ��� �����

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//��� ���������� �������� � �������� ���� ������ � ������, ���� �� ��� ���
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    dbContext.Database.Migrate();
}

app.Run();


