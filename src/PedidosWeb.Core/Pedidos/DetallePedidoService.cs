using PedidosWeb.Core.Lineas;

namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Arma el detalle de un pedido con sus líneas y su total (HU-03, specs/HU-03-detalle-pedido/spec.md).
/// </summary>
public sealed class DetallePedidoService
{
    private readonly IPedidoRepositorio _pedidos;
    private readonly ILineaPedidoRepositorio _lineas;

    public DetallePedidoService(IPedidoRepositorio pedidos, ILineaPedidoRepositorio lineas)
    {
        _pedidos = pedidos;
        _lineas = lineas;
    }

    /// <summary>
    /// El detalle del pedido, o <c>null</c> si el pedido no existe.
    /// El total es la suma de los subtotales de las líneas, sin IVA.
    /// </summary>
    public DetallePedido? ObtenerDetalle(int id)
    {
        if (_pedidos.ObtenerPorId(id) is not { } pedido)
        {
            return null;
        }

        var lineas = _lineas.ObtenerPorPedido(id)
            .OrderBy(l => l.Numero)
            .Select(l => new LineaDetallePedido(
                l.Numero,
                l.Producto,
                l.Cantidad,
                l.PrecioUnitario,
                l.Cantidad * l.PrecioUnitario))
            .ToList();

        return new DetallePedido(
            pedido.Id,
            pedido.Folio,
            pedido.Fecha,
            pedido.Estado,
            pedido.Sucursal,
            pedido.ClienteId,
            pedido.ClienteNombre,
            lineas,
            lineas.Sum(l => l.Subtotal));
    }
}
