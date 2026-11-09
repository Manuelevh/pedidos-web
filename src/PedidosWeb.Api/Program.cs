using PedidosWeb.Core.Pedidos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IPedidoRepositorio, PedidoRepositorioEnMemoria>();
builder.Services.AddSingleton<ListadoPedidos>();

var app = builder.Build();

// Para comprobar que la API está arriba.
app.MapGet("/salud", () => Results.Ok(new { estado = "ok" }));

// Detalle de un pedido por id.
app.MapGet("/api/pedidos/{id:int}", (int id, IPedidoRepositorio repositorio) =>
    repositorio.ObtenerPorId(id) is { } pedido
        ? Results.Ok(pedido)
        : Results.NotFound());

// HU-01 · Listar pedidos con filtros, orden y paginación (specs/HU-01-listar-pedidos/spec.md).
// HU-02 · Buscar por número o folio con «numero» (specs/HU-02-buscar-por-numero/spec.md).
// Los parámetros llegan como texto: ListadoPedidos valida y reporta cada error con su nombre.
app.MapGet("/api/pedidos", (
        string? clienteId,
        string? cliente,
        string? estado,
        string? desde,
        string? hasta,
        string? pagina,
        string? tamanoPagina,
        string? numero,
        ListadoPedidos listado) =>
    {
        var resultado = listado.Listar(
            new ConsultaPedidos(clienteId, cliente, estado, desde, hasta, pagina, tamanoPagina, numero));

        return resultado.EsValido
            ? Results.Ok(resultado.Pagina)
            : Results.ValidationProblem(resultado.Errores);
    });

app.Run();

// Permite crear tests de integración con WebApplicationFactory<Program> más adelante.
public partial class Program { }
