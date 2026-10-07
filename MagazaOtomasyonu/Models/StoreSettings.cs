namespace MagazaOtomasyonu.Models;

public sealed record StoreSettings(string StoreName, string Phone, string Address)
{
    public static StoreSettings Default { get; } = new("İmaj Giyim", "", "");
}
