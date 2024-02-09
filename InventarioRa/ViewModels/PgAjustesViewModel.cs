using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventarioRa.Views;

namespace InventarioRa.ViewModels;

public partial class PgAjustesViewModel : ObservableObject
{
    [RelayCommand]
    async Task Back() => await Shell.Current.GoToAsync("..", true);

    [RelayCommand]
    async Task GoToPgConnection() => await Shell.Current.GoToAsync(nameof(PgConnection), true);
}
