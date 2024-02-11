using CommunityToolkit.Maui;
using InventarioRa.Servicios;
using InventarioRa.ViewModels;
using InventarioRa.Views;
using Microsoft.Extensions.Logging;

namespace InventarioRa;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("icofont.ttf", "icofont");
            });

        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<IClientesForApiServicio, ClientesForApiServicio>();
        builder.Services.AddSingleton<IClientesServicio, ClientesServicio>();
        builder.Services.AddSingleton<IInventarioServicio, InventarioServicio>();
        builder.Services.AddSingleton<IDespachosServicio, DespachosServicio>();

        builder.Services.AddTransient<PgPrincipal, PgPrincipalViewModel>();
        builder.Services.AddTransient<PgAjustes, PgAjustesViewModel>();
        builder.Services.AddTransient<PgConnection, PgConnectionViewModel>();
        builder.Services.AddTransient<PgInventario, PgInventarioViewModel>();
        builder.Services.AddTransient<PgDespachoUnico, PgDespachoUnicoViewModel>();
        builder.Services.AddTransient<PgDespachoVarios, PgDespachoVariosViewModel>();
        builder.Services.AddTransient<PgAgregarEntrada, PgAgregarEntradaViewModel>();
        builder.Services.AddTransient<PgClientes, PgClientesViewModel>();
        builder.Services.AddTransient<PgBuscarDespachos, PgBuscarDespachosViewModel>();
        builder.Services.AddTransient<PgBuscarInventario, PgBuscarInventarioViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
