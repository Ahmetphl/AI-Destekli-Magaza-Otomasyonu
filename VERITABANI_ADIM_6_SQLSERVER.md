# Veritabanı geçişi – 6. adım

## Yapılan hazırlık

- `Microsoft.Data.SqlClient` SQL Server sağlayıcısı proje bağımlılığına eklendi.
- `Database.Provider` ayarı ve `StoreRepositoryFactory` eklendi. Aktif değer `SQLite`; uygulama bu nedenle eski SQLite akışında kalır.
- `SqlServerStoreRepository`, `IStoreRepository` arayüzünü SQL Server sorguları ile uygular. Kimlikler `OUTPUT INSERTED` ile alınır; sayısal, tarih, metin ve nullable değerlerin parametre türleri açıkça tanımlanır.
- Satış başlığı, detayları, stok güncellemesi ve ilk tahsilat aynı SQL transaction içindedir. Ürün adı, satış fiyatı ve o anki alış fiyatı detay satırına kopyalanır.
- `Database/ADIM_6_SQLSERVER_SEMA_HAZIRLIGI.sql` mevcut SQL Server DDL’sine gereken müşteri, tahsilat, satış ve ürün alanlarını eklemek üzere hazırlandı. Beden/Renk alanı yoktur.

## SSMS neden değişmedi?

Şema SQL betiği otomatik çalıştırılmıyor ve `appsettings.json` içindeki sağlayıcı hâlâ `SQLite`. SQL Server betiği canlı veritabanına uygulanmadı; 9. adımda yedek/kopya üzerinde kontrollü doğrulamaya bırakıldı. Dolayısıyla SSMS’te henüz yeni tablo/kolon beklenmemeli.

## Doğrulama durumu / engel

Paket sürümü NuGet’in resmi Microsoft.Data.SqlClient sayfasında .NET 8 ile uyumlu görünen `7.0.3` olarak seçildi. Bu çalışma ortamı `api.nuget.org` DNS adresini çözemediği için paket geri yüklemesi tamamlanamadı (`NU1301`). Bu nedenle SQL Server kodunun derleme doğrulaması henüz yapılamadı. Windows/Visual Studio ortamında NuGet erişimi sağlandığında önce Restore, ardından Build yapılmalıdır. Mevcut paketsiz önbellek bu sağlayıcıyı içermiyor.

## Bir sonraki teknik işlem

1. NuGet erişimi olan geliştirme ortamında paketi restore edip derleyin.
2. Derleme hatalarını düzeltin.
3. Yedek SQL Server kopyasında şema betiğini deneyin ve eklenen şemayı doğrulayın.
4. Ancak bu kontrollerden sonra `Provider` değerini `SQLServer` yaparak ekran işlemlerini deneme aşamasına geçin.
