using System.Data;
using MagazaOtomasyonu.Database;
using MagazaOtomasyonu.Models;
using Microsoft.Data.Sqlite;

namespace MagazaOtomasyonu.Services;

/// <summary>
/// SQLite persistence implementation. UI code depends on IStoreRepository
/// and does not create provider connections or commands directly.
/// </summary>
public sealed class SqliteStoreRepository : IStoreRepository
{
    public DataTable GetCategories() => ReadTable(
        "SELECT KategoriID AS [#],KategoriAdi AS [Kategori] FROM Kategoriler ORDER BY KategoriAdi");

    public void AddCategory(string name) => Execute(
        "INSERT OR IGNORE INTO Kategoriler(KategoriAdi) VALUES($name)", ("$name", name.Trim()));

    public DashboardSummary GetDashboardSummary()
    {
        using var connection = DatabaseManager.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    (SELECT COALESCE(SUM(Toplam),0) FROM Satislar WHERE date(Tarih)=date('now','localtime')),
    (SELECT COALESCE(SUM(Toplam),0) FROM Satislar WHERE strftime('%Y-%m',Tarih)=strftime('%Y-%m','now','localtime')),
    (SELECT COALESCE(SUM(Stok),0) FROM Urunler WHERE Aktif=1),
    (SELECT COALESCE(SUM((d.BirimFiyat-d.BirimAlisFiyati)*d.Adet),0)
       FROM SatisDetay d
       JOIN Satislar s ON s.SatisID=d.SatisID
      WHERE d.BirimAlisFiyati IS NOT NULL
        AND strftime('%Y-%m',s.Tarih)=strftime('%Y-%m','now','localtime')),
    (SELECT COALESCE(SUM(Tutar),0) FROM Tahsilatlar WHERE strftime('%Y-%m',Tarih)=strftime('%Y-%m','now','localtime')),
    (SELECT COALESCE(SUM(Toplam-Odenen),0) FROM Satislar);";

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return new DashboardSummary(0, 0, 0, 0, 0, 0);

        return new DashboardSummary(
            Convert.ToDecimal(reader.GetValue(0)),
            Convert.ToDecimal(reader.GetValue(1)),
            Convert.ToInt32(reader.GetValue(2)),
            Convert.ToDecimal(reader.GetValue(3)),
            Convert.ToDecimal(reader.GetValue(4)),
            Convert.ToDecimal(reader.GetValue(5)));
    }

    public DataTable GetSalesReport() => ReadTable(
        "SELECT SatisID AS [Fiş],Tarih AS [Tarih],MusteriAdi AS [Müşteri],Toplam AS [Satış],Odenen AS [Tahsilat],Toplam-Odenen AS [Borç] FROM Satislar ORDER BY SatisID DESC");

    public DataTable GetBestSellingProducts(int limit) => ReadTable(
        "SELECT d.UrunAdi AS [Ürün], SUM(d.Adet) AS [Adet] FROM SatisDetay d JOIN Satislar s ON s.SatisID=d.SatisID WHERE strftime('%Y-%m',s.Tarih)=strftime('%Y-%m','now','localtime') GROUP BY d.UrunAdi ORDER BY SUM(d.Adet) DESC LIMIT $limit",
        ("$limit", limit));

    public DataTable GetRecentSales(int limit) => ReadTable(
        "SELECT Tarih AS [Tarih], 'Satış No: '||SatisID AS [Satış], Toplam AS [Tutar] FROM Satislar ORDER BY SatisID DESC LIMIT $limit",
        ("$limit", limit));

    public DataTable GetLowestStockProducts(int limit) => ReadTable(
        "SELECT UrunAdi AS [Ürün], Stok AS [Stok] FROM Urunler WHERE Aktif=1 ORDER BY Stok ASC LIMIT $limit",
        ("$limit", limit));

