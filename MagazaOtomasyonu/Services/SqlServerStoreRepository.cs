using System.Data;
using MagazaOtomasyonu.Database;
using MagazaOtomasyonu.Models;
using Microsoft.Data.SqlClient;

namespace MagazaOtomasyonu.Services;

/// <summary>SQL Server implementation. Uses explicit SQL Server types and identity retrieval.</summary>
public sealed class SqlServerStoreRepository : IStoreRepository
{
    private readonly string _connectionString;

    public SqlServerStoreRepository(DatabaseOptions options)
    {
        _connectionString = options.BuildSqlServerConnectionString();
        EnsureReturnExchangeSchema();
    }

    // Upgrade existing SQL Server installations automatically; this is idempotent and
    // only creates the return/exchange ledger required by the current application.
    private void EnsureReturnExchangeSchema()
    {
        using var c = Open(); using var cmd = c.CreateCommand();
        cmd.CommandText = @"
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
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_IadeDegisim_SatisDetay' AND object_id=OBJECT_ID(N'dbo.IadeDegisim'))
    CREATE INDEX IX_IadeDegisim_SatisDetay ON dbo.IadeDegisim(SatisDetayID,Tarih);";
        cmd.ExecuteNonQuery();
    }

    private SqlConnection Open()
    {
        var c = new SqlConnection(_connectionString);
        c.Open();
        return c;
    }

    public DataTable GetCategories() => Read("SELECT KategoriID AS [#],KategoriAdi AS [Kategori] FROM dbo.Kategori WHERE AktifMi=1 ORDER BY KategoriAdi");

    public void AddCategory(string name) => Execute("IF NOT EXISTS(SELECT 1 FROM dbo.Kategori WHERE KategoriAdi=@name) INSERT dbo.Kategori(KategoriAdi,AktifMi) VALUES(@name,1)", P("@name", SqlDbType.NVarChar, name.Trim(), 100));

