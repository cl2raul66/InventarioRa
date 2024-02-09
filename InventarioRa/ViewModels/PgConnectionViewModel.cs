using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Servicios;
using System.ComponentModel.DataAnnotations;

namespace InventarioRa.ViewModels;

public partial class PgConnectionViewModel(IApiClientService apiClientService) : ObservableValidator
{
    readonly IApiClientService apiClientServ = apiClientService;
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
        StatusApi = await apiClientServ.Test(UrlApi);
    }

    [RelayCommand]
    async Task Save()
    {
        StatusApi = await apiClientServ.SetUrl(UrlApi!);
    }
}
