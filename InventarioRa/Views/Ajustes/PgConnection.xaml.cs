using InventarioRa.ViewModels;

namespace InventarioRa.Views;

public partial class PgConnection : ContentPage
{
    public PgConnection(PgConnectionViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}