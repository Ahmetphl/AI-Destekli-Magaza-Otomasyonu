using MagazaOtomasyonu.Database;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace MagazaOtomasyonu.Services;

public sealed class BackupService
{
    private readonly DatabaseOptions _options = DatabaseOptions.Load();
    private bool IsSqlServer => _options.Provider.Equals("SQLServer", StringComparison.OrdinalIgnoreCase);
    private string DatabaseName => _options.SqlServer.Database;

    public string GetSuggestedBackupDirectory()
    {
        if (!IsSqlServer) return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MagazaYedekleri");
        using var c = new SqlConnection(_options.BuildSqlServerConnectionString()); c.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000))";
        string? value;
        try { value = cmd.ExecuteScalar() as string; }
        catch (SqlException) { value = null; }
        if (!string.IsNullOrWhiteSpace(value)) return value;
        cmd.CommandText = "SELECT TOP(1) physical_name FROM sys.database_files WHERE type_desc=N'ROWS' ORDER BY file_id";
        var dataFile = Convert.ToString(cmd.ExecuteScalar());
        var dataDirectory = string.IsNullOrWhiteSpace(dataFile) ? null : Path.GetDirectoryName(dataFile);
        var instanceDirectory = dataDirectory is null ? null : Directory.GetParent(dataDirectory)?.FullName;
        return instanceDirectory is null ? throw new InvalidOperationException("SQL Server yedekleme klasörü bulunamadı. SQL Server yöneticinizden varsayılan yedekleme klasörünü öğrenin.") : Path.Combine(instanceDirectory, "Backup");
    }

    public void CreateBackup(string destinationPath)
    {
        destinationPath = Path.GetFullPath(destinationPath);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        if (IsSqlServer) CreateSqlServerBackup(destinationPath);
        else CreateSqliteBackup(DatabaseManagerPath(), destinationPath);
    }

    public string Restore(string backupPath)
    {
        backupPath = Path.GetFullPath(backupPath);
        if (!File.Exists(backupPath)) throw new FileNotFoundException("Yedek dosyası bulunamadı.", backupPath);
        if (IsSqlServer) return RestoreSqlServer(backupPath);
        var activePath = DatabaseManagerPath();
        if (string.Equals(activePath, backupPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Açık veritabanı dosyası kendi üzerine geri yüklenemez.");
        VerifySqlite(backupPath);
        var safetyPath = Path.Combine(Path.GetDirectoryName(backupPath)!, $"Magaza_otomatik_geri_yukleme_oncesi_{DateTime.Now:yyyyMMdd_HHmmss_fff}.db");
        CreateSqliteBackup(activePath, safetyPath);
        CreateSqliteBackup(backupPath, activePath);
        VerifySqlite(activePath);
        return safetyPath;
    }

    private void CreateSqlServerBackup(string path)
    {
        using var c = new SqlConnection(_options.BuildSqlServerConnectionString()); c.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "BACKUP DATABASE " + new SqlCommandBuilder().QuoteIdentifier(DatabaseName) + " TO DISK=@path WITH COPY_ONLY, CHECKSUM, INIT; RESTORE VERIFYONLY FROM DISK=@path WITH CHECKSUM;";
        cmd.Parameters.AddWithValue("@path", path); cmd.CommandTimeout = 0; cmd.ExecuteNonQuery();
    }

    private string RestoreSqlServer(string path)
    {
        using (var c = new SqlConnection(_options.BuildSqlServerConnectionString()))
        using (var cmd = c.CreateCommand())
        {
            c.Open(); cmd.CommandText = "RESTORE VERIFYONLY FROM DISK=@path"; cmd.Parameters.AddWithValue("@path", path); cmd.CommandTimeout = 0; cmd.ExecuteNonQuery();
            cmd.Parameters.Clear(); cmd.CommandText = "RESTORE HEADERONLY FROM DISK=@path"; cmd.Parameters.AddWithValue("@path", path);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read() || !string.Equals(Convert.ToString(reader["DatabaseName"]), DatabaseName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Seçilen yedek '{DatabaseName}' veritabanına ait değil.");
        }
        var directory = GetSuggestedBackupDirectory();
        var safetyPath = Path.Combine(directory, $"{DatabaseName}_before_restore_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bak");
        CreateSqlServerBackup(safetyPath);
        var masterBuilder = new SqlConnectionStringBuilder(_options.BuildSqlServerConnectionString()) { InitialCatalog = "master" };
        var database = new SqlCommandBuilder().QuoteIdentifier(DatabaseName);
        using var connection = new SqlConnection(masterBuilder.ConnectionString); connection.Open();
        try
        {
            using var cmd = connection.CreateCommand(); cmd.CommandTimeout = 0;
            cmd.CommandText = $"ALTER DATABASE {database} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE {database} FROM DISK=@path WITH REPLACE, CHECKSUM; ALTER DATABASE {database} SET MULTI_USER;";
            cmd.Parameters.AddWithValue("@path", path); cmd.ExecuteNonQuery();
        }
        catch
        {
            try { using var multi = connection.CreateCommand(); multi.CommandText = $"ALTER DATABASE {database} SET MULTI_USER WITH ROLLBACK IMMEDIATE"; multi.ExecuteNonQuery(); } catch { }
            throw;
        }
        return safetyPath;
    }

    private static void CreateSqliteBackup(string sourcePath, string destinationPath)
    {
        var tempPath = destinationPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            using (var source = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = sourcePath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString()))
            using (var target = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = tempPath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString()))
            {
                source.Open(); target.Open(); source.BackupDatabase(target);
            }
            VerifySqlite(tempPath);
            File.Move(tempPath, destinationPath, true);
            VerifySqlite(destinationPath);
        }
        finally { if (File.Exists(tempPath)) File.Delete(tempPath); }
    }

    private static void VerifySqlite(string path)
    {
        using var c = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly }.ToString()); c.Open();
        using var cmd = c.CreateCommand(); cmd.CommandText = "PRAGMA integrity_check";
        if (!string.Equals(Convert.ToString(cmd.ExecuteScalar()), "ok", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("SQLite veritabanı bütünlük kontrolünü geçemedi.");
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('Urunler','Satislar','SatisDetay')";
        if (Convert.ToInt32(cmd.ExecuteScalar()) != 3) throw new InvalidDataException("Dosya uyumlu bir mağaza veritabanı yedeği değil.");
    }

    private string DatabaseManagerPath() => _options.SqliteDatabasePath;
}
