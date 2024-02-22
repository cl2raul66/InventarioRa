using Microsoft.AspNetCore.SignalR;
using Spectre.Console;
using InventarioRaServer.Services;
using InventarioRaServer.Tools;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClientesServicio, ClientesServicio>();
builder.Services.AddSingleton<IInventarioServicio, InventarioServicio>();
builder.Services.AddSingleton<IDespachosServicio, DespachosServicio>();
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

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
    _ = endpoints.MapHealthChecks("/healthchecks", new HealthCheckOptions
    {
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    });
});
app.MapControllers();

var applicationLifetime = app.Services.GetService<IHostApplicationLifetime>();
applicationLifetime?.ApplicationStopping.Register(async () =>
    {
        if (hubContext is not null)
        {
            AnsiConsole.MarkupLine("[red]El servidor va a detenerse[/]");
            NotificationHub.ServerStatus = "El servidor va a detenerse";
            await hubContext.Clients.All.SendAsync("ReceiveStatusMessage", NotificationHub.ServerStatus);
        }
    });

app.Run();
