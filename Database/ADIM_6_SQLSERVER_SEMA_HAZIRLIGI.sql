/*
  ADIM 6 - hedef SQL Server uygulama semasi
  Bu dosya hazirlik/migrasyon betigidir. Kendiliginden calistirilmaz.
  Once yedek/kopya uzerinde uygulanip kontrol edilmeli (Adim 9).
  Beden ve Renk alanlari bilerek eklenmemistir.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Musteriler', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Musteriler
    (
        MusteriID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Musteriler PRIMARY KEY,
        AdSoyad nvarchar(150) NOT NULL,
        Telefon nvarchar(30) NOT NULL CONSTRAINT DF_Musteriler_Telefon DEFAULT N'',
        Notlar nvarchar(max) NOT NULL CONSTRAINT DF_Musteriler_Notlar DEFAULT N''
    );
END;

IF COL_LENGTH(N'dbo.Urunler', N'AktifMi') IS NULL
    ALTER TABLE dbo.Urunler ADD AktifMi bit NOT NULL CONSTRAINT DF_Urunler_AktifMi DEFAULT (1) WITH VALUES;
IF COL_LENGTH(N'dbo.Urunler', N'OlusturmaTarihi') IS NULL
    ALTER TABLE dbo.Urunler ADD OlusturmaTarihi datetime2(0) NOT NULL CONSTRAINT DF_Urunler_OlusturmaTarihi DEFAULT (SYSDATETIME()) WITH VALUES;

IF COL_LENGTH(N'dbo.Satis', N'MusteriID') IS NULL ALTER TABLE dbo.Satis ADD MusteriID int NULL;
IF COL_LENGTH(N'dbo.Satis', N'MusteriAdi') IS NULL ALTER TABLE dbo.Satis ADD MusteriAdi nvarchar(150) NOT NULL CONSTRAINT DF_Satis_MusteriAdi DEFAULT (N'Perakende') WITH VALUES;
IF COL_LENGTH(N'dbo.Satis', N'Odenen') IS NULL ALTER TABLE dbo.Satis ADD Odenen decimal(18,2) NOT NULL CONSTRAINT DF_Satis_Odenen DEFAULT (0) WITH VALUES;
IF COL_LENGTH(N'dbo.Satis', N'Notlar') IS NULL ALTER TABLE dbo.Satis ADD Notlar nvarchar(max) NOT NULL CONSTRAINT DF_Satis_Notlar DEFAULT (N'') WITH VALUES;

IF COL_LENGTH(N'dbo.SatisDetay', N'UrunAdi') IS NULL ALTER TABLE dbo.SatisDetay ADD UrunAdi nvarchar(150) NOT NULL CONSTRAINT DF_SatisDetay_UrunAdi DEFAULT (N'') WITH VALUES;
IF COL_LENGTH(N'dbo.SatisDetay', N'BirimAlisFiyati') IS NOT NULL
    ALTER TABLE dbo.SatisDetay ALTER COLUMN BirimAlisFiyati decimal(18,2) NULL;
IF COL_LENGTH(N'dbo.SatisDetay', N'BirimSatisFiyati') IS NOT NULL
    ALTER TABLE dbo.SatisDetay ALTER COLUMN BirimSatisFiyati decimal(18,2) NOT NULL;
IF COL_LENGTH(N'dbo.SatisDetay', N'UrunID') IS NOT NULL
    ALTER TABLE dbo.SatisDetay ALTER COLUMN UrunID int NULL;

IF OBJECT_ID(N'dbo.Tahsilatlar', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tahsilatlar
    (
        TahsilatID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tahsilatlar PRIMARY KEY,
        SatisID int NOT NULL,
        Tutar decimal(18,2) NOT NULL,
        Tarih datetime2(0) NOT NULL CONSTRAINT DF_Tahsilatlar_Tarih DEFAULT (SYSDATETIME()),
        Aciklama nvarchar(300) NOT NULL CONSTRAINT DF_Tahsilatlar_Aciklama DEFAULT (N''),
        CONSTRAINT FK_Tahsilatlar_Satis FOREIGN KEY (SatisID) REFERENCES dbo.Satis(SatisID)
    );
END;

IF OBJECT_ID(N'dbo.IadeDegisim', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IadeDegisim
    (
        IslemID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_IadeDegisim PRIMARY KEY,
        SatisID int NOT NULL, SatisDetayID int NOT NULL, UrunID int NULL,
        UrunAdi nvarchar(150) NOT NULL, Adet int NOT NULL, IadeTutari decimal(18,2) NOT NULL,
        YeniUrunID int NULL, YeniUrunAdi nvarchar(150) NOT NULL CONSTRAINT DF_IadeDegisim_YeniUrun DEFAULT N'',
        YeniAdet int NOT NULL CONSTRAINT DF_IadeDegisim_YeniAdet DEFAULT 0,
        YeniTutar decimal(18,2) NOT NULL CONSTRAINT DF_IadeDegisim_YeniTutar DEFAULT 0,
        Fark decimal(18,2) NOT NULL CONSTRAINT DF_IadeDegisim_Fark DEFAULT 0,
        Mutabakat nvarchar(30) NOT NULL, Neden nvarchar(500) NOT NULL CONSTRAINT DF_IadeDegisim_Neden DEFAULT N'',
        Tarih datetime2(0) NOT NULL CONSTRAINT DF_IadeDegisim_Tarih DEFAULT SYSDATETIME(),
        CONSTRAINT FK_IadeDegisim_Satis FOREIGN KEY(SatisID) REFERENCES dbo.Satis(SatisID),
        CONSTRAINT FK_IadeDegisim_SatisDetay FOREIGN KEY(SatisDetayID) REFERENCES dbo.SatisDetay(SatisDetayID),
        CONSTRAINT FK_IadeDegisim_Urun FOREIGN KEY(UrunID) REFERENCES dbo.Urunler(UrunID),
        CONSTRAINT FK_IadeDegisim_YeniUrun FOREIGN KEY(YeniUrunID) REFERENCES dbo.Urunler(UrunID),
        CONSTRAINT CK_IadeDegisim_Adet CHECK(Adet > 0 AND YeniAdet >= 0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_Satis_Musteriler')
    ALTER TABLE dbo.Satis ADD CONSTRAINT FK_Satis_Musteriler FOREIGN KEY (MusteriID) REFERENCES dbo.Musteriler(MusteriID);

IF COL_LENGTH(N'dbo.Satis', N'ToplamTutar') IS NOT NULL
    ALTER TABLE dbo.Satis ALTER COLUMN ToplamTutar decimal(18,2) NOT NULL;
IF COL_LENGTH(N'dbo.Urunler', N'AlisFiyati') IS NOT NULL
    ALTER TABLE dbo.Urunler ALTER COLUMN AlisFiyati decimal(18,2) NOT NULL;
IF COL_LENGTH(N'dbo.Urunler', N'SatisFiyati') IS NOT NULL
    ALTER TABLE dbo.Urunler ALTER COLUMN SatisFiyati decimal(18,2) NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_Tahsilatlar_SatisID' AND object_id=OBJECT_ID(N'dbo.Tahsilatlar'))
    CREATE INDEX IX_Tahsilatlar_SatisID ON dbo.Tahsilatlar(SatisID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_Satis_SatisTarihi' AND object_id=OBJECT_ID(N'dbo.Satis'))
    CREATE INDEX IX_Satis_SatisTarihi ON dbo.Satis(SatisTarihi);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_IadeDegisim_SatisDetay' AND object_id=OBJECT_ID(N'dbo.IadeDegisim'))
    CREATE INDEX IX_IadeDegisim_SatisDetay ON dbo.IadeDegisim(SatisDetayID,Tarih);

COMMIT TRANSACTION;
