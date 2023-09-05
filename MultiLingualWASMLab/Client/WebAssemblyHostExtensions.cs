using Blazored.LocalStorage;
using Blazored.LocalStorage.StorageOptions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

namespace MultiLingualWASMLab.Client;

public static class WebAssemblyHostExtensions
{
  public async static Task SetDefaultCultureAsync(this WebAssemblyHost host)
  {
    var localStorage = host.Services.GetRequiredService<ILocalStorageService>();
    var cultureString = await localStorage.GetItemAsync<string>("culture");

    CultureInfo cultureInfo = !String.IsNullOrWhiteSpace(cultureString)
        ? new CultureInfo(cultureString)
        : new CultureInfo(LocalizerSettings.NeutralCulture.Culture);

    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
  }
}
