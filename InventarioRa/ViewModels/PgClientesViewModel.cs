using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Models;
using InventarioRa.Servicios;
using System.Collections.ObjectModel;

namespace InventarioRa.ViewModels;

public partial class PgClientesViewModel : ObservableRecipient
{
    readonly IClientesServicio clientesServ;

    public PgClientesViewModel(IClientesServicio clientesServicio)
    {
        clientesServ = clientesServicio;
        GetClients();
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

        bool resultInsert = clientesServ.Insert(new Client { Id = Guid.NewGuid().ToString(), Name = name });
        if (resultInsert)
        {
            Clients!.Add(name);
        }
    }

    [RelayCommand]
    void Eliminar()
    {
        var id = clientesServ.GetId(SelectedClient!);
        if (string.IsNullOrEmpty(id))
        {
            return;
        }
        bool resultRemove = clientesServ.Delete(id);
        if (resultRemove)
        {
            Clients!.Remove(SelectedClient!);
        }
    }

    [RelayCommand]
    async Task GoToBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }


    #region Extra
    void GetClients()
    {
        if (clientesServ.Exist)
        {
            Clients = new(clientesServ.GetNames());
        }
    }
    #endregion
}
