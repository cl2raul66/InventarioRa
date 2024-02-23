using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Servicios;
using InventarioRa.Views;

namespace InventarioRa.ViewModels;

public partial class PgPrincipalViewModel : ObservableRecipient
{
    readonly DateTime hoy;
    readonly DateTime primerDiaSemana;
    readonly IApiService apiServ;
    readonly IInventarioForApiServicio inventarioForApiServ;
    readonly IDespachosForApiServicio despachosForApiServ;

    public PgPrincipalViewModel(IApiService apiService, IInventarioForApiServicio inventarioForApiServicio, IDespachosForApiServicio despachosForApiServicio)
    {
        IsActive = true;
        hoy = DateTime.Now.Date;
        primerDiaSemana = FirstDayOfWeek(hoy);
        apiServ = apiService;
        inventarioForApiServ = inventarioForApiServicio;
        despachosForApiServ = despachosForApiServicio;
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
    }

    #region Extra
    public async Task InitializeNotificationApi()
    {
        if (string.IsNullOrEmpty(apiServ.GetServerUrl))
        {
            IsApiHealthy = false;
            return;
        }
        await apiServ.ConnectAsync();
        if (apiServ.IsConnected)
        {
            inventarioForApiServ.Initialize(apiServ.HttpClient, apiServ.GetServerUrl);
            despachosForApiServ.Initialize(apiServ.HttpClient, apiServ.GetServerUrl);
        }
        apiServ.OnNotificationsReceived += ApiServ_OnNotificationReceived;
        IsApiHealthy = apiServ.IsConnected;
    }

    private async void ApiServ_OnNotificationReceived(string channel, string message)
    {
        switch (channel)
        {
            case "ReceiveMessage":
                Console.WriteLine($"Mensaje recibido: {message}");
                if (message.Contains("Un nuevo inventario ha sido agregado")
                    || message.Contains("Un inventario ha sido actualizado")
                    || message.Contains("Un inventario ha sido eliminado"))
                {
                    await GetTotalarticulos();
                }
                if (message.Contains("Un nuevo despacho ha sido agregado")
                    || message.Contains("Un despacho ha sido eliminado"))
                {
                    await GetTotaluso();
                    await GetTotalventas();
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

    DateTime FirstDayOfWeek(DateTime? datetime = null)
    {
        var now = datetime is null ? DateTime.Now : datetime.Value;
        DayOfWeek dayOfWeek = now.DayOfWeek;
        int daysUntilFirstDayOfWeek = ((int)dayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return now.AddDays(-daysUntilFirstDayOfWeek);
    }

    public async Task GetTotalarticulos()
    {
        TotalArticulos = (await inventarioForApiServ.TotalStockAsync()).ToString("0");
    }

    public async Task GetTotalventas()
    {
        Ventas = (await despachosForApiServ.GetAllByDateAsync(primerDiaSemana, hoy))?.Where(x => x.IsSale).Count().ToString("0") ?? "0";
    }

    public async Task GetTotaluso()
    {
        Usadas = (await despachosForApiServ.GetAllByDateAsync(primerDiaSemana, hoy))?.Where(x => !x.IsSale).Count().ToString("0") ?? "0";
    }
    #endregion
}
