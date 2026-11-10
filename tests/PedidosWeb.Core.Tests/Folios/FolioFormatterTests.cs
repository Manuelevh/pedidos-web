using PedidosWeb.Core.Folios;

namespace PedidosWeb.Core.Tests.Folios;

public class FolioFormatterTests
{
    // Casos base del README (tres primeras filas de la tabla de ejemplos).
    [Theory]
    [InlineData(123, "STGO", "PED-STGO-000123")]
    [InlineData(1, "VAL", "PED-VAL-000001")]
    [InlineData(999999, "CO", "PED-CO-999999")]
    public void FormatearFolio_CasosBase_DevuelveElFolioEsperado(int numero, string sucursal, string esperado)
    {
        var folio = FolioFormatter.FormatearFolio(numero, sucursal);

        Assert.Equal(esperado, folio);
    }

    // R1: numero fuera de 1..999999.
    [Theory]
    [InlineData(0)]
    [InlineData(1000000)]
    [InlineData(-5)]
    public void FormatearFolio_NumeroFueraDeRango_LanzaArgumentOutOfRange(int numero)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FolioFormatter.FormatearFolio(numero, "STGO"));
    }

    // R2: se quitan espacios de los extremos y se pasa a mayúsculas.
    [Theory]
    [InlineData(45, " stgo ", "PED-STGO-000045")]
    [InlineData(7, "val", "PED-VAL-000007")]
    public void FormatearFolio_SucursalConEspaciosOMinusculas_SeNormaliza(int numero, string sucursal, string esperado)
    {
        Assert.Equal(esperado, FolioFormatter.FormatearFolio(numero, sucursal));
    }

    // R3: después de R2, entre 2 y 4 letras de la A a la Z.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("S")]
    [InlineData("SANTI")]
    [InlineData("ÑUÑO")]
    [InlineData("ST1")]
    [InlineData("VÁL")]
    [InlineData("S O")]
    public void FormatearFolio_SucursalInvalida_LanzaArgumentException(string? sucursal)
    {
        // ThrowsAny acepta derivadas como ArgumentNullException, que R3 permite.
        var ex = Assert.ThrowsAny<ArgumentException>(() => FolioFormatter.FormatearFolio(7, sucursal!));
        Assert.IsNotType<ArgumentOutOfRangeException>(ex);
    }

    // R3: los extremos válidos del largo (2 y 4 letras).
    [Theory]
    [InlineData("AB", "PED-AB-000007")]
    [InlineData("ABCD", "PED-ABCD-000007")]
    public void FormatearFolio_SucursalEnLimitesDeLargo_EsValida(string sucursal, string esperado)
    {
        Assert.Equal(esperado, FolioFormatter.FormatearFolio(7, sucursal));
    }

    // R4: relleno con ceros hasta 6 dígitos.
    [Theory]
    [InlineData(5, "PED-VAL-000005")]
    [InlineData(12345, "PED-VAL-012345")]
    public void FormatearFolio_Numero_SeRellenaConCerosASeisDigitos(int numero, string esperado)
    {
        Assert.Equal(esperado, FolioFormatter.FormatearFolio(numero, "VAL"));
    }

    // R5: si ambos son inválidos, se valida primero numero.
    [Theory]
    [InlineData(0, "S")]
    [InlineData(-1, null)]
    [InlineData(1000000, "ÑUÑO")]
    public void FormatearFolio_AmbosInvalidos_ValidaPrimeroNumero(int numero, string? sucursal)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FolioFormatter.FormatearFolio(numero, sucursal!));
    }
}
