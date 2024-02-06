using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventarioRa.Tools.Messages;

namespace InventarioRa.ViewModels;

public partial class PgBuscarInventarioViewModel : ObservableObject
{
    [ObservableProperty]
    string? findByText;

    [ObservableProperty]
    bool isByArticle = true;

    [ObservableProperty]
    bool visibleErrorinfo;

    [ObservableProperty]
    string? hasErrorinfo;

    [RelayCommand]
    async Task Buscar()
    {
        if (IsByArticle)
        {
            if (string.IsNullOrEmpty(FindByText))
            {
                VisibleErrorinfo = true;
                HasErrorinfo = "Complete los requeridos (*).";
                await Task.Delay(5000);
                VisibleErrorinfo = false;
                HasErrorinfo = string.Empty;
                return;
            }

            _ = WeakReferenceMessenger.Default.Send(new SearchInventoryForSearchChangedMessage(FindByText));
        }

        await GoToBack();
    }

    [RelayCommand]
    async Task GoToBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
