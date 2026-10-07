using System.Data.Common;
using System.Text.Json;

namespace MagazaOtomasyonu.Database;

public sealed class DatabaseOptions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public string Provider { get; init; } = "SQLite";
    public string SqliteFileName { get; init; } = "Magaza.db";
    public SqlServerOptions SqlServer { get; init; } = new();

    public string SqliteDatabasePath => Path.Combine(AppContext.BaseDirectory, SqliteFileName);

    public string BuildSqlServerConnectionString()
    {
        if (!string.Equals(SqlServer.Authentication, "Windows", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Bu yapılandırma SQL Server için Windows kimlik doğrulamasını destekler.");

        var builder = new DbConnectionStringBuilder
        {
            ["Data Source"] = SqlServer.Server,
            ["Initial Catalog"] = SqlServer.Database,
            ["Integrated Security"] = true,
            ["Encrypt"] = SqlServer.Encrypt,
            ["TrustServerCertificate"] = SqlServer.TrustServerCertificate
        };

        return builder.ConnectionString;
    }

    public static DatabaseOptions Load()
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(settingsPath))
            return new DatabaseOptions();

        using var stream = File.OpenRead(settingsPath);
        var settings = JsonSerializer.Deserialize<AppSettingsDocument>(stream, JsonOptions);
        var options = settings?.Database ?? new DatabaseOptions();

        if (string.IsNullOrWhiteSpace(options.SqliteFileName) || Path.IsPathRooted(options.SqliteFileName))
            throw new InvalidDataException("Database.SqliteFileName uygulama klasörü içinde bir dosya adı olmalıdır.");
        if (string.IsNullOrWhiteSpace(options.SqlServer.Server) || string.IsNullOrWhiteSpace(options.SqlServer.Database))
            throw new InvalidDataException("SQL Server sunucusu ve veritabanı adı appsettings.json içinde belirtilmelidir.");

        return options;
    }

    private sealed class AppSettingsDocument
    {
        public AppSettingsDocument() { }
        public DatabaseOptions? Database { get; init; }
    }
}

public sealed class SqlServerOptions
{
    public string Server { get; init; } = @".\SQLEXPRESS";
    public string Database { get; init; } = "MagazaOtomasyonu";
    public string Authentication { get; init; } = "Windows";
    public bool Encrypt { get; init; } = true;
    public bool TrustServerCertificate { get; init; } = true;
}
