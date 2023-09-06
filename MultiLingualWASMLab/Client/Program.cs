using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MultiLingualWASMLab.Client;
using MultiLingualWASMLab.Client.RefitClient;
using MudBlazor.Services;
using Refit;
using Blazored.LocalStorage;
using System.Globalization;

//## �h��y�t - FluentValidation
FluentValidation.ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) => GT.ResolveDisplayName(member);

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//## �h��y�t
builder.Services.AddLocalization(options =>
{
  options.ResourcesPath = "Resources";
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorageAsSingleton();

//## ���U RefitClient API�C --- ��ʤ@�Ӥ@�ӵ��U
builder.Services
    .AddRefitClient<IWeatherForecastApi>()
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

//## ���U RefitClient API�C --- ��ʤ@�Ӥ@�ӵ��U
builder.Services
    .AddRefitClient<IOrderApi>()
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

//���� await builder.Build().RunAsync(); ----------------------------------------
var host = builder.Build();
await host.SetDefaultCultureAsync(); // �]�w�w�]�y�t
await host.RunAsync();
