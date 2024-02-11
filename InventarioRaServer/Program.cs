using Microsoft.AspNetCore.SignalR;
using Spectre.Console;
using InventarioRaServer.Services;
using InventarioRaServer.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClientesServicio, ClientesServicio>();
builder.Services.AddSingleton<IInventarioServicio, InventarioServicio>();
builder.Services.AddSingleton<IDespachosServicio, DespachosServicio>();
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var hubContext = app.Services.GetService<IHubContext<NotificationHub>>();
if (hubContext is not null)
{
    AnsiConsole.MarkupLine("[green]El servidor está iniciando[/]");
    NotificationHub.ServerStatus = "El servidor está iniciando";
    await hubContext.Clients.All.SendAsync("ReceiveStatusMessage", NotificationHub.ServerStatus);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapHub<NotificationHub>("/serverStatusHub");
});
app.MapControllers();

var applicationLifetime = app.Services.GetService<IHostApplicationLifetime>();
if (applicationLifetime is not null)
{
    applicationLifetime.ApplicationStopping.Register(async () =>
    {
        if (hubContext is not null)
        {
            AnsiConsole.MarkupLine("[red]El servidor va a detenerse[/]");
            NotificationHub.ServerStatus = "El servidor va a detenerse";
            await hubContext.Clients.All.SendAsync("ReceiveStatusMessage", NotificationHub.ServerStatus);
        }
    });
}

app.Run();
