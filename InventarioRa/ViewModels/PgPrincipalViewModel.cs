using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Servicios;
using InventarioRa.Views;

namespace InventarioRa.ViewModels;

public partial class PgPrincipalViewModel : ObservableRecipient
{
    readonly IDespachosServicio despachosServ;
    readonly DateTime ToDay;

    public PgPrincipalViewModel(IDespachosServicio despachosServicio, IInventarioServicio inventarioServicio)
    {
        ToDay = DateTime.Now;
        despachosServ = despachosServicio;
        GetUsadas();
        GetVentas();
        totalArticulos = inventarioServicio.TotalStock.ToString("00");
    }

    [ObservableProperty]
    string? totalArticulos;

    [ObservableProperty]
    string? usadas;

    [ObservableProperty]
    string? ventas;

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

    #region Extra
    void GetVentas()
    {
        Ventas = despachosServ.GetAllByDate(FirstDayOfWeek(ToDay), ToDay).Where(x => x.IsSale).Count().ToString("00") ?? "00";
    }

    void GetUsadas()
    {
        Usadas = despachosServ.GetAllByDate(FirstDayOfWeek(ToDay), ToDay).Where(x => !x.IsSale).Count().ToString("00") ?? "00";
    }

    DateTime FirstDayOfWeek(DateTime? datetime = null) {
        var now = datetime is null ? DateTime.Now : datetime.Value;
        DayOfWeek dayOfWeek = now.DayOfWeek;
        int daysUntilFirstDayOfWeek = ((int)dayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return now.AddDays(-daysUntilFirstDayOfWeek);
    }
    #endregion
}
