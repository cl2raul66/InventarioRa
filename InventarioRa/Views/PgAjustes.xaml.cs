using InventarioRa.ViewModels;

namespace InventarioRa.Views;

public partial class PgAjustes : ContentPage
{
	public PgAjustes(PgAjustesViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}