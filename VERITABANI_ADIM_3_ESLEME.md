# Veri geçiş eşlemesi — Adım 3

Bu eşleme, uygulamanın mevcut SQLite tabloları ile SQL Server hedefini karşılaştırır. Adım 2'de erişilebilen kaynak ve hedef veritabanlarının boş olduğu doğrulanmıştır; bu belge gerçek kayıtlar bulunduğunda uygulanacak dönüşüm kurallarını tanımlar. Bu aşamada canlı şema veya veri değiştirilmedi.

## Kanonik hedef nesneler

Hedef tablo adları SQL Server'daki mevcut adlarla devam eder: `Kategori`, `AltKategoriler`, `Urunler`, `Musteriler`, `Satis`, `SatisDetay`, `Tahsilatlar`. İade kapsamı ayrıca kararlaştırılıp şemaya eklendiğinde `Iade` ve `IadeDetay` kullanılacaktır. Beden ve renk kapsam dışıdır; hedefte tutulmayacaktır.

## Tablo ve sütun eşlemesi

| SQLite kaynak | SQL Server hedef | Dönüşüm / null / varsayılan |
|---|---|---|
| `Kategoriler.KategoriID` | `Kategori.KategoriID` | `int IDENTITY`; kaynak/hedef boşken kimlik korunarak yüklenir. Foreign key eşlemesi kaynak ID üzerinden kurulur, satır sırasına göre eşleştirme yapılmaz. |
| `Kategoriler.KategoriAdi` | `Kategori.KategoriAdi` | `nvarchar(100) NOT NULL`; metin trim edilir. Yinelenen adlar varsa otomatik birleştirilmez; eşleme belirsizliği olarak raporlanır. `AktifMi` kaynakta yoktur, hedef varsayılanı `1` olur. |
| `Urunler.UrunID` | `Urunler.UrunID` | `int IDENTITY`; kaynak/hedef boşken kimlik korunur. Yükleme sonrasında identity değeri en büyük aktarılan ID'nin ilerisine alınır. |
| `Urunler.UrunAdi` | `Urunler.UrunAdi` | `nvarchar(150) NOT NULL`; trim edilir, kaynakta boş/null varsa satır aktarımı durdurulur. |
| `Urunler.Kategori` | `Urunler.KategoriID` | Serbest metin bir foreign key'e dönüştürülür. Önce kategori tablosundaki adla eşleştirilir; yalnız ürünlerde geçen eksik kategori adları kategori olarak eklenir. Boş değer `Kategori Belirtilmedi` kategorisine bağlanır. Aynı metin birden fazla kategoriye eşleşirse işlem durdurulup elle eşleme istenir. |
| *(kaynakta yok)* | `Urunler.AltKategoriID` | `NULL`; mevcut SQLite modelinde alt kategori bulunmuyor. |
| `Urunler.AlisFiyati` | `Urunler.AlisFiyati` | Hedeften `decimal(18,2)` beklenir. `REAL` değeri iki ondalık basamağa yuvarlanır; hedef aralığı dışında değer varsa aktarım durur. |
| `Urunler.SatisFiyati` | `Urunler.SatisFiyati` | `decimal(18,2)`; alış fiyatıyla aynı dönüşüm ve aralık kontrolü. |
| `Urunler.Stok` | `Urunler.StokMiktari` | `int`; negatif stok veya tam sayı olmayan/taşan değer aktarım öncesi raporlanır, sessizce değiştirilmez. |
| `Urunler.Aktif` | `Urunler.AktifMi` | `INTEGER` 0/1 → `bit`; kaynakta yalnız 0 veya 1 kabul edilir. |
| `Urunler.OlusturmaTarihi` | `Urunler.OlusturmaTarihi` | Hedefte `datetime2(0)`. Kaynak `YYYY-MM-DD HH:MM:SS` metni olduğu gibi yerel tarih/saat olarak okunur; saat dilimi/UTC dönüşümü yapılmaz. Geçersiz metin aktarımı durdurur. |
| `Musteriler.MusteriID` | `Musteriler.MusteriID` | `int IDENTITY`; kimlik korunur ve satış foreign key'leri aynı kimlik eşlemesiyle taşınır. |
| `Musteriler.AdSoyad` | `Musteriler.AdSoyad` | `nvarchar(150) NOT NULL`; boş/null değer hata sayılır. |
| `Musteriler.Telefon` | `Musteriler.Telefon` | `nvarchar(30) NOT NULL`; kaynak null ise `''` kullanılır. Kaynak zaten boş varsayılanı kullanıyor. |
| `Musteriler.Notlar` | `Musteriler.Notlar` | `nvarchar(max) NOT NULL`; null ise `''`. |
| `Satislar.SatisID` | `Satis.SatisID` | `int IDENTITY`; kimlik korunur. |
| `Satislar.Tarih` | `Satis.SatisTarihi` | `datetime2(0)`; SQLite yerel tarih/saat metni saat dilimi eklenmeden korunur. Geçersiz tarih hata sayılır. |
| `Satislar.MusteriID` | `Satis.MusteriID` | Nullable foreign key; `NULL` aynen korunur, dolu değer `Musteriler` kimlik eşlemesine göre çevrilir. Eşleşmeyen müşteri ID'si aktarımı durdurur. |
| `Satislar.MusteriAdi` | `Satis.MusteriAdi` | Satış anındaki adın görüntü kopyası olarak `nvarchar(150) NOT NULL` saklanır; boş/null ise mevcut satış varsayılanı `Perakende` uygulanır. |
| `Satislar.Toplam` | `Satis.ToplamTutar` | `decimal(18,2)`; iki basamağa yuvarlama ve aralık kontrolü. |
| `Satislar.Odenen` | `Tahsilatlar` toplamı / bakiye görünümü | Hedefte ayrıca saklanan `Odenen` yerine tahsilat kayıtlarının toplamı esas alınır. Her satış için kaynak `Odenen` ile `SUM(Tahsilatlar.Tutar)` karşılaştırılır; uyuşmazlık varsa otomatik düzeltme yapılmadan aktarım durur ve raporlanır. |
| `Satislar.Notlar` | `Satis.Notlar` | `nvarchar(max) NOT NULL`; null ise `''`. |
| `SatisDetay.SatisDetayID` | `SatisDetay.SatisDetayID` | `int IDENTITY`; kimlik korunur. |
| `SatisDetay.SatisID` | `SatisDetay.SatisID` | Zorunlu foreign key; satış kimlik eşlemesine çevrilir. Eşleşme yoksa aktarım durur. |
| `SatisDetay.UrunID` | `SatisDetay.UrunID` | Nullable foreign key olmalıdır. SQLite'da ürün silinince değer `NULL` olurken `UrunAdi` korunur; SQL Server hedefindeki mevcut `NOT NULL` tanımı bu nedenle genişletilmelidir. |
| `SatisDetay.UrunAdi` | `SatisDetay.UrunAdi` | `nvarchar(150) NOT NULL`; satış anındaki ad kopyası korunur; ürün ID'si `NULL` olsa bile silinmiş ürün satış satırı kaybolmaz. |
| `SatisDetay.Adet` | `SatisDetay.Adet` | `int NOT NULL`; pozitif tam sayı kontrolü. |
| `SatisDetay.BirimFiyat` | `SatisDetay.BirimSatisFiyati` | `decimal(18,2)`; iki basamağa yuvarlanır ve taşma kontrol edilir. |
| *(SQLite kaynağında yok)* | `SatisDetay.BirimAlisFiyati` | Tarihsel alış maliyeti kaynakta saklanmadığı için eski satış satırlarında `NULL` olmalıdır. SQL Server'daki mevcut `NOT NULL` tanımı nullable yapılmalıdır. Ürünlerin bugünkü alış fiyatını geçmiş satış maliyeti gibi kopyalamak yanlış kâr raporu üretir. Yeni satışlar kendi maliyet görüntüsünü kaydetmelidir. |
| `Tahsilatlar.TahsilatID` | `Tahsilatlar.TahsilatID` | `int IDENTITY`; kimlik korunur. |
| `Tahsilatlar.SatisID` | `Tahsilatlar.SatisID` | Zorunlu foreign key; satış kimlik eşlemesine göre çevrilir. |
| `Tahsilatlar.Tutar` | `Tahsilatlar.Tutar` | `decimal(18,2)`; pozitif tutar ve aralık kontrolü. |
| `Tahsilatlar.Tarih` | `Tahsilatlar.TahsilatTarihi` | `datetime2(0)`; yerel tarih/saat korunur. |
| `Tahsilatlar.Aciklama` | `Tahsilatlar.Aciklama` | `nvarchar(250) NOT NULL`; null ise `''`. |
| *(SQLite kaynağında yok)* | `Iade`, `IadeDetay` | Eski kaynakta iade kaydı bulunmuyor; aktarılacak satır yok. İade iş akışı devreye alınmadan önce hedef şeması ayrı tasarlanacak. |

