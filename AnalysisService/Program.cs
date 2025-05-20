using AnalysisService.Cache;
using AnalysisService.Consumers;
using AnalysisService.Services;
using StackExchange.Redis;
using System.Globalization;


CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration["Redis:ConnectionString"];  
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddSingleton<PlantDataConsumer>();
builder.Services.AddSingleton<WeatherDataConsumer>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddHostedService<InitRabbitMQService>();

builder.Services.AddSingleton<IRedisBufferService, RedisBufferService>();
builder.Services.AddHttpClient<IAIAnalysisService, AIAnalysisService>();

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
