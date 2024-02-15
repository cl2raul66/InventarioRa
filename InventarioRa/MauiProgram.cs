using CommunityToolkit.Maui;
using InventarioRa.Servicios;
using InventarioRa.ViewModels;
using InventarioRa.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

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

        builder.AddAppsettings();

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

    private static void AddAppsettings(this MauiAppBuilder builder)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.appsettings.json")!;
        if (stream is not null)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            builder.Configuration.AddConfiguration(config);
        }

    }
}
