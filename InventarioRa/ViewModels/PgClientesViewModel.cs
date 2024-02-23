using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Models;
using InventarioRa.Servicios;
using System.Collections.ObjectModel;

namespace InventarioRa.ViewModels;

public partial class PgClientesViewModel : ObservableRecipient
{
    readonly IClientesForApiServicio clientesServ;
    readonly IApiService apiServ;

    public PgClientesViewModel(IApiService apiService, IClientesForApiServicio clientesServicio)
    {
        apiServ = apiService;
        clientesServ = clientesServicio;
        apiServ.OnNotificationsReceived += ApiServ_OnNotificationReceived;
    }

    [ObservableProperty]
    bool isApiHealthy;

    [ObservableProperty]
    ObservableCollection<Client>? clients;

    [ObservableProperty]
    Client? selectedClient;

    [RelayCommand]
    async Task AddCliente()
    {
        var result = await Shell.Current.DisplayPromptAsync("Agregar cliente", "Nombre:");
        if (string.IsNullOrEmpty(result))
        {
            if (result == string.Empty)
            {
                await MensajeAlInsertar("Debe poner un nombre, vuelva a intentar");
            }
            return;
        }

        string name = result.Trim().ToUpper();

        if (!Clients?.Any() ?? true)
        {
            Clients = [];
        }

        if (Clients!.Any(x => x.Name == name))
        {
            await MensajeAlInsertar("Ya existe ese nombre, favor coloque otro");
            return;
        }
        Client newClient = new() { Id = Guid.NewGuid().ToString(), Name = name };
        _ = await clientesServ.CreateClienteAsync(newClient);
    }

    [RelayCommand]
    void Eliminar()
    {
        //var id = clientesServ.GetId(SelectedClient!);
        //if (string.IsNullOrEmpty(id))
        //{
        //    return;
        //}
        //bool resultRemove = clientesServ.Delete(id);
        //if (resultRemove)
        //{
        //    Clients!.Remove(SelectedClient!);
        //}
    }

    [RelayCommand]
    async Task GoToBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    #region Extra
    private async void ApiServ_OnNotificationReceived(string channel, string message)
    {
        switch (channel)
        {
            case "ReceiveMessage":
                Console.WriteLine($"Mensaje recibido: {message}");
                if (message.Contains("Un nuevo cliente ha sido agregado") || message.Contains("Un cliente ha sido eliminado"))
                {
                    await GetClients();
                }
                break;
            case "ReceiveStatusMessage":
                IsApiHealthy = message == "El servidor está iniciando";
                break;
        }
    }

    public async Task GetClients()
    {
        if (await clientesServ.ExistAsync())
        {
            var getClients = await clientesServ.GetAllClientesAsync();
            Clients = new(getClients!);
        }
    }

    async Task MensajeAlInsertar(string mensaje)
    {
        CancellationTokenSource cancellationTokenSource = new();
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;

        var toast = Toast.Make(mensaje, duration, fontSize);

        await toast.Show(cancellationTokenSource.Token);
    }
    #endregion
}
