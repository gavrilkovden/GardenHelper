using AnalysisService.Cache;
using AnalysisService.Consumers;
using AnalysisService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<PlantDataConsumer>();
builder.Services.AddSingleton<WeatherDataConsumer>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IRedisBufferService, RedisBufferService>();
builder.Services.AddSingleton<IAIAnalysisService, AIAnalysisService>();

var app = builder.Build();

var plantConsumer = app.Services.GetRequiredService<PlantDataConsumer>();
var weatherConsumer = app.Services.GetRequiredService<WeatherDataConsumer>();

_ = Task.Run(() => plantConsumer.StartConsumingAsync());
_ = Task.Run(() => weatherConsumer.StartConsumingAsync());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
