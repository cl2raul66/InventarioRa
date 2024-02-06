using InventarioRa.ViewModels;

namespace InventarioRa.Views;

public partial class PgInventario : ContentPage
{
	public PgInventario(PgInventarioViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //(BindingContext as PgInventarioViewModel)!.Verinventario();
    }
}
