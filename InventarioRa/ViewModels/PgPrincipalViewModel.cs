﻿using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Servicios;
using InventarioRa.Views;

namespace InventarioRa.ViewModels;

public partial class PgPrincipalViewModel : ObservableRecipient
{
    readonly IDespachosForApiServicio despachosServ;
    readonly IInventarioForApiServicio inventarioServ;
    readonly IApiService apiServ;
    readonly DateTime ToDay;

    public PgPrincipalViewModel(IApiService apiService, IDespachosForApiServicio despachosServicio, IInventarioForApiServicio inventarioServicio)
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

    #region Extra
    private async void ApiServ_OnNotificationReceived(string channel, string message)
    {
        switch (channel)
        {
            case "ReceiveMessage":
                Console.WriteLine($"Mensaje recibido: {message}");
                if (message.Contains("ha sido agregado") || message.Contains("ha sido eliminado"))
                {
                    await GetUsadas();
                    await GetVentas();
                    totalArticulos = (await inventarioServ.TotalStockAsync()).ToString("00");
                }
                break;
            case "ReceiveStatusMessage":
                IsApiHealthy = message == "El servidor está iniciando";
                break;
        }
    }

    async Task GetVentas()
    {
        Ventas = (await despachosServ.GetAllByDateAsync(FirstDayOfWeek(ToDay), ToDay))?.Where(x => x.IsSale).Count().ToString("00") ?? "00";
    }

    async Task GetUsadas()
    {
        Usadas = (await despachosServ.GetAllByDateAsync(FirstDayOfWeek(ToDay), ToDay))?.Where(x => !x.IsSale).Count().ToString("00") ?? "00";
    }

    DateTime FirstDayOfWeek(DateTime? datetime = null)
    {
        var now = datetime is null ? DateTime.Now : datetime.Value;
        DayOfWeek dayOfWeek = now.DayOfWeek;
        int daysUntilFirstDayOfWeek = ((int)dayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return now.AddDays(-daysUntilFirstDayOfWeek);
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
