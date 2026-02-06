using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Poliview.crm.apicrm;
using Poliview.crm.api;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);

// Configuração do sistema de log personalizado
builder.Services.Configure<CustomLogOptions>(
    builder.Configuration.GetSection("CustomLog"));
builder.Services.AddSingleton<ConsoleLogService>();

builder.Host.UseWindowsService();

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Poliview CRM", Version = "4.3.5" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Jwt Authorization",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

builder.Services.AddLogging();

builder.Services.AddHttpLogging(logging =>
{
    // Customize HTTP logging here.
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("My-Request-Header");
    logging.ResponseHeaders.Add("My-Response-Header");
    logging.MediaTypeOptions.AddText("application/javascript");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "basewebApiIssuer",
        ValidAudience = "baseWebApiAudience",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("poliview.tecnologia.crm@2022"))
    };

});

var app = builder.Build();

app.MapGet("/", () =>
{
    return Results.Ok("Poliview CRM - APICRM 4.3.6");
});

// Configura o sistema de log personalizado
var consoleLogService = app.Services.GetRequiredService<ConsoleLogService>();
consoleLogService.ConfigureConsoleLogging();

app.UseCors("AllowAll");

app.UseHttpLogging();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
