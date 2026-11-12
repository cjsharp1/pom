using System.Text.Json;

namespace Pom;

public class PomSettings
{
    public bool PlaySound { get; set; } = true;
    public bool ShowToast { get; set; } = false;
    public bool FlashWindow { get; set; } = false;
    public bool DarkMode { get; set; } = false;
    public double Opacity { get; set; } = 1.0;

    // Stored in %AppData%\Pom so settings persist when Pom runs as a
    // single-file exe (whose base directory is a temp extraction folder)
    // or from a read-only location such as Program Files.
    private static readonly string SettingsDir =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Pom");

    private static readonly string SettingsPath =
        Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static PomSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            // One-time migration from the old next-to-the-exe location.
            var legacyPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
            if (File.Exists(legacyPath))
            {
                try
                {
                    Directory.CreateDirectory(SettingsDir);
                    File.Copy(legacyPath, SettingsPath);
                }
                catch
                {
                    // Fall through to defaults if the copy fails.
                }
            }

            if (!File.Exists(SettingsPath))
            {
                var defaults = new PomSettings();
                defaults.Save();
                return defaults;
            }
        }

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<PomSettings>(json, JsonOptions) ?? new PomSettings();
        }
        catch
        {
            return new PomSettings();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Never let a failed settings write take down the app.
        }
    }
}
