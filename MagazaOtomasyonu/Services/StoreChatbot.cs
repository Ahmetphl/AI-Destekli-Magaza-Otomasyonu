using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using MagazaOtomasyonu.Models;

namespace MagazaOtomasyonu.Services;

/// <summary>Rule-based shop assistant. Answers are computed from the active store database.</summary>
public sealed class StoreChatbot(IStoreRepository repository)
{
    private const int LowStockLimit = 5;
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public string OpeningMessage()
    {
        var rows = repository.GetProducts().Rows.Cast<DataRow>().ToList();
        var critical = rows.Where(r => Convert.ToInt32(r["Stok"]) <= LowStockLimit).OrderBy(r => Convert.ToInt32(r["Stok"])).Take(5).ToList();
        var message = "Merhaba! Mağazanın kayıtlarına bakarak net yanıtlar verebilirim. Örneğin “Bu ay nasıl gidiyor?”, “25.000 TL kâra ne zaman ulaşırım?”, “Azalan stokları göster” veya “Açık borçları listele” diye sorabilirsiniz.\n\nKâr tahminleri ürün alış/satış fiyatlarına dayanır; işletme giderleri düşülmez.";
        if (critical.Count > 0) message += $"\n\n⚠ Stok uyarısı · {LowStockLimit} adet ve altında:\n" + string.Join("\n", critical.Select(r => $"• {r["Ürün"]}: {r["Stok"]} adet"));
        return message;
    }

    public string Reply(string question)
    {
        var text = question.Trim().ToLower(Turkish);
        if (text.Length == 0) return "Sorunuzu yazın; satış, kâr hedefi, borç veya stok konusunda yardımcı olayım.";
        if (Has(text, "merhaba", "selam", "iyi gün", "günaydın")) return "Merhaba! Mağazanın güncel verilerine göre net yanıtlar verebilirim. Örneğin: “Bu ayki kârım ne kadar?”, “25.000 TL kâra ne zaman ulaşırım?” veya “Azalan stokları göster.”";
        if (Has(text, "yardım", "neler yap", "ne sorabilirim", "örnek")) return "Şunları sorabilirsiniz:\n• Bugünkü veya bu ayki satışım ne kadar?\n• Bu ay tahmini ürün kârım ne kadar?\n• 25.000 TL kâra ne zaman ulaşırım?\n• Gidişat böyle sürerse ay sonu ne olur?\n• Azalan stokları ve açık borçları göster.\n\nYanıtlar uygulamadaki kayıtlı verilere dayanır; kâr tahminleri giderler düşülmeden hesaplanır.";

        var summary = repository.GetDashboardSummary();
        if (Has(text, "stok", "azalan", "kritik")) return StockReply(text);
        if (Has(text, "borç", "borc", "alacak", "tahsilat")) return DebtReply(text, summary);
        if (Has(text, "gidişat", "gidisat", "böyle devam", "boyle devam", "ay sonu", "nasıl devam", "nasil devam", "öngörü", "ongoru", "tahmin", "bu ay nasıl", "bu ay nasil")) return TrendReply(summary);
        if (Has(text, "tavsiye", "öner", "oner", "ne yapmal", "ekonomik", "durumum", "durum nasıl", "durum nasil")) return AdviceReply(summary);
        if (Has(text, "hedef", "ulaş", "ulas", "ne zaman", "kâr", "kar", "kazanç", "kazanc"))
        {
            var target = ReadMoneyTarget(text);
            if (target is decimal goal) return ProfitGoalReply(goal, summary);
            if (Has(text, "ne zaman", "ulaş", "ulas", "hedef")) return $"Hedefinizi TL olarak yazın; örneğin “{Money(Math.Max(10000, summary.EstimatedProfit + 10000))} kâra ne zaman ulaşırım?”. Şu an bu ayki tahmini ürün kârı {Money(summary.EstimatedProfit)}.";
            return ProfitReply(summary);
        }
        if (Has(text, "bugün", "bugun", "günlük", "gunluk")) return $"Bugün kaydedilen satış toplamı {Money(summary.TodaySales)}. Bu, satış tutarıdır; tahsilat ve net kârla aynı şey değildir.";
        if (Has(text, "satış", "satis", "ciro", "aylık", "aylik")) return $"Bu ay kaydedilen satış toplamı {Money(summary.MonthSales)}. Tahsil edilen tutar {Money(summary.MonthCollections)}, açık müşteri bakiyesi toplamı {Money(summary.OpenDebt)}.";
        return "Bu konuda kayıtlı verilerden net yanıt bulamadım. “Bu ayki kârım”, “satışlarım”, “azalan stoklar”, “açık borçlar” ya da TL tutarlı bir kâr hedefi sorabilirsiniz.";
    }

