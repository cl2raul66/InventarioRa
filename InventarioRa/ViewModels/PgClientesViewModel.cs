using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Models;
using InventarioRa.Servicios;
using InventarioRa.Tools.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace InventarioRa.ViewModels;

public partial class PgClientesViewModel : ObservableObject
{
    readonly IClientesForApiServicio clientesServ;
    readonly IApiService apiServ;

    public PgClientesViewModel(IApiService apiService, IClientesForApiServicio clientesServicio)
    {
        apiServ = apiService;
        clientesServ = clientesServicio;
        apiServ.OnNotificationsReceived += ApiServ_OnNotificationReceived;
        IsApiHealthy = apiServ.IsConnected;
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
        var resultDisplayPrompt = await Shell.Current.DisplayPromptAsync("Agregar cliente", "Nombre:");
        if (string.IsNullOrEmpty(resultDisplayPrompt))
        {
            if (resultDisplayPrompt == string.Empty)
            {
                await MensajeAlInsertar("Debe poner un nombre, vuelva a intentar");
            }
            return;
        }

        string name = resultDisplayPrompt.Trim().ToUpper();

        Clients ??= [];

        if (Clients!.Any(x => x.Name == name))
        {
            await MensajeAlInsertar("Ya existe ese nombre, favor coloque otro");
            return;
        }
        Client newClient = new() { Id = Guid.NewGuid().ToString(), Name = name };
        _ = await clientesServ.CreateAsync(newClient);
    }

    [RelayCommand]
    async Task Eliminar()
    {
        _ = await clientesServ.DeleteAsync(SelectedClient!.Id!);
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
                var messageObj = message.Split(':');
                if (messageObj[1] == nameof(Client))
                {
                    Clients ??= [];                    
                    if (messageObj[0] == OperationType.Create.ToString())
                    {
                        Client Ele = (await clientesServ.GetByIdAsync(messageObj[2]))!;
                        Clients.Insert(0, Ele);
                    }
                    if (messageObj[0] == OperationType.Delete.ToString())
                    {
                        if (Clients.Count > 0)
                        {
                            Client deleteEle = Clients.First(x => x.Id == messageObj[2]);
                            Clients.Remove(deleteEle);
                        }
                    }
                }
                break;
            case "ReceiveStatusMessage":
                IsApiHealthy = message == ServerStatus.Running.ToString();
                break;
        }
    }

    public async Task GetClients()
    {
        if (await clientesServ.ExistAsync() && Clients is null)
        {
            var getClients = await clientesServ.GetAllAsync();
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

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsApiHealthy))
        {
            if (IsApiHealthy)
            {
                clientesServ.Initialize(apiServ.HttpClient, apiServ.GetServerUrl);
            }
        }
    }
    #endregion
}
