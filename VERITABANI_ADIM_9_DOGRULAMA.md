# Veritabanı geçişi – 9. adım sonuçları

## Yedek ve test kopyası

- Kaynak SQL Server: `ahmet\SQLEXPRESS`, `MagazaOtomasyonu`.
- Ön kontrol: `Kategori`, `Urunler`, `Satis`, `SatisDetay` boştu; `Musteriler` ve `Tahsilatlar` tabloları henüz yoktu. Mevcut foreign key’lerin tümü etkin ve güvenilirdi.
- Copy-only yedek: `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\MagazaOtomasyonu_20261005_Adim9.bak`.
- `RESTORE VERIFYONLY ... WITH CHECKSUM` başarılı oldu.
- Yedek `MagazaOtomasyonu_Adim9_Test` adıyla ayrı veritabanına geri yüklendi. Şema betiği önce bu kopyada çalıştırıldı.

## Test kopyasındaki doğrulamalar

SQL Server repository üzerinden kategori/ürün oluşturma, satış başlığı ve detayları, stok düşümü, kısmi tahsilat, borç görünümü, tam tahsilat, rapor/dashboard sorguları, satış anında alış maliyeti görüntüsü ve yetersiz stokta işlemin reddi doğrulandı. Beklenen satış `2 × 20,50 = 41,00 ₺`, ilk tahsilat `10,00 ₺`, bakiye `31,00 ₺`; ürün maliyeti sonradan değiştirilmesine rağmen tahmini kâr `20,50 ₺` kaldı. Test kayıtları temizlendi.

## Asıl veritabanına uygulama

Test kopyası başarılı olduktan ve asıl tabloların hâlâ boş olduğu tekrar görüldükten sonra aynı betik asıl `MagazaOtomasyonu` veritabanına uygulandı. `Musteriler`, `Tahsilatlar`, müşteri/tahsilat alanları, satış detayındaki ürün adı ve nullable maliyet alanı ile ürün aktiflik/tarih alanları artık SSMS’te bu veritabanında görülebilir.

Uygulama yapılandırması `Provider=SQLServer` olarak değiştirildi. `Program.cs`, SQL Server seçiliyken SQLite şeması oluşturma adımını atlar; böylece çalışma verisi SQL Server repository’sine yönelir. SQLite `Magaza.db` dosyası silinmedi.

Asıl veritabanında repository’nin ekranlarda kullanılan kategori, ürün, stok, satış, açık borç, son satış ve dashboard okuma sorguları çalıştırıldı. Kayıt bulunmadığı doğrulandı ve tüm özetler sıfır döndü. Uygulama derlemesi 0 hata / 0 uyarı ile tamamlandı.

## Sınırlar

- Asıl SQL Server’da başlangıçta kullanıcı iş kayıtları bulunmadığı için gerçek eski kayıtlarla veri aktarımı yapılmadı. SQLite `Magaza.db` dosyası da SQL Server’a yüklenmedi; adım 2’de incelenen kaynak boştu.
- SQL Server repository işlemleri test kopyasında doğrulandı; asıl veritabanına yalnızca salt okunur ekran/rapor sorguları gönderildi.
- İade/backup/ayar iş akışları 8. adımda tanımlanan kapsamdadır, ancak bu modüllerin işlemsel davranışı bu veritabanı geçişi doğrulamasına dahil değildir.
