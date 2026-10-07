# Veritabanı geçişi – 8. adım

## Borç ekranı

Borç takibi, sol menüye ve WinForms tasarım görünümüne eklendi. Menü bağlantısı mevcut `DebtPage` ekranını açar; burada açık satışlar listelenir ve seçilen satışa tahsilat girilir. Tahsilatın borç sınırını aşma kontrolü repository katmanında da uygulanır.

## İş kapsamı kararları

- **İade/değişim:** İade ekranı fiş numarasıyla satış satırlarını bulur, daha önce iade edilenleri düşerek kalan adedi sınırlar ve iade edilen ürünü stoğa ekler. Değişim ürünü güncel satış fiyatı ve stoktan alınır; tutar farkı tahsilat, borç, para üstü veya müşteri alacağı olarak seçilir. Tüm stok, satış bakiyesi, tahsilat ve işlem geçmişi değişiklikleri aynı transaction içindedir. SQLite şeması uygulama açılışında oluşturulur; SQL Server için `Database/ADIM_6_SQLSERVER_SEMA_HAZIRLIGI.sql` betiği `IadeDegisim` tablosunu ekler. Ekranda son 100 işlem listelenir.
- **Yedekleme:** SQLite yedeği çalışan veritabanından çevrimiçi Backup API ile alınır, bütünlük ve gerekli tablolar kontrol edilir, sonra dosya atomik olarak yerine taşınır. SQL Server yedeği `COPY_ONLY`, `CHECKSUM` ve sonrasında `RESTORE VERIFYONLY` kullanır; SQL Server hizmet hesabının hedef klasöre yazma izni olmalıdır. Geri yüklemeden önce dosyanın bütünlüğü/veritabanı adı doğrulanır ve mevcut veritabanının otomatik yedeği alınır. Geri yükleme kullanıcı onayıyla başlar; SQL Server `SINGLE_USER`/`MULTI_USER` durumları hata halinde de toparlanır.
- **Ayarlar:** Mağaza adı ve iletişim bilgileri, görünüm/tarih/para tercihleri, sağlayıcı ve bağlantı profilinin görüntülenmesi. Parola varsa düz metin `appsettings.json` içinde saklanmamalı; kimlik bilgisi saklama yöntemi ayrıca ele alınmalı. Ekran henüz işlem yapmıyor.
- **Hakkında:** İşletme durumu, amacı, iletişim numaraları ve Instagram hesabı statik ekranda gösterilir. Dolap hesabı bilgisi geldiğinde eklenmek üzere işaretlidir.
- **Mağaza asistanı:** Menüdeki “Akıllı Asistan” kayıtlı satış, maliyet, borç ve stok verilerine göre kural tabanlı yanıt verir; harici yapay zekâ servisine/veri aktarımına ihtiyaç duymaz. Ay başından bugüne kârı günlük ortalamayla hedef tarihine uzatır ve ay sonu senaryosu verir. Kâr hesabı giderleri düşmez, iade/değişim etkisini tam yansıtmayabilir; yanıtlar kesin muhasebe sonucu değildir. Stok uyarı eşiği 5 adettir ve asistan açıldığında kritik ürünleri gösterir.

## Eski ve tasarım amaçlı kontroller

- `User_Controls/UcAnaSayfa` ve `User_Controls/UcUrunler` bu formun çalışma akışında kullanılmıyor; yeni ana form kendi sayfalarını repository üzerinden kuruyor. Kaynaklar şu aşamada silinmedi.
- `FrmAna.Designer.cs` içindeki `pnlDesign...` kontrolleri WinForms tasarımcısında çalışan ana ekranı temsil eden tasarım önizlemesidir. Çalışma anında `ShowPage` içerik alanını gerçek sayfayla yeniler. Visual Studio görünümünün korunması için bu önizleme kontrolleri tutuldu.
- Boş, hiçbir yere bağlanmamış eski olay metotları `FrmAna.cs` dosyasından kaldırıldı.

## Veritabanı etkisi

Bu adım SQL Server veya SQLite şemasına değişiklik uygulamaz. İade modülü için tablo tasarımı, işlem geçmişi ve kısmi iade kuralları ekran geliştirilirken ayrıca şemaya eklenecek. SQL Server betiği hâlâ canlı veritabanında çalıştırılmadı; SSMS’te tablo farkı oluşmamasının nedeni budur.
