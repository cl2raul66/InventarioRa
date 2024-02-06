using InventarioRa.ViewModels;

namespace InventarioRa.Views;

public partial class PgPrincipal : ContentPage
{
    public PgPrincipal(PgPrincipalViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}
