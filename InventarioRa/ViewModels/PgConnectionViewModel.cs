using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Servicios;
using System.ComponentModel.DataAnnotations;

namespace InventarioRa.ViewModels;

public partial class PgConnectionViewModel : ObservableValidator
{
    readonly IApiService apiServ;

    public PgConnectionViewModel(IApiService apiService)
    {
        apiServ = apiService;
    }

    [ObservableProperty]
    [Required]
    [Url]
    string? urlApi;

    [ObservableProperty]
    bool statusApi;

    [RelayCommand]
    async Task GoToBack() => await Shell.Current.GoToAsync("..", true);

    [RelayCommand]
    async Task Test()
    {
        ValidateAllProperties();
        if (HasErrors || string.IsNullOrEmpty(UrlApi))
        {
            StatusApi = false;
            return;
        }
        var result = await apiServ.SetUrl(UrlApi);
        if (result)
        {
            await apiServ.ConnectAsync();
            StatusApi = apiServ.IsConnected;
        }
    }

    [RelayCommand]
    async Task Save()
    {
        await apiServ.SetUrl(UrlApi!);
        await GoToBack();
    }
}
