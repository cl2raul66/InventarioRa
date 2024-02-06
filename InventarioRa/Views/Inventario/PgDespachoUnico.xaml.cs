namespace InventarioRa.Views;

public partial class PgDespachoUnico : ContentPage
{
    public PgDespachoUnico()
    {
        InitializeComponent();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        PickerClient.IsEnabled = !PickerClient.IsEnabled;
        GridOfEntryClient.IsVisible = !GridOfEntryClient.IsVisible;
    }
}