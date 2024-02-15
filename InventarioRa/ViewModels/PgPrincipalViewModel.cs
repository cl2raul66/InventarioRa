using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Servicios;
using InventarioRa.Views;

namespace InventarioRa.ViewModels;

public partial class PgPrincipalViewModel : ObservableRecipient
{
    readonly IDespachosServicio despachosServ;
    readonly IInventarioServicio inventarioServ;
    readonly IApiService apiServ;
    readonly DateTime ToDay;

    public PgPrincipalViewModel(IApiService apiService, IDespachosServicio despachosServicio, IInventarioServicio inventarioServicio)
    {
        ToDay = DateTime.Now;
        apiServ = apiService;
        inventarioServ = inventarioServicio;
        despachosServ = despachosServicio;
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
        await Shell.Current.GoToAsync(nameof(PgInventario), true);
    }

    [RelayCommand]
    async Task GoToDespacho()
    {
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgDespachoVarios)}", true);
    }

    [RelayCommand]
    async Task GoToAgregarEntrada()
    {
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgAgregarEntrada)}", true);
    }

    [RelayCommand]
    async Task GoToClientes()
    {
        await Shell.Current.GoToAsync(nameof(PgClientes), true);
    }

    [RelayCommand]
    public async Task InitializeNotificationApi()
    {
        if (string.IsNullOrEmpty(apiServ.GetServerUrl))
        {
            IsApiHealthy = false;
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
                if (message.Contains("ha sido agregado") || message.Contains("ha sido eliminado"))
                {
                    GetUsadas();
                    GetVentas();
                    totalArticulos = inventarioServ.TotalStock.ToString("00");
                }
                break;
            case "ReceiveStatusMessage":
                IsApiHealthy = message == "El servidor está iniciando";
                break;
        }
    }

    void GetVentas()
    {
        Ventas = despachosServ.GetAllByDate(FirstDayOfWeek(ToDay), ToDay).Where(x => x.IsSale).Count().ToString("00") ?? "00";
    }

    void GetUsadas()
    {
        Usadas = despachosServ.GetAllByDate(FirstDayOfWeek(ToDay), ToDay).Where(x => !x.IsSale).Count().ToString("00") ?? "00";
    }

    DateTime FirstDayOfWeek(DateTime? datetime = null)
    {
        var now = datetime is null ? DateTime.Now : datetime.Value;
        DayOfWeek dayOfWeek = now.DayOfWeek;
        int daysUntilFirstDayOfWeek = ((int)dayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return now.AddDays(-daysUntilFirstDayOfWeek);
    }
}
