# AI Destekli Mağaza Otomasyonu

Giyim mağazalarının ürün, stok, satış, müşteri borcu, tahsilat ve iade/değişim süreçlerini tek bir Windows masaüstü uygulamasında yönetmek için geliştirilmiş bir otomasyon projesidir. Uygulama ayrıca mağaza verilerinden kurallı yanıtlar üreten bir sohbet asistanı ve veritabanı yedekleme/geri yükleme araçları içerir.

> Sohbet asistanı harici bir üretken yapay zekâ servisine bağlanmaz. Uygulamadaki kayıtları ve tanımlı kuralları kullanarak yanıt verir.

## Özellikler

- Ürün, kategori ve stok yönetimi
- Satış oluşturma, satış detaylarını ve tahsilatları kaydetme
- Müşteri borçlarını ve parça parça tahsilatları izleme
- Satışa bağlı iade ve ürün değişimi kaydı
- SQLite veya SQL Server veri sağlayıcısı seçeneği
- Veritabanı yedekleme, doğrulama ve geri yükleme
- Mağaza ayarları ve mağaza hakkında/iletişim alanı
- Satış, kâr hedefi, borç ve azalan stoklar için kural tabanlı sohbet asistanı

## Teknolojiler

- C# ve .NET 8
- Windows Forms masaüstü arayüzü
- Guna.UI2.WinForms
- Microsoft.Data.Sqlite
- Microsoft.Data.SqlClient
- SQLite ve Microsoft SQL Server

## Projeyi açma

1. Windows üzerinde .NET 8 SDK ve Visual Studio 2022 (veya uyumlu bir .NET IDE'si) kurun.
2. `MagazaOtomasyonu.sln` çözümünü açın.
3. NuGet paketlerinin geri yüklenmesini bekleyin.
4. `MagazaOtomasyonu` projesini başlangıç projesi seçip çalıştırın.

SQLite yerel kullanım için uygundur. SQL Server kullanırken uygulamanın `appsettings.json` dosyasındaki bağlantı ayarlarını kendi ortamınıza göre düzenleyin. Gerçek parola veya erişim anahtarı gibi gizli bilgileri kaynak koduna eklemeyin.

## Veritabanı

`MagazaOtomasyonu.sql` SQL Server şema/kurulum betiğini içerir. Ek `VERITABANI_ADIM_*.md` ve `Database/*.sql` dosyaları şema eşlemesi ve geliştirme adımlarına ilişkin notlardır. Uygulama yerel SQLite veritabanını ilk çalıştırmada oluşturur. Yerel veritabanı dosyaları ve yedekler Git deposuna dahil edilmez.

## Mimari özeti

WinForms ekranları kullanıcı etkileşimini yürütür. Ekranlar, `IStoreRepository` sözleşmesi üzerinden veri işlemlerine erişir; `StoreRepositoryFactory` yapılandırmaya göre SQLite veya SQL Server uygulamasını seçer. İşlemlerin birden fazla tabloyu etkilediği satış ve iade/değişim akışlarında transaction kullanılır.

## Lisans ve kullanım

Bu depo mağaza otomasyonu projesinin kaynak kodunu içerir. Üçüncü taraf NuGet paketleri kendi lisanslarına tabidir. Projeyi kullanmadan önce mağaza verilerinizi korumak için yedekleme ve erişim ayarlarınızı kendi çalışma ortamınızda doğrulayın.
