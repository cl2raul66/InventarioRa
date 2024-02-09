using InventarioRaServer.Services;
using InventarioRaServer.Tools;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClientesServicio, ClientesServicio>();
builder.Services.AddSingleton<IInventarioServicio, InventarioServicio>();
builder.Services.AddSingleton<IDespachosServicio, DespachosServicio>();
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var hubContext = app.Services.GetService<IHubContext<NotificationHub>>();
    if (hubContext is not null)
    {
        await hubContext.Clients.All.SendAsync("ReceiveStatusMessage", "El servidor está iniciando");
    }
    await next.Invoke();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
    
app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapHub<NotificationHub>("/serverStatusHub");
});

app.Use(async (context, next) =>
{
    var hubContext = app.Services.GetService<IHubContext<NotificationHub>>();
    if (hubContext is not null)
    {
        await hubContext.Clients.All.SendAsync("ReceiveStatusMessage", "El servidor va a detenerse");
    }
    await next.Invoke();
});

app.Run();
