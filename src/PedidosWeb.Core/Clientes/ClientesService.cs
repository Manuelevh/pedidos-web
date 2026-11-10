namespace PedidosWeb.Core.Clientes;

/// <summary>
/// Consultas sobre los clientes de la distribuidora: la ficha para la pantalla y si está activo.
/// Módulo heredado, ordenado en la Unidad 3 sin cambiar su comportamiento (ver ClientesServiceTests).
/// </summary>
public class ClientesService
{
    private readonly List<Cliente> _clientes;

    public ClientesService() : this(ClientesDeEjemplo.Crear()) { }

    public ClientesService(List<Cliente> clientes)
    {
        _clientes = clientes;
    }

    /// <summary>
    /// Ficha del cliente para la pantalla, con el formato
    /// <c>Nombre | RUT | Zona | Activo/Inactivo</c>. Texto vacío si el cliente no existe.
    /// </summary>
    public string ObtenerFicha(string id)
    {
        if (BuscarPorId(id) is not { } cliente)
        {
            return "";
        }

        var estado = cliente.Activo ? "Activo" : "Inactivo";
        return $"{cliente.Nombre} | {NormalizarRut(cliente.Rut)} | {ObtenerZona(cliente.Region)} | {estado}";
    }

    /// <summary><c>true</c> si el cliente existe y está activo.</summary>
    public bool EstaActivo(string id) => BuscarPorId(id)?.Activo == true;

    private Cliente? BuscarPorId(string id) => _clientes.FirstOrDefault(c => c.Id == id);

    // El legado guarda el RUT con puntos, espacios o «k» minúscula: se quitan y se pasa a mayúsculas.
    private static string NormalizarRut(string rut) =>
        rut.Replace(".", "").Replace(" ", "").ToUpper();

    private static string ObtenerZona(string region) => region switch
    {
        "RM" or "V" => "Zona centro",
        "VIII" => "Zona sur",
        "II" => "Zona norte",
        _ => "Sin zona",
    };
}
