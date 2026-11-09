namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Parámetros del listado de pedidos (HU-01 y HU-02) tal como llegan en la query, todos como texto.
/// Así cada error de formato se informa con el nombre de su parámetro.
/// </summary>
/// <param name="Numero">HU-02: número del pedido (ej.: "9") o folio (ej.: "PED-ANT-000009").</param>
public sealed record ConsultaPedidos(
    string? ClienteId = null,
    string? Cliente = null,
    string? Estado = null,
    string? Desde = null,
    string? Hasta = null,
    string? Pagina = null,
    string? TamanoPagina = null,
    string? Numero = null);
