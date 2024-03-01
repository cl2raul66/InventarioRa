using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventarioRa.Models;
using InventarioRa.Tools.Messages;
using System.ComponentModel;

namespace InventarioRa.ViewModels;

[QueryProperty(nameof(Articles), "articles")]
[QueryProperty(nameof(Clients), "clients")]
public partial class PgBuscarDespachosViewModel : ObservableObject
{
    [ObservableProperty]
    ArticleInventory[]? articles;

    [ObservableProperty]
    ArticleInventory? selectedArticle;
    
    [ObservableProperty]
    Client[]? clients;

    [ObservableProperty]
    Client? selectedClient;

    [ObservableProperty]
    bool isByDate = true;

    [ObservableProperty]
    DateTime startDate = DateTime.Now;

    [ObservableProperty]
    DateTime endDate = DateTime.Now;

    [ObservableProperty]
    bool isByClient;

    [ObservableProperty]
    bool isByArticle;

    [ObservableProperty]
    bool visibleErrorinfo;

    [ObservableProperty]
    string? hasErrorinfo;

    [RelayCommand]
    async Task Buscar()
    {
        if (IsByDate)
        {
            if (DateTime.Compare(EndDate, StartDate) < 0)
            {
                VisibleErrorinfo = true;
                HasErrorinfo = "La fecha final es anterior a la fecha de inicio";
                await Task.Delay(5000);
                VisibleErrorinfo = false;
                HasErrorinfo = string.Empty;
                return;
            }

            _ = WeakReferenceMessenger.Default.Send(new SendDispatchForSearchChangedMessage(new() { StartDate = StartDate, EndDate = EndDate }));
        }

        if (IsByArticle)
        {
            if (SelectedArticle is null)
            {
                VisibleErrorinfo = true;
                HasErrorinfo = "Debe seleccionar un artículo.";
                await Task.Delay(5000);
                VisibleErrorinfo = false;
                HasErrorinfo = string.Empty;
                return;
            }

            _ = WeakReferenceMessenger.Default.Send(new SendDispatchForSearchChangedMessage(new() { Article = SelectedArticle }));
        }

        if (IsByClient)
        {
            if (SelectedClient is null)
            {
                VisibleErrorinfo = true;
                HasErrorinfo = "Debe seleccionar un cliente.";
                await Task.Delay(5000);
                VisibleErrorinfo = false;
                HasErrorinfo = string.Empty;
                return;
            }

            _ = WeakReferenceMessenger.Default.Send(new SendDispatchForSearchChangedMessage(new() { Client = SelectedClient.Id }));
        }

        await GoToBack();
    }

    [RelayCommand]
    async Task GoToBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsByDate))
        {
            if (IsByDate)
            {
                IsByClient = false;
                IsByArticle = false;
            }
        }
        if (e.PropertyName == nameof(IsByClient))
        {
            if (IsByClient)
            {
                IsByDate = false;
                IsByArticle = false;
            }
        }
        if (e.PropertyName == nameof(IsByArticle))
        {
            if (IsByArticle)
            {
                IsByClient = false;
                IsByDate = false;
            }
        }
    }
}
