using Microsoft.Data.Sqlite;

namespace MagazaOtomasyonu.Database;

public static class DatabaseManager
{
    private static readonly Lazy<DatabaseOptions> Options = new(DatabaseOptions.Load);
    private static string DatabasePath => Options.Value.SqliteDatabasePath;
    private static string ConnectionString => $"Data Source={DatabasePath}";

    public static SqliteConnection Open()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys=ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    public static void CreateDatabase() => Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);

    public static void CreateTables()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS Kategoriler(KategoriID INTEGER PRIMARY KEY AUTOINCREMENT, KategoriAdi TEXT NOT NULL UNIQUE);
CREATE TABLE IF NOT EXISTS Urunler(UrunID INTEGER PRIMARY KEY AUTOINCREMENT, UrunAdi TEXT NOT NULL, Kategori TEXT NOT NULL DEFAULT '', Beden TEXT NOT NULL DEFAULT '', Renk TEXT NOT NULL DEFAULT '', AlisFiyati REAL NOT NULL DEFAULT 0, SatisFiyati REAL NOT NULL DEFAULT 0, Stok INTEGER NOT NULL DEFAULT 0, Aktif INTEGER NOT NULL DEFAULT 1, OlusturmaTarihi TEXT NOT NULL DEFAULT (datetime('now','localtime')));
CREATE TABLE IF NOT EXISTS Musteriler(MusteriID INTEGER PRIMARY KEY AUTOINCREMENT, AdSoyad TEXT NOT NULL, Telefon TEXT NOT NULL DEFAULT '', Notlar TEXT NOT NULL DEFAULT '');
CREATE TABLE IF NOT EXISTS Satislar(SatisID INTEGER PRIMARY KEY AUTOINCREMENT, Tarih TEXT NOT NULL DEFAULT (datetime('now','localtime')), MusteriID INTEGER NULL, MusteriAdi TEXT NOT NULL DEFAULT 'Perakende', Toplam REAL NOT NULL, Odenen REAL NOT NULL DEFAULT 0, Notlar TEXT NOT NULL DEFAULT '', FOREIGN KEY(MusteriID) REFERENCES Musteriler(MusteriID));
CREATE TABLE IF NOT EXISTS SatisDetay(SatisDetayID INTEGER PRIMARY KEY AUTOINCREMENT, SatisID INTEGER NOT NULL, UrunID INTEGER NULL, UrunAdi TEXT NOT NULL, Adet INTEGER NOT NULL, BirimFiyat REAL NOT NULL, BirimAlisFiyati REAL NULL, FOREIGN KEY(SatisID) REFERENCES Satislar(SatisID) ON DELETE CASCADE, FOREIGN KEY(UrunID) REFERENCES Urunler(UrunID) ON DELETE SET NULL);
CREATE TABLE IF NOT EXISTS Tahsilatlar(TahsilatID INTEGER PRIMARY KEY AUTOINCREMENT, SatisID INTEGER NOT NULL, Tutar REAL NOT NULL, Tarih TEXT NOT NULL DEFAULT (datetime('now','localtime')), Aciklama TEXT NOT NULL DEFAULT '', FOREIGN KEY(SatisID) REFERENCES Satislar(SatisID) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS IadeDegisim(IslemID INTEGER PRIMARY KEY AUTOINCREMENT, SatisID INTEGER NOT NULL, SatisDetayID INTEGER NOT NULL, UrunID INTEGER NULL, UrunAdi TEXT NOT NULL, Adet INTEGER NOT NULL, IadeTutari REAL NOT NULL, YeniUrunID INTEGER NULL, YeniUrunAdi TEXT NOT NULL DEFAULT '', YeniAdet INTEGER NOT NULL DEFAULT 0, YeniTutar REAL NOT NULL DEFAULT 0, Fark REAL NOT NULL DEFAULT 0, Mutabakat TEXT NOT NULL, Neden TEXT NOT NULL DEFAULT '', Tarih TEXT NOT NULL DEFAULT (datetime('now','localtime')), FOREIGN KEY(SatisID) REFERENCES Satislar(SatisID), FOREIGN KEY(SatisDetayID) REFERENCES SatisDetay(SatisDetayID), FOREIGN KEY(UrunID) REFERENCES Urunler(UrunID) ON DELETE SET NULL, FOREIGN KEY(YeniUrunID) REFERENCES Urunler(UrunID) ON DELETE SET NULL);
CREATE INDEX IF NOT EXISTS IX_IadeDegisim_Satis ON IadeDegisim(SatisID,Tarih);
CREATE INDEX IF NOT EXISTS IX_Satislar_Tarih ON Satislar(Tarih);
CREATE INDEX IF NOT EXISTS IX_Satislar_Musteri ON Satislar(MusteriID);";
        command.ExecuteNonQuery();
        EnsureSaleCostSnapshotColumn(connection);
    }

    private static void EnsureSaleCostSnapshotColumn(SqliteConnection connection)
    {
        using var check = connection.CreateCommand();
        check.CommandText = "PRAGMA table_info(SatisDetay);";
        using var reader = check.ExecuteReader();
        var hasColumn = false;
        while (reader.Read())
            if (string.Equals(reader.GetString(1), "BirimAlisFiyati", StringComparison.OrdinalIgnoreCase))
                hasColumn = true;
        reader.Close();

        if (hasColumn)
            return;

        using var alter = connection.CreateCommand();
        alter.CommandText = "ALTER TABLE SatisDetay ADD COLUMN BirimAlisFiyati REAL NULL;";
        alter.ExecuteNonQuery();
    }
}
