using InventarioRa.ViewModels;

namespace InventarioRa.Views;

public partial class PgBuscarInventario : ContentPage
{
    public PgBuscarInventario(PgBuscarInventarioViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}