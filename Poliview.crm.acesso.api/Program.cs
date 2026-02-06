
using System.Data.SqlClient;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var _connectionString = builder.Configuration.GetConnectionString("Conexao");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    using var connection = new SqlConnection(_connectionString);
    var query = $"select * from OPE_CONFIG";
    var retorno = connection.Query<Acesso>(query);
    return Results.Ok(retorno);
});

app.MapGet("/acesso/{chaveacessobase64}", ([FromRoute] string chaveacessobase64) =>
{
    var retorno = new Retorno();

    Console.WriteLine($"chave de acesso={chaveacessobase64}");

    try
    {
        byte[] bytes = Convert.FromBase64String(chaveacessobase64);
        string chaveacesso = Encoding.UTF8.GetString(bytes);
        Console.WriteLine($"chave de acesso={chaveacesso}");

        using var connection = new SqlConnection(_connectionString);
        var query = $"select * from OPE_CONFIG WHERE chaveacesso='{chaveacesso}' ";
        var registro = connection.Query<Acesso>(query);

        if (registro.Count()>0)
        {
            retorno.sucesso = 1;
            retorno.mensagem = "Ok";
            retorno.objeto = registro.FirstOrDefault();

        }
        else
        {
            retorno.sucesso = 0;
            retorno.mensagem = "chave de acesso não encontrada!";
            retorno.objeto = registro;

        }

        return Results.Ok(retorno);

    }
    catch (Exception ex)
    {
        retorno.sucesso = 0;
        retorno.mensagem = ex.Message;
        retorno.objeto = null;
        return Results.Ok(retorno);
    }

});

app.Run();

public class Acesso
{
    public string? urlapi { get; set; }
    public string? chaveacesso { get; set; }
    public string? principal { get; set; }
    public string? dark { get; set; }
    public string? light { get; set; }
    public string? texto { get; set; }
    public string? branco { get; set; }
    public string? urllogo { get; set; }
    public string? fundo { get; set; }
    public string? urlportal { get; set; }
    public string? urlapisiecon { get; set; }
    public string? urlimgprincipal { get; set; }
    public int idempresa { get; set; }
    public int id { get; set; }
}

public class Retorno
{
    public int sucesso { get; set; }
    public string? mensagem { get; set; }
    public object? objeto { get; set; }
}