    private string StockReply(string question)
    {
        var products = repository.GetProducts();
        var rows = products.Rows.Cast<DataRow>().ToList();
        var named = rows.FirstOrDefault(r => question.Contains(Convert.ToString(r["Ürün"])?.ToLower(Turkish) ?? "\u0000", StringComparison.Ordinal));
        if (named is not null)
        {
            var stock = Convert.ToInt32(named["Stok"]);
            return $"{named["Ürün"]} ürününde şu an {stock} adet stok var." + (stock <= LowStockLimit ? $" Kritik seviyede: belirlediğim uyarı eşiği {LowStockLimit} adet." : " Stok uyarı eşiğinin üzerinde.");
        }
        var low = rows.Where(r => Convert.ToInt32(r["Stok"]) <= LowStockLimit).OrderBy(r => Convert.ToInt32(r["Stok"])).Take(10).ToList();
        if (low.Count == 0) return $"Güzel haber: stok uyarı eşiğim olan {LowStockLimit} adet veya altında ürün yok.";
        return $"Stok uyarısı (eşik: {LowStockLimit} adet):\n" + string.Join("\n", low.Select(r => $"• {r["Ürün"]}: {r["Stok"]} adet")) + (rows.Count(r => Convert.ToInt32(r["Stok"]) <= LowStockLimit) > low.Count ? "\nDiğer kritik ürünler de var; liste ilk 10 ürünle sınırlandırıldı." : "") + "\n\nBu eşik mevcut uygulamada sabittir.";
    }

    private string DebtReply(string question, DashboardSummary summary)
    {
        var debts = repository.GetOpenDebts();
        if (debts.Rows.Count == 0) return "Açık borç görünmüyor. Kayıtlı satışlarda kalan bakiye bulunmuyor.";
        var total = debts.Rows.Cast<DataRow>().Sum(r => Convert.ToDecimal(r["Kalan"]));
        if (Has(question, "liste", "kim", "müşteri", "musteri", "hangi"))
        {
            var top = debts.Rows.Cast<DataRow>().OrderByDescending(r => Convert.ToDecimal(r["Kalan"])).Take(5);
            return $"Toplam açık bakiye {Money(total)}. En yüksek kalan bakiyeler:\n" + string.Join("\n", top.Select(r => $"• Fiş {r["Fiş"]} · {r["Müşteri"]}: {Money(Convert.ToDecimal(r["Kalan"]))}"));
        }
        return $"Açık müşteri bakiyesi {Money(total)} ve {debts.Rows.Count} satışta bakiye var. Bu ay tahsilat {Money(summary.MonthCollections)}. Tahsilatları düzenli takip etmek nakit akışını iyileştirebilir.";
    }

    private static string ProfitReply(DashboardSummary s) => $"Bu ay kaydedilen satış: {Money(s.MonthSales)}. Tahmini ürün brüt kârı: {Money(s.EstimatedProfit)}.\n\nBu kâr, kayıtlardaki satış ve alış fiyatı farkından hesaplanır; kira, maaş, vergi, komisyon ve diğer giderler düşülmediği için net işletme kârı değildir. İade/değişimlerin kâr etkisi de bu hızlı tahminde tam yansımayabilir.";

    private static string TrendReply(DashboardSummary s)
    {
        var day = Math.Max(1, DateTime.Today.Day); var days = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
        var projectedSales = s.MonthSales / day * days; var projectedProfit = s.EstimatedProfit / day * days;
        return $"Bu ayın {day}. gününe kadarki kayıtları aynı günlük ortalamayla ay sonuna uzatırsam:\n• Tahmini satış: {Money(projectedSales)}\n• Tahmini ürün brüt kârı: {Money(projectedProfit)}\n\nBu bir senaryodur, garanti değildir. Ay içinde satışların eşit dağıldığını varsayar; işletme giderlerini ve iade/değişimlerin kâr etkisini tam içermez.";
    }

