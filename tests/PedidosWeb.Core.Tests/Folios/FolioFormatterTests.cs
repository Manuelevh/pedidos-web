using PedidosWeb.Core.Folios;

namespace PedidosWeb.Core.Tests.Folios;

/// <summary>
/// Cubre cada fila de la tabla de ejemplos del README (reglas R1 a R5).
/// </summary>
public class FolioFormatterTests
{
    // Casos base del README (tres primeras filas) y R4 (ceros a la izquierda hasta 6 dígitos).
    [Theory]
    [InlineData(123, "STGO", "PED-STGO-000123")]
    [InlineData(1, "VAL", "PED-VAL-000001")]
    [InlineData(999999, "CO", "PED-CO-999999")]
    public void FormatearFolio_CasosBase_DevuelveElFolioEsperado(int numero, string sucursal, string esperado)
    {
        var folio = FolioFormatter.FormatearFolio(numero, sucursal);

        Assert.Equal(esperado, folio);
    }

    // R2: se quitan los espacios de los extremos y se pasa a mayúsculas.
    [Fact]
    public void FormatearFolio_SucursalConEspaciosYMinusculas_LaNormaliza()
    {
        var folio = FolioFormatter.FormatearFolio(45, " stgo ");

        Assert.Equal("PED-STGO-000045", folio);
    }

    // R1: número fuera de 1..999999.
    [Theory]
    [InlineData(0)]
    [InlineData(1000000)]
    [InlineData(-5)]
    public void FormatearFolio_NumeroFueraDeRango_LanzaArgumentOutOfRange(int numero)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FolioFormatter.FormatearFolio(numero, "STGO"));
    }

    // R3: sucursal nula, vacía o solo espacios.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FormatearFolio_SucursalVacia_LanzaArgumentException(string? sucursal)
    {
        Assert.ThrowsAny<ArgumentException>(() => FolioFormatter.FormatearFolio(7, sucursal!));
    }

    // R3: 1 letra, 5 letras, Ñ y números.
    [Theory]
    [InlineData("S")]
    [InlineData("SANTI")]
    [InlineData("ÑUÑO")]
    [InlineData("ST1")]
    public void FormatearFolio_SucursalInvalida_LanzaArgumentException(string sucursal)
    {
        Assert.ThrowsAny<ArgumentException>(() => FolioFormatter.FormatearFolio(7, sucursal));
    }

    // R5: si ambos son inválidos, se valida primero el número.
    [Fact]
    public void FormatearFolio_NumeroYSucursalInvalidos_ValidaPrimeroElNumero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FolioFormatter.FormatearFolio(0, "S"));
    }
}
