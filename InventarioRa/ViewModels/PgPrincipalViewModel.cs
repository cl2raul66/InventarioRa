using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventarioRa.Servicios;
using InventarioRa.Views;

namespace InventarioRa.ViewModels;

public partial class PgPrincipalViewModel : ObservableRecipient
{
    readonly IApiService apiServ;

    public PgPrincipalViewModel(IApiService apiService)
    {
        IsActive = true;
        Preferences.Default.Set("hoy", DateTime.Now);
        apiServ = apiService;
        TotalArticulos = Preferences.Default.Get<string?>("totalarticulos", null);
        Ventas = Preferences.Default.Get<string?>("totalventas", null);
        Usadas = Preferences.Default.Get<string?>("totaluso", null);
    }

    [ObservableProperty]
    bool isApiHealthy;

    [ObservableProperty]
    string? totalArticulos;

    [ObservableProperty]
    string? usadas;

    [ObservableProperty]
    string? ventas;

    [RelayCommand]
    async Task GoToAjustes()
    {
        await Shell.Current.GoToAsync(nameof(PgAjustes), true);
    }

    [RelayCommand]
    async Task GoToDetalle()
    {
        if (string.IsNullOrEmpty(apiServ.GetServerUrl))
        {
            await MensajeIrAjustes();
            return;
        }
        if (!apiServ.IsConnected)
        {
            await MensajeReconectar();
            return;
        }
        await Shell.Current.GoToAsync(nameof(PgInventario), true);
    }

    [RelayCommand]
    async Task GoToDespacho()
    {
        if (string.IsNullOrEmpty(apiServ.GetServerUrl))
        {
            await MensajeIrAjustes();
            return;
        }
        if (!apiServ.IsConnected)
        {
            await MensajeReconectar();
            return;
        }
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgDespachoVarios)}", true);
    }

    [RelayCommand]
    async Task GoToAgregarEntrada()
    {
        if (string.IsNullOrEmpty(apiServ.GetServerUrl))
        {
            await MensajeIrAjustes();
            return;
        }
        if (!apiServ.IsConnected)
        {
            await MensajeReconectar();
            return;
        }
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgAgregarEntrada)}", true);
    }

    [RelayCommand]
    async Task GoToClientes()
    {
        if (string.IsNullOrEmpty(apiServ.GetServerUrl))
        {
            await MensajeIrAjustes();
            return;
        }
        if (!apiServ.IsConnected)
        {
            await MensajeReconectar();
            return;
        }
        await Shell.Current.GoToAsync(nameof(PgClientes), true);
    }

    [RelayCommand]
    public async Task ConectarToApi()
    {
        if (string.IsNullOrEmpty(apiServ.GetServerUrl))
        {
            IsApiHealthy = false;
            await MensajeIrAjustes();
            return;
        }
        apiServ.OnNotificationsReceived += ApiServ_OnNotificationReceived;
        IsApiHealthy = apiServ.IsConnected;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        WeakReferenceMessenger.Default.Register<PgPrincipalViewModel, string, string>(this, "totalarticulos", (r, m) =>
        {
            r.TotalArticulos = m;
            Preferences.Default.Set("totalarticulos", m);
        });
        WeakReferenceMessenger.Default.Register<PgPrincipalViewModel, string, string>(this, "totalventas", (r, m) =>
        {
            r.Ventas = m;
            Preferences.Default.Set("totalventas", m);
        });
        WeakReferenceMessenger.Default.Register<PgPrincipalViewModel, string, string>(this, "totaluso", (r, m) =>
        {
            r.Usadas = m;
            Preferences.Default.Set("totaluso", m);
        });
    }

    #region Extra
    public async Task InitializeNotificationApi(bool conMensaje)
    {
        if (string.IsNullOrEmpty(apiServ.GetServerUrl))
        {
            IsApiHealthy = false;
            if (conMensaje)
            {
                await MensajeIrAjustes();
            }
            return;
        }
        await apiServ.ConnectAsync();
        apiServ.OnNotificationsReceived += ApiServ_OnNotificationReceived;
        IsApiHealthy = apiServ.IsConnected;
    }

    private void ApiServ_OnNotificationReceived(string channel, string message)
    {
        switch (channel)
        {
            case "ReceiveMessage":
                Console.WriteLine($"Mensaje recibido: {message}");
                if (message.Contains("Un nuevo inventario ha sido agregado")
                    || message.Contains("Un inventario ha sido actualizado")
                    || message.Contains("Un inventario ha sido eliminado"))
                {
                    //TotalArticulos = (await inventarioServ.TotalStockAsync()).ToString("00");
                }
                if (message.Contains("Un nuevo despacho ha sido agregado")
                    || message.Contains("Un despacho ha sido eliminado"))
                {
                }
                break;
            case "ReceiveStatusMessage":
                IsApiHealthy = message == "El servidor está iniciando";
                break;
        }
    }

    async Task MensajeIrAjustes() => await Shell.Current.DisplayAlert("Alerta", "¡Favor de ingresar una url del servidor de datos!", "Cerrar");

    async Task MensajeReconectar()
    {
        CancellationTokenSource cancellationTokenSource = new();

        string text = "¡No hay conexionó con el servidor!";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;

        var toast = Toast.Make(text, duration, fontSize);

        await toast.Show(cancellationTokenSource.Token);
    }
    #endregion
}
