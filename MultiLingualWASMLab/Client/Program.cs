using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MultiLingualWASMLab.Client;
using MultiLingualWASMLab.Client.RefitClient;
using MudBlazor.Services;
using Refit;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MultiLingualWASMLab.Client.Authentication;

//## 多國語系 - FluentValidation
FluentValidation.ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) => GT.ResolveDisplayName(member);

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//## 多國語系
builder.Services.AddLocalization(options =>
{
  options.ResourcesPath = "Resources"; // 指定『多國語系資源區』
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddBlazoredSessionStorageAsSingleton();

//## for Authz.
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddTransient<AuthHeaderHandler>();

//## 註冊 RefitClient API。 --- 手動一個一個註冊
builder.Services
    .AddRefitClient<IWeatherForecastApi>()
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthHeaderHandler>();

//## 註冊 RefitClient API。 --- 手動一個一個註冊
builder.Services
    .AddRefitClient<IOrderApi>()
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthHeaderHandler>();

//§§ await builder.Build().RunAsync(); ----------------------------------------
var host = builder.Build();
await host.SetDefaultCultureAsync(); // 設定(預設)語系
await host.RunAsync();
