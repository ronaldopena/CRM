using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Poliview.crm;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:9533") });
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://projcrm.poliview.com.br:8181/CRM_4_3_10/apicrm/") });

// Adicionar serviços do MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
