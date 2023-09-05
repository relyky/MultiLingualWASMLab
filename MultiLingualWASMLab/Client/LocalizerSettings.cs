namespace MultiLingualWASMLab.Client;

public static class LocalizerSettings
{
  public static CultureWithName NeutralCulture = new CultureWithName("English", "en-US");
  public static readonly List<CultureWithName> SupportedCulturesWithName = new List<CultureWithName>()
  {
    new CultureWithName("English", "en-US"),
    new CultureWithName("中文","zh-TW")
  };
}

public record CultureWithName
{
  public string Name { get; set; } = default!;
  public string Culture { get; set; } = default!;

  public CultureWithName(string name, string culture)
  {
    Name = name;
    Culture = culture;
  }
}