    public IReadOnlyList<SaleProductOption> GetAvailableProducts()
    {
        var products = new List<SaleProductOption>();
        var table = ReadTable("SELECT UrunID,UrunAdi,Kategori,Beden,Renk,SatisFiyati,Stok FROM Urunler WHERE Aktif=1 AND Stok>0 ORDER BY UrunAdi");
        foreach (DataRow row in table.Rows)
        {
            products.Add(new SaleProductOption(
                Convert.ToInt32(row["UrunID"]),
                Convert.ToString(row["UrunAdi"]) ?? "",
                Convert.ToString(row["Kategori"]) ?? "",
                Convert.ToString(row["Beden"]) ?? "",
                Convert.ToString(row["Renk"]) ?? "",
                Convert.ToDecimal(row["SatisFiyati"]),
                Convert.ToInt32(row["Stok"])));
        }

        return products;
    }

    public DataTable GetProducts() => ReadTable(
        "SELECT UrunID AS [#], UrunAdi AS [Ürün], Kategori AS [Kategori], AlisFiyati AS [Alış], SatisFiyati AS [Satış], Stok AS [Stok] FROM Urunler WHERE Aktif=1 ORDER BY UrunAdi");

    public void SaveProduct(ProductSaveRequest product)
    {
        if (product.ProductId > 0)
        {
            Execute(
                "UPDATE Urunler SET UrunAdi=$name,Kategori=$category,Beden=$size,Renk=$color,AlisFiyati=$costPrice,SatisFiyati=$salePrice,Stok=$stock,Aktif=1 WHERE UrunID=$id",
                ("$name", product.Name.Trim()), ("$category", product.Category.Trim()),
                ("$size", product.Size.Trim()), ("$color", product.Color.Trim()),
                ("$costPrice", product.CostPrice), ("$salePrice", product.SalePrice),
                ("$stock", product.Stock), ("$id", product.ProductId));
            return;
        }

        Execute(
            "INSERT INTO Urunler(UrunAdi,Kategori,Beden,Renk,AlisFiyati,SatisFiyati,Stok) VALUES($name,$category,$size,$color,$costPrice,$salePrice,$stock)",
            ("$name", product.Name.Trim()), ("$category", product.Category.Trim()),
            ("$size", product.Size.Trim()), ("$color", product.Color.Trim()),
            ("$costPrice", product.CostPrice), ("$salePrice", product.SalePrice),
            ("$stock", product.Stock));
    }

    public void DeactivateProduct(int productId) => Execute(
        "UPDATE Urunler SET Aktif=0 WHERE UrunID=$id", ("$id", productId));

    public DataTable GetStockProducts() => ReadTable(
        "SELECT UrunID AS [#],UrunAdi AS [Ürün],Kategori AS [Kategori],Stok AS [Mevcut stok] FROM Urunler WHERE Aktif=1 ORDER BY Stok,UrunAdi");

    public void UpdateStock(int productId, int quantity) => Execute(
        "UPDATE Urunler SET Stok=$quantity WHERE UrunID=$id",
        ("$quantity", quantity), ("$id", productId));

    public long CompleteSale(SaleRequest request)
    {
        if (request.Lines.Count == 0)
            throw new InvalidOperationException("Satışa en az bir ürün ekleyin.");

        if (request.Lines.Any(line => line.Quantity <= 0 || line.UnitPrice < 0 || line.UnitPrice != decimal.Round(line.UnitPrice, 2)))
            throw new InvalidOperationException("Ürün adedi veya fiyatı geçersiz.");

        var total = request.Lines.Sum(line => line.Quantity * line.UnitPrice);
        if (total > 9999999999999999.99m || request.AmountPaid < 0 || request.AmountPaid > total || request.AmountPaid != decimal.Round(request.AmountPaid, 2))
            throw new InvalidOperationException("Tahsilat tutarını kontrol edin.");

        using var connection = DatabaseManager.Open();
        using var transaction = connection.BeginTransaction();
        var customerName = request.CustomerName.Trim();
        var customerId = FindOrCreateCustomer(connection, transaction, customerName);

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO Satislar(MusteriID,MusteriAdi,Toplam,Odenen,Notlar) VALUES($customerId,$customerName,$total,$paid,$note)";
            command.Parameters.AddWithValue("$customerId", customerId is null ? DBNull.Value : customerId.Value);
            command.Parameters.AddWithValue("$customerName", customerName.Length == 0 ? "Perakende" : customerName);
            command.Parameters.AddWithValue("$total", total);
            command.Parameters.AddWithValue("$paid", request.AmountPaid);
            command.Parameters.AddWithValue("$note", request.Note.Trim());
            command.ExecuteNonQuery();
        }

