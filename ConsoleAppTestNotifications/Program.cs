using ConsoleAppTestNotifications;
using Microsoft.AspNetCore.SignalR.Client;
using Spectre.Console;

//var connection = new HubConnectionBuilder()
//    .WithUrl("http://localhost:5000/serverStatusHub")
//    .WithAutomaticReconnect()
//    .Build();

//connection.On<string>("ReceiveStatusMessage", (message) =>
//{
//    AnsiConsole.MarkupLine($"[yellow]Notificación recibida: {message}[/]");

//    if (message == "El servidor va a detenerse")
//    {
//        AnsiConsole.MarkupLine("[red]El servidor se está deteniendo. Intentando reconectar en 3 segundos...[/]");
//        Reconnect();
//    }
//});

//connection.On<string>("ReceiveMessage", (message) =>
//{
//    AnsiConsole.MarkupLine($"[yellow]Notificación recibida: {message}[/]");
//});

//connection.Reconnected += async (connectionId) =>
//{
//    Console.WriteLine("Cliente conectado. Presiona una tecla para salir.");
//    await Task.CompletedTask;
//};

//connection.Closed += async (error) =>
//{
//    AnsiConsole.MarkupLine($"[red]Conexión cerrada: {error?.Message}[/]");
//    if (connection.State is HubConnectionState.Disconnected)
//    {
//        Console.WriteLine("Intentando reconectar en 3 segundos...");
//        await Task.Run(Reconnect);
//    }
//};

//async void Reconnect()
//{
//    while (true)
//    {
//        try
//        {
//            if (connection.State == HubConnectionState.Disconnected)
//            {
//                await connection.StartAsync();
//                AnsiConsole.MarkupLine("[green]Cliente reconectado.[/]");
//                break;
//            }
//        }
//        catch (Exception ex)
//        {
//            AnsiConsole.MarkupLine($"[red]Error al reconectar: {ex.Message} [/]");
//            Console.WriteLine("Intentando reconectar en 3 segundos...");
//            await Task.Delay(3000);
//        }
//    }
//}

//while (true)
//{
//    try
//    {
//        await connection.StartAsync();
//        AnsiConsole.MarkupLine("[green]Cliente conectado. Presiona una tecla para salir.[/]");
//        break;
//    }
//    catch (Exception ex)
//    {
//        AnsiConsole.MarkupLine($"[red]Error al conectar con el servidor: {ex.Message}[/]");
//        Console.WriteLine("Intentando reconectar en 3 segundos...");
//        await Task.Delay(3000);
//    }
//}

//// Escucha las pulsaciones de teclas en un hilo separado
//await Task.Run(() =>
//{
//    Console.ReadKey();
//    Environment.Exit(0);
//});

//while (true)
//{
//    await Task.Delay(1000);
//}

var apiService = new ApiService("http://localhost:5000/serverStatusHub");

apiService.OnStatusMessageReceived += (message) =>
{
    AnsiConsole.MarkupLine($"[yellow]Notificación recibida: {message}[/]");
};

apiService.OnMessageReceived += (message) =>
{
    AnsiConsole.MarkupLine($"[yellow]Notificación recibida: {message}[/]");
};

apiService.Connect();

// Escucha las pulsaciones de teclas en un hilo separado
await Task.Run(() =>
{
    Console.ReadKey();
    Environment.Exit(0);
});

while (true)
{
    Task.Delay(1000).Wait();
}