    private static string ProfitGoalReply(decimal goal, DashboardSummary s)
    {
        if (goal <= 0) return "Kâr hedefi sıfırdan büyük olmalı. Örnek: “25.000 TL kâra ne zaman ulaşırım?”";
        var current = s.EstimatedProfit;
        if (current >= goal) return $"Bu ay kaydedilen tahmini ürün brüt kârı {Money(current)}; yazdığınız {Money(goal)} hedefi aşılmış görünüyor. İşletme giderleri düşülmediğinden bu, net kâr hedefinin karşılandığı anlamına gelmez.";
        var elapsedDays = Math.Max(1, DateTime.Today.Day);
        var dailyProfit = current / elapsedDays;
        if (dailyProfit <= 0) return $"Bu ayki tahmini ürün brüt kârı {Money(current)}. Şu anki veriler pozitif bir günlük kâr hızı göstermediği için {Money(goal)} hedefine ulaşma tarihi hesaplanamıyor. Yeni kârlı satış kayıtları oluştuğunda tekrar sorabilirsiniz.";
        var remaining = goal - current;
        var daysToGoal = Math.Max(1, (int)Math.Ceiling(remaining / dailyProfit));
        var date = DateTime.Today.AddDays(daysToGoal);
        return $"Bu ayki tahmini ürün brüt kârı {Money(current)}. Ay başından bugüne ortalama günlük hız yaklaşık {Money(dailyProfit)}.\n\nAynı hız sürerse {Money(goal)} hedefi yaklaşık {date.ToString("d MMMM yyyy", Turkish)} tarihinde yakalanabilir ({daysToGoal} gün). Bu tahmin işletme giderlerini düşmez, iade/değişim etkisini tam yansıtmayabilir ve satış temposunun değişmediğini varsayar.";
    }

    private string AdviceReply(DashboardSummary s)
    {
        var advice = new List<string>();
        var products = repository.GetProducts();
        var lowCount = products.Rows.Cast<DataRow>().Count(r => Convert.ToInt32(r["Stok"]) <= LowStockLimit);
        if (lowCount > 0) advice.Add($"• Stok: {LowStockLimit} adet ve altında {lowCount} ürün var; önce hızlı satan kritik ürünleri gözden geçirin.");
        if (s.OpenDebt > 0) advice.Add($"• Nakit akışı: açık bakiye {Money(s.OpenDebt)}. Vadesi gelen alacakları takip etmek faydalı olabilir.");
        if (s.MonthSales > 0)
        {
            var margin = s.EstimatedProfit / s.MonthSales * 100m;
            advice.Add($"• Kayıtlı ürün brüt kâr oranı yaklaşık %{margin:N1}. Ürün bazında alış maliyeti ve satış fiyatını düzenli kontrol edin.");
        }
        else advice.Add("• Bu ay henüz satış kaydı yok; önce düzenli satış ve maliyet verisi biriktirmek güvenilir değerlendirme sağlar.");
        return "Kayıtlı mağaza verilerine göre kısa değerlendirme:\n" + string.Join("\n", advice) + "\n\nBu öneriler operasyonel göstergelere dayanır; kâr oranı işletme giderleri düşülmeden hesaplanır.";
    }

    private static decimal? ReadMoneyTarget(string text)
    {
        var match = Regex.Match(text, @"(?<!\d)(\d[\d\.,]*)(?:\s*(bin|milyon))?\s*(?:₺|tl|lira)?", RegexOptions.IgnoreCase);
        if (!match.Success || !decimal.TryParse(match.Groups[1].Value, NumberStyles.Number, Turkish, out var value)) return null;
        if (match.Groups[2].Success) value *= match.Groups[2].Value.Equals("bin", StringComparison.OrdinalIgnoreCase) ? 1000 : 1000000;
        return value;
    }

    private static bool Has(string text, params string[] terms) => terms.Any(text.Contains);
    private static string Money(decimal amount) => amount.ToString("N2", Turkish) + " ₺";
}