        long saleId;
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "SELECT last_insert_rowid();";
            saleId = Convert.ToInt64(command.ExecuteScalar());
        }

        foreach (var line in request.Lines)
        {
            using (var stockCommand = connection.CreateCommand())
            {
                stockCommand.Transaction = transaction;
                stockCommand.CommandText = "SELECT Stok,AlisFiyati,UrunAdi FROM Urunler WHERE UrunID=$id AND Aktif=1";
                stockCommand.Parameters.AddWithValue("$id", line.ProductId);
                int availableStock;
                decimal saleTimeCost;
                string saleTimeProductName;
                using (var stockReader = stockCommand.ExecuteReader())
                {
                    if (!stockReader.Read())
                        throw new InvalidOperationException("Satış sırasında ürün bulunamadı veya pasif hale geldi.");
                    availableStock = stockReader.GetInt32(0);
                    saleTimeCost = Convert.ToDecimal(stockReader.GetValue(1));
                    saleTimeProductName = stockReader.GetString(2);
                }
                if (availableStock < line.Quantity)
                    throw new InvalidOperationException("Satış sırasında stok değişti; satış iptal edildi.");

                using var detailCommand = connection.CreateCommand();
                detailCommand.Transaction = transaction;
                detailCommand.CommandText = "INSERT INTO SatisDetay(SatisID,UrunID,UrunAdi,Adet,BirimFiyat,BirimAlisFiyati) VALUES($saleId,$productId,$productName,$quantity,$price,$costPrice)";
                detailCommand.Parameters.AddWithValue("$saleId", saleId);
                detailCommand.Parameters.AddWithValue("$productId", line.ProductId);
                detailCommand.Parameters.AddWithValue("$productName", saleTimeProductName);
                detailCommand.Parameters.AddWithValue("$quantity", line.Quantity);
                detailCommand.Parameters.AddWithValue("$price", line.UnitPrice);
                detailCommand.Parameters.AddWithValue("$costPrice", saleTimeCost);
                detailCommand.ExecuteNonQuery();
            }

            using (var updateStock = connection.CreateCommand())
            {
                updateStock.Transaction = transaction;
                updateStock.CommandText = "UPDATE Urunler SET Stok=Stok-$quantity WHERE UrunID=$productId AND Aktif=1 AND Stok >= $quantity";
                updateStock.Parameters.AddWithValue("$quantity", line.Quantity);
                updateStock.Parameters.AddWithValue("$productId", line.ProductId);
                if (updateStock.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("Satış sırasında stok değişti; satış iptal edildi.");
            }
        }

        if (request.AmountPaid > 0)
        {
            using var paymentCommand = connection.CreateCommand();
            paymentCommand.Transaction = transaction;
            paymentCommand.CommandText = "INSERT INTO Tahsilatlar(SatisID,Tutar,Aciklama) VALUES($saleId,$amount,'Satış tahsilatı')";
            paymentCommand.Parameters.AddWithValue("$saleId", saleId);
            paymentCommand.Parameters.AddWithValue("$amount", request.AmountPaid);
            paymentCommand.ExecuteNonQuery();
        }

        transaction.Commit();
        return saleId;
    }

    public DataTable GetRecentSalesForEntry(int limit) => ReadTable(
        "SELECT SatisID AS [Fiş],Tarih AS [Tarih],MusteriAdi AS [Müşteri],Toplam AS [Toplam],Odenen AS [Ödenen],Toplam-Odenen AS [Kalan] FROM Satislar ORDER BY SatisID DESC LIMIT $limit",
        ("$limit", limit));

    public DataTable GetOpenDebts() => ReadTable(
        "SELECT SatisID AS [Fiş],MusteriAdi AS [Müşteri],Tarih AS [Tarih],Toplam AS [Toplam],Odenen AS [Ödenen],Toplam-Odenen AS [Kalan] FROM Satislar WHERE Toplam>Odenen ORDER BY Tarih");

    public decimal? GetOutstandingBalance(int saleId)
    {
        using var connection = DatabaseManager.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Toplam-Odenen FROM Satislar WHERE SatisID=$id";
        command.Parameters.AddWithValue("$id", saleId);
        var result = command.ExecuteScalar();
        return result is null || result is DBNull ? null : Convert.ToDecimal(result);
    }

    public void RecordPayment(int saleId, decimal amount, string description)
    {
        using var connection = DatabaseManager.Open();
        using var transaction = connection.BeginTransaction();
        if (amount <= 0 || amount != decimal.Round(amount, 2))
            throw new InvalidOperationException("Tahsilat tutarı sıfırdan büyük ve en fazla iki ondalık basamaklı olmalıdır.");

        using (var updateCommand = connection.CreateCommand())
        {
            updateCommand.Transaction = transaction;
            updateCommand.CommandText = "UPDATE Satislar SET Odenen=Odenen+$amount WHERE SatisID=$id AND Toplam-Odenen >= $amount AND $amount > 0";
            updateCommand.Parameters.AddWithValue("$amount", amount);
            updateCommand.Parameters.AddWithValue("$id", saleId);
            if (updateCommand.ExecuteNonQuery() != 1)
                throw new InvalidOperationException("Satış kaydı bulunamadı veya tahsilat kalan borçtan fazla.");
        }

        using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = "INSERT INTO Tahsilatlar(SatisID,Tutar,Aciklama) VALUES($id,$amount,$description)";
            insertCommand.Parameters.AddWithValue("$id", saleId);
            insertCommand.Parameters.AddWithValue("$amount", amount);
            insertCommand.Parameters.AddWithValue("$description", description.Trim());
            insertCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public DataTable GetSaleDetails(int saleId) => ReadTable(@"SELECT d.SatisDetayID AS [Satır],d.UrunAdi AS [Ürün],d.Adet-COALESCE((SELECT SUM(i.Adet) FROM IadeDegisim i WHERE i.SatisDetayID=d.SatisDetayID),0) AS [İade edilebilir],d.BirimFiyat AS [Birim fiyat],d.UrunID AS [ÜrünID] FROM SatisDetay d WHERE d.SatisID=$id AND d.Adet>COALESCE((SELECT SUM(i.Adet) FROM IadeDegisim i WHERE i.SatisDetayID=d.SatisDetayID),0)", ("$id", saleId));

    public DataTable GetRecentReturns(int limit) => ReadTable("SELECT IslemID AS [İşlem],SatisID AS [Fiş],UrunAdi AS [İade edilen],Adet AS [Adet],IadeTutari AS [İade ₺],YeniUrunAdi AS [Verilen ürün],YeniTutar AS [Yeni ürün ₺],Fark AS [Fark ₺],Mutabakat AS [Mutabakat],Neden AS [Neden],Tarih AS [Tarih] FROM IadeDegisim ORDER BY IslemID DESC LIMIT $limit", ("$limit", limit));

    public void CompleteReturnExchange(ReturnExchangeRequest request)
    {
        if (request.Quantity < 1 || request.ReplacementQuantity < 0 || (request.ReplacementProductId is null) != (request.ReplacementQuantity == 0)) throw new InvalidOperationException("İade/değişim adetlerini kontrol edin.");
        using var c = DatabaseManager.Open(); using var tx = c.BeginTransaction();
        try
        {
            long? returnedProduct = null; string returnedName = ""; decimal returnUnit = 0; int soldQty = 0;
            using (var q = c.CreateCommand()) { q.Transaction = tx; q.CommandText = "SELECT UrunID,UrunAdi,BirimFiyat,Adet FROM SatisDetay WHERE SatisDetayID=$detail AND SatisID=$sale"; q.Parameters.AddWithValue("$detail", request.SaleDetailId); q.Parameters.AddWithValue("$sale", request.SaleId); using var r = q.ExecuteReader(); if (!r.Read()) throw new InvalidOperationException("Satış satırı bulunamadı."); returnedProduct = r.IsDBNull(0) ? null : r.GetInt64(0); returnedName = r.GetString(1); returnUnit = r.GetDecimal(2); soldQty = r.GetInt32(3); }
            using (var q = c.CreateCommand()) { q.Transaction = tx; q.CommandText = "SELECT COALESCE(SUM(Adet),0) FROM IadeDegisim WHERE SatisDetayID=$detail"; q.Parameters.AddWithValue("$detail", request.SaleDetailId); if (Convert.ToInt32(q.ExecuteScalar()) + request.Quantity > soldQty) throw new InvalidOperationException("İade adedi, kalan satılan adetten fazla olamaz."); }
            var returnTotal = decimal.Round(returnUnit * request.Quantity, 2); decimal newTotal = 0; string newName = ""; decimal newUnit = 0;
            if (request.ReplacementProductId is int newId) { using var q = c.CreateCommand(); q.Transaction=tx; q.CommandText="SELECT UrunAdi,SatisFiyati,Stok FROM Urunler WHERE UrunID=$id AND Aktif=1"; q.Parameters.AddWithValue("$id",newId); using var r=q.ExecuteReader(); if(!r.Read()) throw new InvalidOperationException("Yeni ürün bulunamadı."); newName=r.GetString(0); newUnit=r.GetDecimal(1); if(r.GetInt32(2)<request.ReplacementQuantity) throw new InvalidOperationException("Yeni ürün için stok yetersiz."); newTotal=decimal.Round(newUnit*request.ReplacementQuantity,2); }
            var diff = decimal.Round(newTotal-returnTotal,2);
            if (diff > 0 && request.Settlement is not ("Borç" or "Tahsilat")) throw new InvalidOperationException("Fiyat farkı için Borç veya Tahsilat seçin.");
            if (diff < 0 && request.Settlement is not ("Para üstü" or "Müşteri alacağı")) throw new InvalidOperationException("Müşteri lehine fark için Para üstü veya Müşteri alacağı seçin.");
            decimal oldTotal, oldPaid; using(var q=c.CreateCommand()){q.Transaction=tx;q.CommandText="SELECT Toplam,Odenen FROM Satislar WHERE SatisID=$id";q.Parameters.AddWithValue("$id",request.SaleId);using var r=q.ExecuteReader();if(!r.Read())throw new InvalidOperationException("Satış bulunamadı.");oldTotal=r.GetDecimal(0);oldPaid=r.GetDecimal(1);}
            var revisedTotal=Math.Max(0,oldTotal+diff); var revisedPaid=oldPaid;
            if(diff>0 && request.Settlement=="Tahsilat") revisedPaid+=diff;
            if(diff<0) revisedPaid=Math.Min(oldPaid,revisedTotal);
            using(var u=c.CreateCommand()){u.Transaction=tx;u.CommandText="UPDATE Satislar SET Toplam=$total,Odenen=$paid WHERE SatisID=$id";u.Parameters.AddWithValue("$total",revisedTotal);u.Parameters.AddWithValue("$paid",revisedPaid);u.Parameters.AddWithValue("$id",request.SaleId);u.ExecuteNonQuery();}
            if(returnedProduct is long oldProduct){using var u=c.CreateCommand();u.Transaction=tx;u.CommandText="UPDATE Urunler SET Stok=Stok+$qty WHERE UrunID=$id";u.Parameters.AddWithValue("$qty",request.Quantity);u.Parameters.AddWithValue("$id",oldProduct);u.ExecuteNonQuery();}
            if(request.ReplacementProductId is int stockId){using var u=c.CreateCommand();u.Transaction=tx;u.CommandText="UPDATE Urunler SET Stok=Stok-$qty WHERE UrunID=$id AND Stok >= $qty";u.Parameters.AddWithValue("$qty",request.ReplacementQuantity);u.Parameters.AddWithValue("$id",stockId);if(u.ExecuteNonQuery()!=1)throw new InvalidOperationException("Stok işlem sırasında değişti; işlem iptal edildi.");}
            using(var ins=c.CreateCommand()){ins.Transaction=tx;ins.CommandText="INSERT INTO IadeDegisim(SatisID,SatisDetayID,UrunID,UrunAdi,Adet,IadeTutari,YeniUrunID,YeniUrunAdi,YeniAdet,YeniTutar,Fark,Mutabakat,Neden) VALUES($sale,$detail,$oldId,$oldName,$qty,$return,$newId,$newName,$newQty,$newTotal,$diff,$settlement,$reason)";ins.Parameters.AddWithValue("$sale",request.SaleId);ins.Parameters.AddWithValue("$detail",request.SaleDetailId);ins.Parameters.AddWithValue("$oldId",(object?)returnedProduct??DBNull.Value);ins.Parameters.AddWithValue("$oldName",returnedName);ins.Parameters.AddWithValue("$qty",request.Quantity);ins.Parameters.AddWithValue("$return",returnTotal);ins.Parameters.AddWithValue("$newId",(object?)request.ReplacementProductId??DBNull.Value);ins.Parameters.AddWithValue("$newName",newName);ins.Parameters.AddWithValue("$newQty",request.ReplacementQuantity);ins.Parameters.AddWithValue("$newTotal",newTotal);ins.Parameters.AddWithValue("$diff",diff);ins.Parameters.AddWithValue("$settlement",request.Settlement);ins.Parameters.AddWithValue("$reason",request.Reason.Trim());ins.ExecuteNonQuery();}
            if(diff>0 && request.Settlement=="Tahsilat"){using var p=c.CreateCommand();p.Transaction=tx;p.CommandText="INSERT INTO Tahsilatlar(SatisID,Tutar,Aciklama) VALUES($id,$amount,'Değişim fiyat farkı tahsilatı')";p.Parameters.AddWithValue("$id",request.SaleId);p.Parameters.AddWithValue("$amount",diff);p.ExecuteNonQuery();}
            tx.Commit();
        } catch { tx.Rollback(); throw; }
    }

    private static long? FindOrCreateCustomer(SqliteConnection connection, SqliteTransaction transaction, string name)
    {
        if (name.Length == 0)
            return null;

        using (var findCommand = connection.CreateCommand())
        {
            findCommand.Transaction = transaction;
            findCommand.CommandText = "SELECT MusteriID FROM Musteriler WHERE lower(AdSoyad)=lower($name) AND Telefon='' LIMIT 1";
            findCommand.Parameters.AddWithValue("$name", name);
            var result = findCommand.ExecuteScalar();
            if (result is not null && result is not DBNull)
                return Convert.ToInt64(result);
        }

        using var insertCommand = connection.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = "INSERT INTO Musteriler(AdSoyad) VALUES($name); SELECT last_insert_rowid();";
        insertCommand.Parameters.AddWithValue("$name", name);
        return Convert.ToInt64(insertCommand.ExecuteScalar());
    }

    private static DataTable ReadTable(string sql, params (string Name, object Value)[] parameters)
    {
        using var connection = DatabaseManager.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);
        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    private static void Execute(string sql, params (string Name, object Value)[] parameters)
    {
        using var connection = DatabaseManager.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);
        command.ExecuteNonQuery();
    }

    private static void AddParameters(SqliteCommand command, IEnumerable<(string Name, object Value)> parameters)
    {
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);
    }
}
