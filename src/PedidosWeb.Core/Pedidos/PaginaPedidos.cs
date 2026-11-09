namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Una página del listado de pedidos (HU-01).
/// </summary>
/// <param name="Items">Pedidos de la página, ordenados por fecha e id descendentes.</param>
/// <param name="Total">Cantidad de pedidos que cumplen los filtros, antes de paginar.</param>
/// <param name="Pagina">Página devuelta, desde 1.</param>
/// <param name="TamanoPagina">Pedidos por página.</param>
public sealed record PaginaPedidos(
    IReadOnlyList<Pedido> Items,
    int Total,
    int Pagina,
    int TamanoPagina);
