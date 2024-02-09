using InventarioRa.Servicios;
using InventarioRa.ViewModels;

namespace InventarioRa.Views;

public partial class PgPrincipal : ContentPage
{
    readonly IApiClientService apiClientServ;

    public PgPrincipal(PgPrincipalViewModel vm, IApiClientService apiClientService)
    {
        InitializeComponent();

        apiClientServ = apiClientService;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await (BindingContext as PgPrincipalViewModel)!.GetStatusapi();
    }
}
