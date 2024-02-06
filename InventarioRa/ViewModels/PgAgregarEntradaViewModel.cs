using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventarioRa.Models;
using InventarioRa.Tools.Messages;
using System.ComponentModel.DataAnnotations;

namespace InventarioRa.ViewModels;

public partial class PgAgregarEntradaViewModel : ObservableValidator
{
    [ObservableProperty]
    [Required]
    [MinLength(3)]
    string? articulo;

    [ObservableProperty]
    [Required]
    [MinLength(0)]
    string? cantidad;

    [ObservableProperty]
    bool visibleErrorinfo;

    [RelayCommand]
    async Task Guardar()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            VisibleErrorinfo = true;
            await Task.Delay(5000);
            VisibleErrorinfo = false;
            return;
        }

        _ = WeakReferenceMessenger.Default.Send(new SendArticleentryChangedMessage(new ArticleEntry { Name = Articulo!.Trim().ToUpper(), Amount = double.Parse(Cantidad!) }));

        Articulo = string.Empty;
        Cantidad = string.Empty;
    }

    [RelayCommand]
    async Task GoToBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
