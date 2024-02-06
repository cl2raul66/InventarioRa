using InventarioRa.ViewModels;

namespace InventarioRa.Views;

public partial class PgClientes : ContentPage
{
	public PgClientes(PgClientesViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}