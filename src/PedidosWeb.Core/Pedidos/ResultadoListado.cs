namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Resultado de listar pedidos: una página o los errores de validación por parámetro.
/// </summary>
public sealed class ResultadoListado
{
    private ResultadoListado(PaginaPedidos? pagina, IReadOnlyDictionary<string, string[]> errores)
    {
        Pagina = pagina;
        Errores = errores;
    }

    /// <summary>La página pedida, o null si hubo errores.</summary>
    public PaginaPedidos? Pagina { get; }

    /// <summary>Mensajes de error por nombre de parámetro. Vacío si la consulta es válida.</summary>
    public IReadOnlyDictionary<string, string[]> Errores { get; }

    public bool EsValido => Errores.Count == 0;

    public static ResultadoListado Exito(PaginaPedidos pagina) =>
        new(pagina, new Dictionary<string, string[]>());

    public static ResultadoListado ConErrores(IReadOnlyDictionary<string, string[]> errores) =>
        new(null, errores);
}
