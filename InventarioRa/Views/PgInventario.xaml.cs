using InventarioRa.ViewModels;

namespace InventarioRa.Views;

public partial class PgInventario : ContentPage
{
	public PgInventario(PgInventarioViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        await (BindingContext as PgInventarioViewModel)!.Inicializar();
    }
}
