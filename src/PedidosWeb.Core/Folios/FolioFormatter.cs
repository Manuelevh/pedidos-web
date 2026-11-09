namespace PedidosWeb.Core.Folios;

/// <summary>
/// Genera el folio de un pedido con el mismo formato del sistema legado.
/// Las reglas completas y los ejemplos están en el README, sección «La función del taller».
/// </summary>
public static class FolioFormatter
{
    private const int NumeroMinimo = 1;
    private const int NumeroMaximo = 999999;

    /// <summary>
    /// Devuelve el folio con el formato PED-{SUCURSAL}-{NUMERO}, por ejemplo "PED-STGO-000123".
    /// </summary>
    /// <param name="numero">Número correlativo del pedido, entre 1 y 999999.</param>
    /// <param name="sucursal">Código de la sucursal: de 2 a 4 letras de la A a la Z.</param>
    /// <exception cref="ArgumentOutOfRangeException">Si <paramref name="numero"/> no está entre 1 y 999999 (R1).</exception>
    /// <exception cref="ArgumentException">Si <paramref name="sucursal"/> es nula, vacía o no tiene de 2 a 4 letras A–Z (R3).</exception>
    public static string FormatearFolio(int numero, string sucursal)
    {
        // R5: el número se valida antes que la sucursal.
        // R1: número entre 1 y 999999, ambos incluidos.
        if (numero < NumeroMinimo || numero > NumeroMaximo)
        {
            throw new ArgumentOutOfRangeException(nameof(numero), numero,
                $"El número debe estar entre {NumeroMinimo} y {NumeroMaximo}.");
        }

        // R2: sin espacios al inicio ni al final, y en mayúsculas.
        var codigo = sucursal?.Trim().ToUpperInvariant();

        // R3: de 2 a 4 letras ASCII de la A a la Z (el legado guarda el código en un campo ASCII de 4).
        if (string.IsNullOrEmpty(codigo)
            || codigo.Length is < 2 or > 4
            || !codigo.All(c => c is >= 'A' and <= 'Z'))
        {
            throw new ArgumentException(
                "La sucursal debe tener entre 2 y 4 letras de la A a la Z.", nameof(sucursal));
        }

        // R4: número con ceros a la izquierda hasta 6 dígitos.
        return $"PED-{codigo}-{numero:D6}";
    }
}
