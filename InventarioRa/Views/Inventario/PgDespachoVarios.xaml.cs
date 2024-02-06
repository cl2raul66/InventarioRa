using CommunityToolkit.Maui.Core.Platform;
using System.Runtime.Versioning;

namespace InventarioRa.Views;

public partial class PgDespachoVarios : ContentPage
{
	public PgDespachoVarios()
	{
		InitializeComponent();
        CollectionView1.PropertyChanged += CollectionView1_PropertyChanged;
    }

    private void CollectionView1_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Height")
        {
            ScrollView1.ScrollToAsync(0, ScrollView1.ContentSize.Height, true);
        }
    }

    [UnsupportedOSPlatform("maccatalyst")]
    private async void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry && string.IsNullOrEmpty(entry.Text))
        {
            try
            {
                _ = await entry.HideKeyboardAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        PickerClient.IsEnabled = !PickerClient.IsEnabled;
        GridOfEntryClient.IsVisible = !GridOfEntryClient.IsVisible;
    }
}