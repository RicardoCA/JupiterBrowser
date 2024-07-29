using JupiterBrowser;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class ColorPersistence
{
    private static string filePath = "siteColors.json";

    public static void SaveColors(List<SiteColorInfo> siteColorInfos)
    {
        var json = JsonSerializer.Serialize(siteColorInfos, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public static List<SiteColorInfo> LoadColors()
    {
        if (!File.Exists(filePath))
        {
            return new List<SiteColorInfo>();
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<SiteColorInfo>>(json);
    }
}
