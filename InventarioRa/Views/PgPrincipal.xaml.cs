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
    }
}