## Tür, tarih ve para kuralları

- SQLite çalışma dosyasındaki gerçek `Kategoriler` tablosunda `KategoriAdi` için `UNIQUE` kısıtı yok; güncel `DatabaseManager.cs` tanımı ise `UNIQUE` içeriyor. Kaynak şema koddan varsayılmayacak, aktarım anında `sqlite_master` üzerinden okunacak. Adım 2'deki kaynak dosya boş olduğundan bu şema sürüm farkı kayıtları etkilemiyor.
- Para tutarlarında hedef tür `decimal(18,2)` olarak belirlenir. SQLite `REAL` IEEE kayan nokta değeridir; dönüşümden önce tüm tutarlar aralık ve iki basamaklı yuvarlama kontrolünden geçirilir. Yuvarlama farkları ve negatif/olağandışı değerler ayrı hata raporuna yazılır.
- SQLite tarihleri `datetime('now','localtime')` ile üretilen saat dilimsiz yerel metinlerdir. `datetime2(0)`'a dönüştürülür ve UTC varsayımıyla kaydırılmaz. İleride yeni kayıtlar için uygulama ve SQL Server'da tek bir saat dilimi standardı ayrıca belirlenmelidir.
- Boş metin ile `NULL` aynı kabul edilmez. Kaynakta ilişki alanlarında kullanılan `NULL` korunur; açıklama/telefon/not alanlarında kaynak varsayılanı `''` korunur.
- Kimlikler yalnız hedef tablolar boşken `IDENTITY_INSERT` ile korunabilir. Aktarım başında hedef tabloların boş olduğu yeniden kontrol edilir; doluysa kimlikleri ezmek yerine işlem durdurulur. Kimlik korunduktan sonra identity sayaçları en yüksek ID'nin sonrasına alınır.

