namespace MagazaOtomasyonu.Services;

/// <summary>
/// Composition point for selecting the active persistence provider.
/// The UI depends only on IStoreRepository.
/// </summary>
public static class StoreRepositoryFactory
{
    public static IStoreRepository Create()
    {
        var options = MagazaOtomasyonu.Database.DatabaseOptions.Load();
        return options.Provider.Trim().ToUpperInvariant() switch
        {
            "SQLSERVER" => new SqlServerStoreRepository(options),
            "SQLITE" => new SqliteStoreRepository(),
            _ => throw new InvalidOperationException($"Desteklenmeyen veritabanı sağlayıcısı: {options.Provider}")
        };
    }
}
