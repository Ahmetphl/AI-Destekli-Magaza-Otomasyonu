# Veritabanı geçişi – 7. adım

## Satış ve tahsilat bütünlüğü

Her iki repository’de de satış başlığı, tüm satış detayları, stok düşümleri ve satış anındaki ilk tahsilat aynı transaction kapsamında yürütülür. Bir detay veya stok güncellemesi başarısız olursa işlem tamamlanmaz; SQL Server tarafı açıkça rollback yapar, SQLite transaction ise tamamlanmadan dispose edildiğinde geri alınır.

Stok düşümü ürün aktifliği ve eldeki adet koşuluyla tekrar doğrulanır. Tahsilat ekleme sırasında satışın ödenen tutarı yalnızca kalan borç yeterliyse artırılır; tahsilat kaydı ve bu artış aynı transaction’da yapılır. Tutarlar iki ondalık basamakla sınırlandırılır; SQL Server `decimal(18,2)` sınırını aşacak satış toplamı reddedilir.

## Satış anı fiyat/maliyet kararı

- Satış fiyatı: sepetteki `SaleLine.UnitPrice`, satış detayındaki `BirimFiyat` / `BirimSatisFiyati` alanına anlık görüntü olarak yazılır.
- Ürün adı: satış tamamlanırken ürün tablosundan okunup detay satırına kopyalanır.
- Alış maliyeti: satış tamamlanırken ürünün geçerli `AlisFiyati` değeri `SatisDetay.BirimAlisFiyati` alanına kopyalanır. Ürünün alış fiyatı ileride değiştirilse bile eski satış kârı değişmez.
- Eski satış satırlarında maliyet bilinmiyorsa `BirimAlisFiyati` `NULL` kalır. Kâr hesabı bugünkü ürün alış fiyatına geri dönmez; maliyeti bilinmeyen satırlar tahmini kâr toplamına katılmaz.

## Şema uyumluluğu

SQLite açılış şeması yeni satış detaylarında nullable `BirimAlisFiyati` oluşturur. Var olan SQLite dosyasında kolon yoksa `DatabaseManager.CreateTables()` boş kolon ekleyen güvenli `ALTER TABLE ... ADD COLUMN` uyarlamasını yapar; geçmiş kayıtların maliyeti uydurulmaz. SQL Server için bu alanı nullable yapan değişiklik 6. adım hazırlık betiğinde bulunuyor.

Bu adımda kod/şema hazırlığı yapıldı; satışları üretim verisi üzerinde çalıştırma veya SQL Server betiğini canlı veritabanına uygulama yapılmadı. SQL Server paket restore engeli nedeniyle derleme de doğrulanamadı.
