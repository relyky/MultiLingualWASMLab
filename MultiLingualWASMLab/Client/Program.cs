using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MultiLingualWASMLab.Client;
using MultiLingualWASMLab.Client.RefitClient;
using MudBlazor.Services;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

//## ���U RefitClient API�C --- ��ʤ@�Ӥ@�ӵ��U
builder.Services
    .AddRefitClient<IWeatherForecastApi>()
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync();
