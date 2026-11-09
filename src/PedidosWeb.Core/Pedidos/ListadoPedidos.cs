using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PedidosWeb.Core.Folios;

namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Lista pedidos con filtros, orden y paginación (HU-01) y búsqueda por número o folio (HU-02).
/// Reglas en specs/HU-01-listar-pedidos/spec.md (RN1 a RN9) y specs/HU-02-buscar-por-numero/spec.md.
/// </summary>
public sealed class ListadoPedidos
{
    public const int PaginaPorDefecto = 1;
    public const int TamanoPaginaPorDefecto = 20;
    public const int TamanoPaginaMaximo = 100;

    private const int NumeroMinimo = 1;
    private const int NumeroMaximo = 999999;

    private static readonly string[] FormatosFecha = ["yyyy-MM-dd", "dd-MM-yyyy"];

    // HU-02 RN1: solo dígitos ASCII. [0-9] y no \d, que acepta dígitos de otros alfabetos.
    private static readonly Regex SoloDigitos = new("^[0-9]+$", RegexOptions.CultureInvariant);

    // HU-02 RN1: PED-{SUCURSAL}-{NUMERO}; sucursal de 2 a 4 letras A–Z (sin Ñ), número con o sin ceros.
    private static readonly Regex FormatoFolio = new("^PED-([A-Z]{2,4})-([0-9]+)$", RegexOptions.CultureInvariant);

    private readonly IPedidoRepositorio _repositorio;

