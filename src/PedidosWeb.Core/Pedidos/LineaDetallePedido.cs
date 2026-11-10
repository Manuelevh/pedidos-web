namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Una línea dentro del detalle de un pedido (HU-03, specs/HU-03-detalle-pedido/spec.md).
/// </summary>
/// <param name="Numero">Número de la línea dentro del pedido, desde 1.</param>
/// <param name="Producto">Descripción del producto.</param>
/// <param name="Cantidad">Unidades pedidas.</param>
/// <param name="PrecioUnitario">Precio de una unidad en CLP, sin IVA: <c>decimal</c> sin decimales.</param>
/// <param name="Subtotal"><c>Cantidad × PrecioUnitario</c>, en CLP, sin IVA.</param>
public sealed record LineaDetallePedido(
    int Numero,
    string Producto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);
