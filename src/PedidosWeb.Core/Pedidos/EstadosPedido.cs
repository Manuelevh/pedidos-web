namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Estados de pedido aceptados en el filtro del listado (HU-01, RN5).
/// </summary>
public static class EstadosPedido
{
    /// <summary>Los cinco estados válidos, con el texto tal como lo guarda el legado.</summary>
    public static readonly IReadOnlyList<string> Validos =
        ["Pendiente", "Confirmado", "Despachado", "Entregado", "Anulado"];

    /// <summary>Indica si el texto es un estado válido, sin distinguir mayúsculas.</summary>
    public static bool EsValido(string estado) =>
        Validos.Any(v => string.Equals(v, estado, StringComparison.OrdinalIgnoreCase));
}
