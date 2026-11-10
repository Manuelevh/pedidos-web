namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Detalle de un pedido para el vendedor (HU-03, specs/HU-03-detalle-pedido/spec.md).
/// </summary>
/// <param name="Id">Número correlativo del pedido.</param>
/// <param name="Folio">Folio con formato PED-{SUCURSAL}-{NUMERO}.</param>
/// <param name="Fecha">Fecha en que se emitió el pedido.</param>
/// <param name="Estado">Estado del pedido, tal como lo guarda el legado.</param>
/// <param name="Sucursal">Código de la sucursal que emitió el pedido.</param>
/// <param name="ClienteId">Código del cliente, tal como viene en el pedido.</param>
/// <param name="ClienteNombre">Nombre del cliente, tal como viene en el pedido.</param>
/// <param name="Lineas">Líneas del pedido, ordenadas por número. Vacía si no tiene líneas.</param>
/// <param name="Total">Suma de los subtotales de las líneas, en CLP, sin IVA. 0 si no tiene líneas.</param>
public sealed record DetallePedido(
    int Id,
    string Folio,
    DateOnly Fecha,
    string Estado,
    string Sucursal,
    string ClienteId,
    string ClienteNombre,
    IReadOnlyList<LineaDetallePedido> Lineas,
    decimal Total);
