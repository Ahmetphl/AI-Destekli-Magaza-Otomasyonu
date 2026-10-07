# Veritabanı doğrulaması — Adım 2

Tarih: 4 Ekim 2026

## İncelenen kaynaklar

- SQL Server Express üzerindeki `MagazaOtomasyonu` veritabanı salt okunur sorgularla incelendi.
- Uygulamanın Debug çalışma dizinindeki `MagazaOtomasyonu/bin/Debug/net8.0-windows/Magaza.db` SQLite dosyası salt okunur incelendi.
- Proje klasörlerinde başka bir `Magaza.db`, SQLite dosyası, yedek veya SQL Server yedek dosyası bulunmadı.
- `MagazaOtomasyonu.sql` bir şema oluşturma betiği; veri satırı ekleyen `INSERT` ifadeleri içermiyor.

## Gerçek SQL Server veritabanı

| Tablo | Kayıt |
|---|---:|
| `Kategori` | 0 |
| `AltKategoriler` | 0 |
| `Urunler` | 0 |
| `Satis` | 0 |
| `SatisDetay` | 0 |

Veritabanında ayrıca SQL Server tarafından kullanılan `sysdiagrams` sistem tablosu var. Uygulama tabloları yukarıdaki beş tablo.

Kimlik alanları `int IDENTITY(1,1)`. Satış ve alış fiyatları `decimal(10,2)`. Satış tarihi `datetime`. Şemada şu beş foreign key mevcut:

- `AltKategoriler.KategoriID` → `Kategori.KategoriID`
- `Urunler.KategoriID` → `Kategori.KategoriID`
- `Urunler.AltKategoriID` → `AltKategoriler.AltKategoriID`
- `SatisDetay.SatisID` → `Satis.SatisID`
- `SatisDetay.UrunID` → `Urunler.UrunID`

Bu ilişkilerin tamamında silme davranışı `NO ACTION`. Canlı SQL Server şemasında `Musteriler`, `Tahsilatlar` ve iade tabloları yok.

## SQLite çalışma veritabanı

`Magaza.db` içindeki tüm uygulama tablolarında kayıt sayısı 0. `PRAGMA integrity_check` sonucu `ok`; `PRAGMA foreign_key_check` ihlal döndürmedi.

SQLite ürün tablosunda halen `Beden` ve `Renk` sütunları bulunuyor. Bunlar uygulama kapsamı dışında olduğundan hedef şema ve sonraki ekran/veri erişimi düzenlemelerinde kaldırılacak.

## Adım 2 sonucu ve aktarım sınırı

Şu an erişilebilen iki veritabanı da boştur. Bu nedenle aktarılacak kimlik değerleri veya doğrulanabilir kayıt düzeyinde foreign key eşleşmeleri yok. Şema ve kayıt sayıları doğrulandı; veri taşınabilirliği ise gerçek kayıt içeren bir kaynak veritabanı bulunana kadar kanıtlanamaz. Daha sonra farklı bir üretim/kişisel veritabanı kullanılıyorsa aktarım planı o kaynağın yedeklenmiş kopyasıyla yeniden doğrulanmalıdır.

Mevcut boş SQL Server veritabanında bir veri aktarımı yapılmadı ve hiçbir tablo değiştirilmedi.
