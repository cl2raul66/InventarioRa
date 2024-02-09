using InventarioRa.Views;

namespace InventarioRa;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(PgAjustes), typeof(PgAjustes));
        Routing.RegisterRoute(nameof(PgConnection), typeof(PgConnection));
        Routing.RegisterRoute(nameof(PgInventario), typeof(PgInventario));
        Routing.RegisterRoute(nameof(PgDespachoUnico), typeof(PgDespachoUnico));
        Routing.RegisterRoute(nameof(PgDespachoVarios), typeof(PgDespachoVarios));
        Routing.RegisterRoute(nameof(PgAgregarEntrada), typeof(PgAgregarEntrada));
        Routing.RegisterRoute(nameof(PgClientes), typeof(PgClientes));
        Routing.RegisterRoute(nameof(PgBuscarInventario), typeof(PgBuscarInventario));
        Routing.RegisterRoute(nameof(PgBuscarDespachos), typeof(PgBuscarDespachos));
    }
}
