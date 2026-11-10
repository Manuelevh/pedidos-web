using PedidosWeb.Core.Lineas;
using PedidosWeb.Core.Pedidos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IPedidoRepositorio, PedidoRepositorioEnMemoria>();
builder.Services.AddSingleton<ListadoPedidos>();
builder.Services.AddSingleton<ILineaPedidoRepositorio, LineaPedidoRepositorioEnMemoria>();
builder.Services.AddSingleton<DetallePedidoService>();

var app = builder.Build();

// Para comprobar que la API está arriba.
app.MapGet("/salud", () => Results.Ok(new { estado = "ok" }));

// Detalle de un pedido por id.
app.MapGet("/api/pedidos/{id:int}", (int id, IPedidoRepositorio repositorio) =>
    repositorio.ObtenerPorId(id) is { } pedido
        ? Results.Ok(pedido)
        : Results.NotFound());

// HU-03 · Detalle de un pedido con sus líneas y total (specs/HU-03-detalle-pedido/spec.md).
// Si no existe: 404 con un mensaje claro para el vendedor, no un error técnico.
app.MapGet("/api/pedidos/{id:int}/detalle", (int id, DetallePedidoService servicio) =>
    servicio.ObtenerDetalle(id) is { } detalle
        ? Results.Ok(detalle)
        : Results.NotFound(new { mensaje = $"No encontramos el pedido {id}. Revisa el número e intenta de nuevo." }));

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
