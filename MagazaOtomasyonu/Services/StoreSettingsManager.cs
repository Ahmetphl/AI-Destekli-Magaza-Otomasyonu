using System.Text.Json;
using MagazaOtomasyonu.Models;

namespace MagazaOtomasyonu.Services;

public static class StoreSettingsManager
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MagazaOtomasyonu", "magaza-ayarlar.json");

    public static StoreSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return StoreSettings.Default;
            return JsonSerializer.Deserialize<StoreSettings>(File.ReadAllText(SettingsPath)) ?? StoreSettings.Default;
        }
        catch (IOException) { return StoreSettings.Default; }
        catch (JsonException) { return StoreSettings.Default; }
    }

    public static void Save(StoreSettings settings)
    {
        var folder = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(folder);
        var tempPath = SettingsPath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(tempPath, SettingsPath, true);
    }
}
