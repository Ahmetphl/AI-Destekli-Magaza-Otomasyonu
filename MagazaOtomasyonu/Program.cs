using MagazaOtomasyonu.Database;
using MagazaOtomasyonu.Services;
namespace MagazaOtomasyonu
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            var databaseOptions = DatabaseOptions.Load();
            if (string.Equals(databaseOptions.Provider, "SQLite", StringComparison.OrdinalIgnoreCase))
            {
                DatabaseManager.CreateDatabase();
                DatabaseManager.CreateTables();
            }
            else if (!string.Equals(databaseOptions.Provider, "SQLServer", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Desteklenmeyen veritabanı sağlayıcısı: {databaseOptions.Provider}");
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new FrmAna(StoreRepositoryFactory.Create()));
        }
    }
}