    public DashboardSummary GetDashboardSummary()
    {
        using var c = Open(); using var cmd = c.CreateCommand();
        cmd.CommandText = @"SELECT
 COALESCE((SELECT SUM(ToplamTutar) FROM dbo.Satis WHERE SatisTarihi>=CONVERT(date,GETDATE()) AND SatisTarihi<DATEADD(day,1,CONVERT(date,GETDATE()))),0),
 COALESCE((SELECT SUM(ToplamTutar) FROM dbo.Satis WHERE SatisTarihi>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1) AND SatisTarihi<DATEADD(month,1,DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1))),0),
 COALESCE((SELECT SUM(StokMiktari) FROM dbo.Urunler WHERE AktifMi=1),0),
 COALESCE((SELECT SUM((d.BirimSatisFiyati-d.BirimAlisFiyati)*d.Adet) FROM dbo.SatisDetay d JOIN dbo.Satis s ON s.SatisID=d.SatisID WHERE d.BirimAlisFiyati IS NOT NULL AND s.SatisTarihi>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1) AND s.SatisTarihi<DATEADD(month,1,DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1))),0),
 COALESCE((SELECT SUM(Tutar) FROM dbo.Tahsilatlar WHERE Tarih>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1) AND Tarih<DATEADD(month,1,DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1))),0),
 COALESCE((SELECT SUM(CASE WHEN ToplamTutar-Odenen>0 THEN ToplamTutar-Odenen ELSE 0 END) FROM dbo.Satis),0);";
        using var r = cmd.ExecuteReader(); r.Read();
        return new DashboardSummary(Convert.ToDecimal(r[0]), Convert.ToDecimal(r[1]), Convert.ToInt32(r[2]), Convert.ToDecimal(r[3]), Convert.ToDecimal(r[4]), Convert.ToDecimal(r[5]));
    }

    public DataTable GetSalesReport() => Read("SELECT SatisID AS [Fiş],SatisTarihi AS [Tarih],MusteriAdi AS [Müşteri],ToplamTutar AS [Satış],Odenen AS [Tahsilat],ToplamTutar-Odenen AS [Borç] FROM dbo.Satis ORDER BY SatisID DESC");
    public DataTable GetBestSellingProducts(int limit) => Read("SELECT TOP (@limit) d.UrunAdi AS [Ürün],SUM(d.Adet) AS [Adet] FROM dbo.SatisDetay d JOIN dbo.Satis s ON s.SatisID=d.SatisID WHERE s.SatisTarihi>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1) AND s.SatisTarihi<DATEADD(month,1,DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1)) GROUP BY d.UrunAdi ORDER BY SUM(d.Adet) DESC", P("@limit", SqlDbType.Int, limit));
    public DataTable GetRecentSales(int limit) => Read("SELECT TOP (@limit) SatisTarihi AS [Tarih],N'Satış No: '+CONVERT(nvarchar(20),SatisID) AS [Satış],ToplamTutar AS [Tutar] FROM dbo.Satis ORDER BY SatisID DESC", P("@limit", SqlDbType.Int, limit));
    public DataTable GetLowestStockProducts(int limit) => Read("SELECT TOP (@limit) UrunAdi AS [Ürün],StokMiktari AS [Stok] FROM dbo.Urunler WHERE AktifMi=1 ORDER BY StokMiktari,UrunAdi", P("@limit", SqlDbType.Int, limit));

    public IReadOnlyList<SaleProductOption> GetAvailableProducts() => Read("SELECT u.UrunID,u.UrunAdi,k.KategoriAdi,u.SatisFiyati,u.StokMiktari FROM dbo.Urunler u JOIN dbo.Kategori k ON k.KategoriID=u.KategoriID WHERE u.AktifMi=1 AND u.StokMiktari>0 ORDER BY u.UrunAdi")
        .Rows.Cast<DataRow>().Select(r => new SaleProductOption(Convert.ToInt32(r[0]), Convert.ToString(r[1]) ?? "", Convert.ToString(r[2]) ?? "", "", "", Convert.ToDecimal(r[3]), Convert.ToInt32(r[4]))).ToList();
    public DataTable GetProducts() => Read("SELECT u.UrunID AS [#],u.UrunAdi AS [Ürün],k.KategoriAdi AS [Kategori],u.AlisFiyati AS [Alış],u.SatisFiyati AS [Satış],u.StokMiktari AS [Stok] FROM dbo.Urunler u JOIN dbo.Kategori k ON k.KategoriID=u.KategoriID WHERE u.AktifMi=1 ORDER BY u.UrunAdi");

    public void SaveProduct(ProductSaveRequest p)
    {
        var categoryId = FindOrCreateCategory(p.Category);
        if (p.ProductId > 0)
            Execute("UPDATE dbo.Urunler SET KategoriID=@cat,UrunAdi=@name,AlisFiyati=@cost,SatisFiyati=@price,StokMiktari=@stock,AktifMi=1 WHERE UrunID=@id", ProductParams(p, categoryId).Append(P("@id", SqlDbType.Int, p.ProductId)).ToArray());
        else
            Execute("INSERT dbo.Urunler(KategoriID,UrunAdi,AlisFiyati,SatisFiyati,StokMiktari,AktifMi) VALUES(@cat,@name,@cost,@price,@stock,1)", ProductParams(p, categoryId));
    }
    public void DeactivateProduct(int productId) => Execute("UPDATE dbo.Urunler SET AktifMi=0 WHERE UrunID=@id", P("@id", SqlDbType.Int, productId));
    public DataTable GetStockProducts() => Read("SELECT u.UrunID AS [#],u.UrunAdi AS [Ürün],k.KategoriAdi AS [Kategori],u.StokMiktari AS [Mevcut stok] FROM dbo.Urunler u JOIN dbo.Kategori k ON k.KategoriID=u.KategoriID WHERE u.AktifMi=1 ORDER BY u.StokMiktari,u.UrunAdi");
    public void UpdateStock(int productId, int quantity) => Execute("UPDATE dbo.Urunler SET StokMiktari=@stock WHERE UrunID=@id", P("@stock", SqlDbType.Int, quantity), P("@id", SqlDbType.Int, productId));

    public long CompleteSale(SaleRequest request)
    {
        if (request.Lines.Count == 0 || request.Lines.Any(x => x.Quantity <= 0 || x.UnitPrice < 0 || x.UnitPrice != decimal.Round(x.UnitPrice, 2))) throw new InvalidOperationException("Satış satırlarını kontrol edin.");
        var total = request.Lines.Sum(x => x.Quantity * x.UnitPrice);
        if (total > 9999999999999999.99m || request.AmountPaid < 0 || request.AmountPaid > total || request.AmountPaid != decimal.Round(request.AmountPaid, 2)) throw new InvalidOperationException("Tahsilat tutarını kontrol edin.");
        using var c = Open(); using var tx = c.BeginTransaction();
        try
        {
            var name = request.CustomerName.Trim(); int? customerId = name.Length == 0 ? null : FindOrCreateCustomer(c, tx, name);
            using var cmd = c.CreateCommand(); cmd.Transaction = tx;
            cmd.CommandText = "INSERT dbo.Satis(SatisTarihi,ToplamTutar,MusteriID,MusteriAdi,Odenen,Notlar) OUTPUT INSERTED.SatisID VALUES(SYSDATETIME(),@total,@customerId,@customerName,@paid,@note)";
            Add(cmd, P("@total", SqlDbType.Decimal, total, precision: 18, scale: 2), P("@customerId", SqlDbType.Int, customerId), P("@customerName", SqlDbType.NVarChar, name.Length == 0 ? "Perakende" : name, 150), P("@paid", SqlDbType.Decimal, request.AmountPaid, precision: 18, scale: 2), P("@note", SqlDbType.NVarChar, request.Note.Trim(), -1));
            var saleId = Convert.ToInt32(cmd.ExecuteScalar());
            foreach (var line in request.Lines)
            {
                using var detail = c.CreateCommand(); detail.Transaction = tx;
                detail.CommandText = "INSERT dbo.SatisDetay(SatisID,UrunID,UrunAdi,Adet,BirimSatisFiyati,BirimAlisFiyati) SELECT @sale,@product,UrunAdi,@qty,@price,AlisFiyati FROM dbo.Urunler WHERE UrunID=@product AND AktifMi=1 AND StokMiktari>=@qty; IF @@ROWCOUNT=0 THROW 50001,'Ürün bulunamadı veya stok yetersiz.',1; UPDATE dbo.Urunler SET StokMiktari=StokMiktari-@qty WHERE UrunID=@product AND AktifMi=1 AND StokMiktari>=@qty; IF @@ROWCOUNT=0 THROW 50002,'Stok satış sırasında değişti.',1;";
                Add(detail, P("@sale", SqlDbType.Int, saleId), P("@product", SqlDbType.Int, line.ProductId), P("@qty", SqlDbType.Int, line.Quantity), P("@price", SqlDbType.Decimal, line.UnitPrice, precision: 18, scale: 2)); detail.ExecuteNonQuery();
            }
            if (request.AmountPaid > 0) { using var pay = c.CreateCommand(); pay.Transaction = tx; pay.CommandText = "INSERT dbo.Tahsilatlar(SatisID,Tutar,Tarih,Aciklama) VALUES(@id,@amount,SYSDATETIME(),N'Satış tahsilatı')"; Add(pay, P("@id", SqlDbType.Int, saleId), P("@amount", SqlDbType.Decimal, request.AmountPaid, precision: 18, scale: 2)); pay.ExecuteNonQuery(); }
            tx.Commit(); return saleId;
        }
        catch { tx.Rollback(); throw; }
    }

    public DataTable GetRecentSalesForEntry(int limit) => Read("SELECT TOP (@limit) SatisID AS [Fiş],SatisTarihi AS [Tarih],MusteriAdi AS [Müşteri],ToplamTutar AS [Toplam],Odenen AS [Ödenen],ToplamTutar-Odenen AS [Kalan] FROM dbo.Satis ORDER BY SatisID DESC", P("@limit", SqlDbType.Int, limit));
    public DataTable GetOpenDebts() => Read("SELECT SatisID AS [Fiş],MusteriAdi AS [Müşteri],SatisTarihi AS [Tarih],ToplamTutar AS [Toplam],Odenen AS [Ödenen],ToplamTutar-Odenen AS [Kalan] FROM dbo.Satis WHERE ToplamTutar>Odenen ORDER BY SatisTarihi");
    public decimal? GetOutstandingBalance(int saleId) { var x = Scalar("SELECT ToplamTutar-Odenen FROM dbo.Satis WHERE SatisID=@id", P("@id", SqlDbType.Int, saleId)); return x is null or DBNull ? null : Convert.ToDecimal(x); }
    public void RecordPayment(int saleId, decimal amount, string description)
    {
        using var c = Open(); using var tx = c.BeginTransaction();
        try
        {
            if (amount <= 0 || amount != decimal.Round(amount, 2)) throw new InvalidOperationException("Tahsilat tutarı sıfırdan büyük ve en fazla iki ondalık basamaklı olmalıdır.");
            using var cmd = c.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = "SELECT ToplamTutar-Odenen FROM dbo.Satis WITH (UPDLOCK,ROWLOCK) WHERE SatisID=@id"; Add(cmd, P("@id", SqlDbType.Int, saleId)); var result = cmd.ExecuteScalar();
            if (result is null or DBNull) throw new InvalidOperationException("Satış kaydı bulunamadı.");
            var debt = Convert.ToDecimal(result); if (amount <= 0 || amount > debt) throw new InvalidOperationException("Tahsilat tutarı kalan borçtan fazla olamaz.");
            using var insert = c.CreateCommand(); insert.Transaction = tx; insert.CommandText = "INSERT dbo.Tahsilatlar(SatisID,Tutar,Tarih,Aciklama) VALUES(@id,@amount,SYSDATETIME(),@description); UPDATE dbo.Satis SET Odenen=Odenen+@amount WHERE SatisID=@id"; Add(insert, P("@id", SqlDbType.Int, saleId), P("@amount", SqlDbType.Decimal, amount, precision: 18, scale: 2), P("@description", SqlDbType.NVarChar, description.Trim(), 300)); insert.ExecuteNonQuery(); tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    public DataTable GetSaleDetails(int saleId) => Read("SELECT d.SatisDetayID AS [Satır],d.UrunAdi AS [Ürün],d.Adet-COALESCE((SELECT SUM(i.Adet) FROM dbo.IadeDegisim i WHERE i.SatisDetayID=d.SatisDetayID),0) AS [İade edilebilir],d.BirimSatisFiyati AS [Birim fiyat],d.UrunID AS [ÜrünID] FROM dbo.SatisDetay d WHERE d.SatisID=@id AND d.Adet>COALESCE((SELECT SUM(i.Adet) FROM dbo.IadeDegisim i WHERE i.SatisDetayID=d.SatisDetayID),0)", P("@id",SqlDbType.Int,saleId));
    public DataTable GetRecentReturns(int limit) => Read("SELECT TOP (@limit) IslemID AS [İşlem],SatisID AS [Fiş],UrunAdi AS [İade edilen],Adet AS [Adet],IadeTutari AS [İade ₺],YeniUrunAdi AS [Verilen ürün],YeniTutar AS [Yeni ürün ₺],Fark AS [Fark ₺],Mutabakat AS [Mutabakat],Neden AS [Neden],Tarih AS [Tarih] FROM dbo.IadeDegisim ORDER BY IslemID DESC",P("@limit",SqlDbType.Int,limit));
    public void CompleteReturnExchange(ReturnExchangeRequest request)
    {
        if(request.Quantity<1 || request.ReplacementQuantity<0 || (request.ReplacementProductId is null)!=(request.ReplacementQuantity==0)) throw new InvalidOperationException("İade/değişim adetlerini kontrol edin.");
        using var c=Open(); using var tx=c.BeginTransaction();
        try
        {
            using var q=c.CreateCommand(); q.Transaction=tx; q.CommandText="SELECT d.UrunID,d.UrunAdi,d.BirimSatisFiyati,d.Adet,s.ToplamTutar,s.Odenen FROM dbo.SatisDetay d WITH (UPDLOCK,HOLDLOCK) JOIN dbo.Satis s WITH (UPDLOCK,HOLDLOCK) ON s.SatisID=d.SatisID WHERE d.SatisDetayID=@detail AND d.SatisID=@sale"; Add(q,P("@detail",SqlDbType.Int,request.SaleDetailId),P("@sale",SqlDbType.Int,request.SaleId)); string oldName; int? oldId; decimal oldUnit,oldTotal,oldPaid; int soldQty; using(var r=q.ExecuteReader()){if(!r.Read())throw new InvalidOperationException("Satış satırı bulunamadı.");oldId=r.IsDBNull(0)?null:r.GetInt32(0);oldName=r.GetString(1);oldUnit=r.GetDecimal(2);soldQty=r.GetInt32(3);oldTotal=r.GetDecimal(4);oldPaid=r.GetDecimal(5);}
            using(var q2=c.CreateCommand()){q2.Transaction=tx;q2.CommandText="SELECT COALESCE(SUM(Adet),0) FROM dbo.IadeDegisim WITH (UPDLOCK,HOLDLOCK) WHERE SatisDetayID=@detail";Add(q2,P("@detail",SqlDbType.Int,request.SaleDetailId));if(Convert.ToInt32(q2.ExecuteScalar())+request.Quantity>soldQty)throw new InvalidOperationException("İade adedi, kalan satılan adetten fazla olamaz.");}
            decimal returned=decimal.Round(oldUnit*request.Quantity,2),newTotal=0,newUnit=0;string newName="";
            if(request.ReplacementProductId is int pid){using var p=c.CreateCommand();p.Transaction=tx;p.CommandText="SELECT UrunAdi,SatisFiyati,StokMiktari FROM dbo.Urunler WITH (UPDLOCK,ROWLOCK) WHERE UrunID=@id AND AktifMi=1";Add(p,P("@id",SqlDbType.Int,pid));using var r=p.ExecuteReader();if(!r.Read())throw new InvalidOperationException("Yeni ürün bulunamadı.");newName=r.GetString(0);newUnit=r.GetDecimal(1);if(r.GetInt32(2)<request.ReplacementQuantity)throw new InvalidOperationException("Yeni ürün için stok yetersiz.");newTotal=decimal.Round(newUnit*request.ReplacementQuantity,2);}
            var diff=decimal.Round(newTotal-returned,2);if(diff>0&&request.Settlement is not("Borç" or "Tahsilat"))throw new InvalidOperationException("Fiyat farkı için Borç veya Tahsilat seçin.");if(diff<0&&request.Settlement is not("Para üstü" or "Müşteri alacağı"))throw new InvalidOperationException("Müşteri lehine fark için Para üstü veya Müşteri alacağı seçin.");
            var revisedTotal=Math.Max(0,oldTotal+diff);var revisedPaid=diff>0&&request.Settlement=="Tahsilat"?oldPaid+diff:diff<0?Math.Min(oldPaid,revisedTotal):oldPaid;
            using(var upd=c.CreateCommand()){upd.Transaction=tx;upd.CommandText="UPDATE dbo.Satis SET ToplamTutar=@total,Odenen=@paid WHERE SatisID=@id";Add(upd,P("@total",SqlDbType.Decimal,revisedTotal,precision:18,scale:2),P("@paid",SqlDbType.Decimal,revisedPaid,precision:18,scale:2),P("@id",SqlDbType.Int,request.SaleId));upd.ExecuteNonQuery();}
            if(oldId is int oldProduct){using var upd=c.CreateCommand();upd.Transaction=tx;upd.CommandText="UPDATE dbo.Urunler SET StokMiktari=StokMiktari+@qty WHERE UrunID=@id";Add(upd,P("@qty",SqlDbType.Int,request.Quantity),P("@id",SqlDbType.Int,oldProduct));upd.ExecuteNonQuery();}
            if(request.ReplacementProductId is int stockId){using var upd=c.CreateCommand();upd.Transaction=tx;upd.CommandText="UPDATE dbo.Urunler SET StokMiktari=StokMiktari-@qty WHERE UrunID=@id AND StokMiktari>=@qty";Add(upd,P("@qty",SqlDbType.Int,request.ReplacementQuantity),P("@id",SqlDbType.Int,stockId));if(upd.ExecuteNonQuery()!=1)throw new InvalidOperationException("Stok işlem sırasında değişti.");}
            using(var ins=c.CreateCommand()){ins.Transaction=tx;ins.CommandText="INSERT dbo.IadeDegisim(SatisID,SatisDetayID,UrunID,UrunAdi,Adet,IadeTutari,YeniUrunID,YeniUrunAdi,YeniAdet,YeniTutar,Fark,Mutabakat,Neden) VALUES(@sale,@detail,@oldId,@oldName,@qty,@returned,@newId,@newName,@newQty,@newTotal,@diff,@settlement,@reason)";Add(ins,P("@sale",SqlDbType.Int,request.SaleId),P("@detail",SqlDbType.Int,request.SaleDetailId),P("@oldId",SqlDbType.Int,oldId),P("@oldName",SqlDbType.NVarChar,oldName,150),P("@qty",SqlDbType.Int,request.Quantity),P("@returned",SqlDbType.Decimal,returned,precision:18,scale:2),P("@newId",SqlDbType.Int,request.ReplacementProductId),P("@newName",SqlDbType.NVarChar,newName,150),P("@newQty",SqlDbType.Int,request.ReplacementQuantity),P("@newTotal",SqlDbType.Decimal,newTotal,precision:18,scale:2),P("@diff",SqlDbType.Decimal,diff,precision:18,scale:2),P("@settlement",SqlDbType.NVarChar,request.Settlement,30),P("@reason",SqlDbType.NVarChar,request.Reason,500));ins.ExecuteNonQuery();}
            if(diff>0&&request.Settlement=="Tahsilat"){using var p=c.CreateCommand();p.Transaction=tx;p.CommandText="INSERT dbo.Tahsilatlar(SatisID,Tutar,Tarih,Aciklama) VALUES(@id,@amount,SYSDATETIME(),N'Değişim fiyat farkı tahsilatı')";Add(p,P("@id",SqlDbType.Int,request.SaleId),P("@amount",SqlDbType.Decimal,diff,precision:18,scale:2));p.ExecuteNonQuery();}tx.Commit();
        }catch{tx.Rollback();throw;}
    }

    private int FindOrCreateCategory(string name)
    {
        var value = name.Trim(); if (value.Length == 0) value = "Kategori Belirtilmedi";
        using var c = Open(); using var cmd = c.CreateCommand(); cmd.CommandText = "IF NOT EXISTS(SELECT 1 FROM dbo.Kategori WHERE KategoriAdi=@name) INSERT dbo.Kategori(KategoriAdi,AktifMi) VALUES(@name,1); SELECT TOP(1) KategoriID FROM dbo.Kategori WHERE KategoriAdi=@name ORDER BY KategoriID"; Add(cmd, P("@name", SqlDbType.NVarChar, value, 100)); return Convert.ToInt32(cmd.ExecuteScalar());
    }
    private static int FindOrCreateCustomer(SqlConnection c, SqlTransaction tx, string name)
    {
        using var cmd = c.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = "DECLARE @id int=(SELECT TOP(1) MusteriID FROM dbo.Musteriler WITH (UPDLOCK,HOLDLOCK) WHERE AdSoyad=@name AND Telefon=N''); IF @id IS NULL BEGIN INSERT dbo.Musteriler(AdSoyad,Telefon,Notlar) VALUES(@name,N'',N''); SET @id=CONVERT(int,SCOPE_IDENTITY()); END SELECT @id"; Add(cmd, P("@name", SqlDbType.NVarChar, name, 150)); return Convert.ToInt32(cmd.ExecuteScalar());
    }
    private static DbP[] ProductParams(ProductSaveRequest p, int categoryId) => [P("@cat", SqlDbType.Int, categoryId), P("@name", SqlDbType.NVarChar, p.Name.Trim(), 150), P("@cost", SqlDbType.Decimal, p.CostPrice, precision: 18, scale: 2), P("@price", SqlDbType.Decimal, p.SalePrice, precision: 18, scale: 2), P("@stock", SqlDbType.Int, p.Stock)];
    private object? Scalar(string sql, params DbP[] parameters) { using var c = Open(); using var cmd = c.CreateCommand(); cmd.CommandText = sql; Add(cmd, parameters); return cmd.ExecuteScalar(); }
    private DataTable Read(string sql, params DbP[] parameters) { using var c = Open(); using var cmd = c.CreateCommand(); cmd.CommandText = sql; Add(cmd, parameters); using var reader = cmd.ExecuteReader(); var t = new DataTable(); t.Load(reader); return t; }
    private void Execute(string sql, params DbP[] parameters) { using var c = Open(); using var cmd = c.CreateCommand(); cmd.CommandText = sql; Add(cmd, parameters); cmd.ExecuteNonQuery(); }
    private static DbP P(string name, SqlDbType type, object? value, int size = 0, byte precision = 0, byte scale = 0) => new(name, type, value ?? DBNull.Value, size, precision, scale);
    private static void Add(SqlCommand cmd, params DbP[] parameters) { foreach (var p in parameters) { var x = cmd.Parameters.Add(p.Name, p.Type); x.Value = p.Value; if (p.Size != 0) x.Size = p.Size; if (p.Type == SqlDbType.Decimal) { x.Precision = p.Precision; x.Scale = p.Scale; } } }
    private sealed record DbP(string Name, SqlDbType Type, object Value, int Size, byte Precision, byte Scale);
}
