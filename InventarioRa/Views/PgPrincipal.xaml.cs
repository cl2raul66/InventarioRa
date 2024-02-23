using InventarioRa.ViewModels;

namespace InventarioRa.Views;

public partial class PgPrincipal : ContentPage
{
    public PgPrincipal(PgPrincipalViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await (BindingContext as PgPrincipalViewModel)!.InitializeNotificationApi();
        if ((BindingContext as PgPrincipalViewModel)!.IsApiHealthy)
        {
            await (BindingContext as PgPrincipalViewModel)!.GetTotalarticulos();
            await (BindingContext as PgPrincipalViewModel)!.GetTotalventas();
            await (BindingContext as PgPrincipalViewModel)!.GetTotaluso();
        }
    }
}
