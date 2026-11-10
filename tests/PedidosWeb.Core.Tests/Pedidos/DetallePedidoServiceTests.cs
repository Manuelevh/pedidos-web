using PedidosWeb.Core.Lineas;
using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

// Criterios de aceptación de specs/HU-03-detalle-pedido/spec.md.
public class DetallePedidoServiceTests
{
    private readonly DetallePedidoService _servicio =
        new(new PedidoRepositorioEnMemoria(), new LineaPedidoRepositorioEnMemoria());

    [Fact]
    public void ObtenerDetalle_PedidoExistente_DevuelveDatosDelPedidoYLineasEnOrden() // CA-01
    {
        var detalle = _servicio.ObtenerDetalle(8);

        Assert.NotNull(detalle);
        Assert.Equal(8, detalle.Id);
        Assert.Equal("PED-STGO-000008", detalle.Folio);
        Assert.Equal("C-001", detalle.ClienteId);
        Assert.Equal("Ferretería El Clavo", detalle.ClienteNombre);
        Assert.Equal("Entregado", detalle.Estado);
        Assert.Equal("STGO", detalle.Sucursal);
        Assert.Equal(new DateOnly(2026, 10, 20), detalle.Fecha);
        Assert.Equal(new[] { 1, 2, 3 }, detalle.Lineas.Select(l => l.Numero));
    }

    [Fact]
    public void ObtenerDetalle_PedidoExistente_CalculaSubtotalesYTotalSinIva() // CA-02
    {
        var detalle = _servicio.ObtenerDetalle(8);

        Assert.NotNull(detalle);
        Assert.Equal(new[] { 64500m, 35400m, 20200m }, detalle.Lineas.Select(l => l.Subtotal));
        Assert.All(detalle.Lineas, l => Assert.Equal(l.Cantidad * l.PrecioUnitario, l.Subtotal));
        Assert.Equal(120100m, detalle.Total);
        Assert.Equal(new PedidoRepositorioEnMemoria().ObtenerPorId(8)!.Total, detalle.Total);
    }

    [Fact]
    public void ObtenerDetalle_PedidoInexistente_DevuelveNull() // CA-03 (en Core; el mensaje se prueba en la API)
    {
        Assert.Null(_servicio.ObtenerDetalle(999));
    }

    [Fact]
    public void ObtenerDetalle_PedidoSinLineas_DevuelveListaVaciaYTotalCero() // CA-04
    {
        var servicio = new DetallePedidoService(new PedidoRepositorioEnMemoria(), new LineasVacias());

        var detalle = servicio.ObtenerDetalle(8);

        Assert.NotNull(detalle);
        Assert.Empty(detalle.Lineas);
        Assert.Equal(0m, detalle.Total);
    }

    [Fact]
    public void ObtenerDetalle_PedidoAnulado_LoDevuelveConSuEstado() // CA-05
    {
        var detalle = _servicio.ObtenerDetalle(2);

        Assert.NotNull(detalle);
        Assert.Equal("Anulado", detalle.Estado);
        Assert.NotEmpty(detalle.Lineas);
    }

    // Ningún pedido de ejemplo está sin líneas: este repositorio falso simula ese caso.
    private sealed class LineasVacias : ILineaPedidoRepositorio
    {
        public IReadOnlyList<LineaPedido> ObtenerPorPedido(int pedidoId) => Array.Empty<LineaPedido>();
    }
}
