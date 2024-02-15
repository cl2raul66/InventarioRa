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
    }

    private async void HandleNotification(string message)
    {
        await GetClients();
    }

    [ObservableProperty]
    ObservableCollection<string>? clients;

    [ObservableProperty]
    string? selectedClient;

    [RelayCommand]
    async Task AddCliente()
    {
        var result = await Shell.Current.DisplayPromptAsync("Agregar cliente", "Nombre:");
        if (string.IsNullOrEmpty(result))
        {
            if (result == string.Empty)
            {
                await Shell.Current.DisplayAlert("Error", "Debe poner un nombre, vuelva a intentar", "Cerrar");
            }
            return;
        }

        string name = result.Trim().ToUpper();

        if (Clients?.Any(x => x.Equals(name)) ?? false)
        {
            await Shell.Current.DisplayAlert("Error", "Ya existe ese nombre, favor coloque otro", "Cerrar");
            return;
        }

        if (!Clients?.Any() ?? true)
        {
            Clients = [];
        }

        bool resultInsert = await clientesServ.CreateClienteAsync(new Client { Id = Guid.NewGuid().ToString(), Name = name });

        if (resultInsert)
        {
            Clients!.Add(name);
        }
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
    public async Task InitializeNotificationApi()
    {
        await apiServ.ConnectAsync();
        apiServ.OnNotificationsReceived += NotificationApiServ_OnNotificationReceived;
    }

    private async void NotificationApiServ_OnNotificationReceived(string channel, string message)
    {        
        switch (channel)
        {
            case "ReceiveMessage":
                await GetClients();
                break;
            case "ReceiveStatusMessage":
                break;
        }
    }

    async Task GetClients()
    {
        if (await clientesServ.ExistAsync())
        {
            var names = await clientesServ.GetNames();
            Clients = new(names);
        }
    }
    #endregion
}
