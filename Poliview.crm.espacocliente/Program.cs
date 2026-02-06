global using Microsoft.AspNetCore.Components.Authorization;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Poliview.crm.espacocliente;
using Blazored.Toast;
using CurrieTechnologies.Razor.SweetAlert2;
using Poliview.crm.infra;
using Poliview.crm.http.services;
using Poliview.crm.services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");



builder.Services.AddWMBSC(false);


var baseAddress = builder.Configuration.GetValue<string>("urlapicrm");
if (baseAddress == null) baseAddress = string.Empty;

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

builder.Services.AddHttpClient("crmHttpClient", client =>
{
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddScoped<IMensagemHttpService, MensagemHttpService>();
builder.Services.AddScoped<IArquivoDownloadHttpService, ArquivoDownloadHttpService>();
builder.Services.AddScoped<IGrupoMidiaHttpService, GrupoMidiaHttpService>();
builder.Services.AddScoped<IUsuarioHttpService, UsuarioHttpService>();
builder.Services.AddScoped<IEmpreendimentoHttpService, EmpreendimentoHttpService>();
builder.Services.AddScoped<IEmpreendimentoService, EmpreendimentoService>();
builder.Services.AddScoped<ITipoUnidadeService, TipoUnidadeService>();
builder.Services.AddScoped<IBlocoHttpService, BlocoHttpService>();
builder.Services.AddScoped<IUnidadeHttpService, UnidadeHttpService>();
builder.Services.AddScoped<IAutenticacaoHttpService, AutenticacaoHttpService>();
builder.Services.AddScoped<INotificacaoHttpService, NotificacaodHttpService>();
builder.Services.AddScoped<ITipoUnidadeHttpService, TipoUnidadeHttpService>();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<ILocalStorage, LocalStorage>();
builder.Services.AddBlazoredToast();
builder.Services.AddSweetAlert2();
builder.Services.AddBlazoredModal();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
