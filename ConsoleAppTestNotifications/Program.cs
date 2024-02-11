using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/serverStatusHub")
    .WithAutomaticReconnect()
    .Build();

connection.On<string>("ReceiveStatusMessage", (message) =>
{
    Console.WriteLine($"Notificación recibida: {message}");

    if (message == "El servidor va a detenerse")
    {
        Console.WriteLine("El servidor se está deteniendo. Intentando reconectar en 3 segundos...");
        Reconnect();
    }
});

async void Reconnect()
{
    while (true)
    {
        try
        {
            if (connection.State == HubConnectionState.Disconnected)
            {
                await connection.StartAsync();
                Console.WriteLine("Cliente reconectado.");
                break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al reconectar: {ex.Message}");
            Console.WriteLine("Intentando reconectar en 3 segundos...");
            await Task.Delay(3000);
        }
    }
}


while (true)
{
    try
    {
        await connection.StartAsync();
        Console.WriteLine("Cliente conectado. Presiona una tecla para salir.");
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al conectar con el servidor: {ex.Message}");
        Console.WriteLine("Intentando reconectar en 3 segundos...");
        await Task.Delay(3000);
    }
}

// Escucha las pulsaciones de teclas en un hilo separado
await Task.Run(() =>
{
    Console.ReadKey();
    Environment.Exit(0);
});

while (true)
{
    await Task.Delay(1000);
}
