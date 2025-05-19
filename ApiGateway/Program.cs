using Ocelot.DependencyInjection;
using Ocelot.Middleware;


var builder = WebApplication.CreateBuilder(args);

// ��������� ������������ �� ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ������������ Ocelot
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

// ���������� Ocelot
await app.UseOcelot();

app.Run();
