namespace InventarioRaServer.Tools;

public class Helpers
{
    static string DIR_DB = Path.Combine(AppContext.BaseDirectory, "Data");

    public static string GetFileDbPath(string namefiledb)
    {        
        if (!Directory.Exists(DIR_DB))
        {
            Directory.CreateDirectory(DIR_DB);
        }

        return Path.Combine(DIR_DB, $"{namefiledb}.db");
    }

}
