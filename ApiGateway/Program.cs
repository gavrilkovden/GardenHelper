using Ocelot.DependencyInjection;
using Ocelot.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Загружаем конфигурацию из ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Регистрируем Ocelot
builder.Services.AddOcelot();

var configSection = builder.Configuration.GetSection("Routes");

if (!configSection.Exists())
{
    Console.WriteLine("Ocelot routes section not found!");
}
else
{
    Console.WriteLine("Ocelot routes section loaded.");
}


var app = builder.Build();

// Используем Ocelot
await app.UseOcelot();

app.Run();
