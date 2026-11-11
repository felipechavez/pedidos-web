using Microsoft.AspNetCore.Mvc;
using PedidosWeb.Core.Lineas;
using PedidosWeb.Core.Pedidos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IPedidoRepositorio, PedidoRepositorioEnMemoria>();
builder.Services.AddSingleton<ILineaPedidoRepositorio, LineaPedidoRepositorioEnMemoria>();
builder.Services.AddSingleton<PedidoServicio>();
builder.Services.AddSingleton<DetallePedidoServicio>();

var app = builder.Build();

// Para comprobar que la API está arriba.
app.MapGet("/salud", () => Results.Ok(new { estado = "ok" }));

// Detalle de un pedido por id.
app.MapGet("/api/pedidos/{id:int}", (int id, IPedidoRepositorio repositorio) =>
    repositorio.ObtenerPorId(id) is { } pedido
        ? Results.Ok(pedido)
        : Results.NotFound());

// HU-02 · Detalle de un pedido con sus líneas y el total calculado desde ellas.
app.MapGet("/api/pedidos/{id:int}/detalle", (int id, DetallePedidoServicio servicio) =>
    servicio.ObtenerDetalle(id) is { } detalle
        ? Results.Ok(detalle)
        : Results.NotFound());

// HU-01 · Listar pedidos con filtros.
app.MapGet("/api/pedidos", (
    string? cliente,
    string? estado,
    DateOnly? fechaDesde,
    DateOnly? fechaHasta,
    int? pagina,
    [FromQuery(Name = "tamañoPagina")] int? tamañoPagina,
    string? ordenarPor,
    bool? ascendente,
    PedidoServicio servicio) =>
{
    try
    {
        var consulta = new ConsultaPedidos
        {
            Cliente = cliente,
            Estado = estado,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Pagina = pagina ?? 1,
            TamañoPagina = tamañoPagina ?? 10,
            OrdenarPor = ordenarPor ?? "fecha",
            Ascendente = ascendente ?? false
        };

        return Results.Ok(servicio.ListarPedidos(consulta));
    }
    catch (ValidacionException ex)
    {
        return Results.Problem(detail: ex.Detalle, statusCode: 400);
    }
});

app.Run();

// Permite crear tests de integración con WebApplicationFactory<Program> más adelante.
public partial class Program { }