## Aktarım öncesi durdurma kontrolleri

1. Kaynak SQLite ve hedef SQL Server yedeklenir; şema/kayıt sayıları aktarım başlangıcında tekrar alınır.
2. Hedef tablolar boş değilse bu kimlik koruma planı uygulanmaz.
3. Ürün kategori metinleri tek bir kategoriye eşlenemiyorsa manuel eşleme alınır.
4. Her satış için `Odenen` ile tahsilat toplamı eşit olmalıdır; aksi durumda fark çözülmeden yükleme yapılmaz.
5. Eksik müşteri/satış/ürün foreign key'i, hatalı tarih, para taşması, negatif stok veya geçersiz aktiflik değeri aktarımı durdurur.
6. Satış detayındaki eksik tarihsel alış maliyeti `NULL` kalır; bu satırlar kâr raporunda tarihsel maliyet bilinmiyor olarak ele alınır.

## Mevcut durum

Adım 2'de hem incelenen SQLite hem de SQL Server uygulama tablolarının boş olduğu bulundu. Bu yüzden yukarıdaki eşleme henüz gerçek kayıtlarla prova edilmedi. Şema farkları ve hedefte genişletilmesi gereken alanlar bellidir; gerçek verili bir kaynağa rastlanırsa aktarım öncesi durdurma kontrolleri o kopya üzerinde çalıştırılmalıdır.