    public ListadoPedidos(IPedidoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Valida la consulta y, si es válida, devuelve la página de pedidos que cumplen todos los filtros.
    /// </summary>
    public ResultadoListado Listar(ConsultaPedidos consulta)
    {
        var errores = new Dictionary<string, string[]>();

        // RN9: vacío o solo espacios equivale a no enviado.
        var clienteId = Limpiar(consulta.ClienteId);
        var cliente = Limpiar(consulta.Cliente);
        var estado = Limpiar(consulta.Estado);
        var textoDesde = Limpiar(consulta.Desde);
        var textoHasta = Limpiar(consulta.Hasta);
        var textoPagina = Limpiar(consulta.Pagina);
        var textoTamano = Limpiar(consulta.TamanoPagina);
        var textoNumero = Limpiar(consulta.Numero);

        // HU-02 RN1, RN2 y RN5: número o folio válido; si no, error 400 en «numero».
        var (idBuscado, folioBuscado) = LeerNumero(textoNumero, errores);

        // RN5: solo los cinco estados válidos, sin distinguir mayúsculas.
        if (estado is not null && !EstadosPedido.EsValido(estado))
        {
            errores["estado"] = [$"El estado debe ser {string.Join(", ", EstadosPedido.Validos)}."];
        }

        // RN6: desde y hasta van juntos, con formato yyyy-MM-dd o dd-MM-yyyy, y desde <= hasta.
        var desde = LeerFecha(textoDesde, "desde", errores);
        var hasta = LeerFecha(textoHasta, "hasta", errores);
        if (textoDesde is not null && textoHasta is null)
        {
            errores["hasta"] = ["Si se envía «desde», también se debe enviar «hasta»."];
        }
        else if (textoDesde is null && textoHasta is not null)
        {
            errores["desde"] = ["Si se envía «hasta», también se debe enviar «desde»."];
        }
        else if (desde is not null && hasta is not null && desde > hasta)
        {
            errores["desde"] = ["«desde» no puede ser posterior a «hasta»."];
        }

        // RN8: pagina >= 1 y tamanoPagina entre 1 y 100.
        var pagina = LeerEntero(textoPagina, PaginaPorDefecto, 1, int.MaxValue, "pagina",
            "«pagina» debe ser un número entero mayor o igual a 1.", errores);
        var tamano = LeerEntero(textoTamano, TamanoPaginaPorDefecto, 1, TamanoPaginaMaximo, "tamanoPagina",
            $"«tamanoPagina» debe ser un número entero entre 1 y {TamanoPaginaMaximo}.", errores);

        if (errores.Count > 0)
        {
            return ResultadoListado.ConErrores(errores);
        }

        // RN1 y RN2: filtros opcionales, combinados con Y.
        IEnumerable<Pedido> pedidos = _repositorio.ObtenerTodos();

        // HU-02 RN3, RN4 y RN6: por id o por folio completo; si no existe, lista vacía sin error.
        if (idBuscado is not null)
        {
            pedidos = pedidos.Where(p => p.Id == idBuscado);
        }
        else if (folioBuscado is not null)
        {
            pedidos = pedidos.Where(p => p.Folio == folioBuscado);
        }

        if (clienteId is not null)
        {
            // RN3: código exacto.
            pedidos = pedidos.Where(p => p.ClienteId == clienteId);
        }

        if (cliente is not null)
        {
            // RN4: el nombre contiene el texto, sin distinguir mayúsculas ni tildes.
            var buscado = QuitarTildes(cliente);
            pedidos = pedidos.Where(p =>
                QuitarTildes(p.ClienteNombre).Contains(buscado, StringComparison.OrdinalIgnoreCase));
        }

        if (estado is not null)
        {
            pedidos = pedidos.Where(p => string.Equals(p.Estado, estado, StringComparison.OrdinalIgnoreCase));
        }

        if (desde is not null && hasta is not null)
        {
            // RN6: límites inclusivos.
            pedidos = pedidos.Where(p => p.Fecha >= desde && p.Fecha <= hasta);
        }

        // RN7: fecha descendente y, si empatan, id descendente. Se ordena antes de paginar.
        var filtrados = pedidos
            .OrderByDescending(p => p.Fecha)
            .ThenByDescending(p => p.Id)
            .ToList();

        // RN8: una página más allá de la última devuelve items vacío. El salto se calcula en long para no desbordar.
        var salto = (long)(pagina - 1) * tamano;
        var items = salto >= filtrados.Count
            ? []
            : filtrados.Skip((int)salto).Take(tamano).ToList();

        return ResultadoListado.Exito(new PaginaPedidos(items, filtrados.Count, pagina, tamano));
    }

    // HU-02: devuelve el id buscado o el folio normalizado al formato del legado (ej.: "ped-ant-9" → "PED-ANT-000009").
    // El texto ya viene sin espacios en los extremos y nunca vacío (RN9 de HU-01).
    private static (int? Id, string? Folio) LeerNumero(string? texto, Dictionary<string, string[]> errores)
    {
        if (texto is null)
        {
            return (null, null);
        }

        var mayusculas = texto.ToUpperInvariant();

        if (SoloDigitos.IsMatch(mayusculas) && EsNumeroValido(mayusculas, out var id))
        {
            return (id, null);
        }

        var folio = FormatoFolio.Match(mayusculas);
        if (folio.Success && EsNumeroValido(folio.Groups[2].Value, out var numero))
        {
            // Las reglas del folio ya se validaron arriba: FormatearFolio no lanza aquí.
            return (null, FolioFormatter.FormatearFolio(numero, folio.Groups[1].Value));
        }

        errores["numero"] =
            [$"«numero» debe ser un número entre {NumeroMinimo} y {NumeroMaximo} o un folio PED-{{SUCURSAL}}-{{NUMERO}}."];
        return (null, null);
    }

    // HU-02 RN1: solo dígitos, con valor entre 1 y 999999; se aceptan ceros a la izquierda.
    private static bool EsNumeroValido(string digitos, out int numero) =>
        int.TryParse(digitos, NumberStyles.None, CultureInfo.InvariantCulture, out numero)
        && numero is >= NumeroMinimo and <= NumeroMaximo;

    private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static DateOnly? LeerFecha(string? texto, string parametro, Dictionary<string, string[]> errores)
    {
        if (texto is null)
        {
            return null;
        }

        if (DateOnly.TryParseExact(texto, FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var fecha))
        {
            return fecha;
        }

        errores[parametro] = [$"«{parametro}» debe tener formato yyyy-MM-dd o dd-MM-yyyy."];
        return null;
    }

    private static int LeerEntero(string? texto, int porDefecto, int minimo, int maximo, string parametro,
        string mensaje, Dictionary<string, string[]> errores)
    {
        if (texto is null)
        {
            return porDefecto;
        }

        if (int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var valor)
            && valor >= minimo && valor <= maximo)
        {
            return valor;
        }

        errores[parametro] = [mensaje];
        return porDefecto;
    }

    // Quita las marcas diacríticas: «í» queda «i» y «ñ» queda «n» (RN4).
    private static string QuitarTildes(string texto)
    {
        var descompuesto = texto.Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(c);
            }
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }
}
