using PedidosWeb.Core.Clientes;

namespace PedidosWeb.Core.Tests.Clientes;

public class ClientesServiceTests
{
    private readonly ClientesService _servicio = new();

    [Fact]
    public void ObtenerFicha_ClienteInexistente_DevuelveTextoVacio()
    {
        var ficha = _servicio.ObtenerFicha("C-999");

        Assert.Equal("", ficha);
    }

    [Fact]
    public void ObtenerFicha_ClienteActivo_DevuelveLaFicha()
    {
        var ficha = _servicio.ObtenerFicha("C-001");

        Assert.Equal("Ferretería El Clavo | 76123456-0 | Zona centro | Activo", ficha);
    }

    [Fact]
    public void ObtenerFicha_RutConPuntosYKMinuscula_LoNormaliza()
    {
        var ficha = _servicio.ObtenerFicha("C-004");

        Assert.Equal("Almacén La Esquina | 76990011-K | Zona centro | Activo", ficha);
    }

    [Fact]
    public void EstaActivo_ClienteActivoEInactivo_DevuelveSiEstaActivo()
    {
        Assert.True(_servicio.EstaActivo("C-001"));
        Assert.False(_servicio.EstaActivo("C-007"));
        Assert.False(_servicio.EstaActivo("C-999"));
    }

    // Tests de caracterización: fijan lo que el código hace hoy, antes de ordenarlo.

    [Theory]
    [InlineData("C-002", "Minimarket Doña Rosa | 77654321-7 | Zona norte | Activo")]  // región II
    [InlineData("C-003", "Botillería Los Andes | 78345612-5 | Zona sur | Activo")]    // región VIII
    [InlineData("C-008", "Distribuidora Norte | 76432198-7 | Zona centro | Activo")]  // región V y RUT con espacios
    [InlineData("C-007", "Kiosco Central | 77321654-1 | Zona sur | Inactivo")]        // cliente inactivo
    public void ObtenerFicha_ClientesDeEjemplo_DevuelveLaFichaComoHoy(string id, string fichaEsperada)
    {
        var ficha = _servicio.ObtenerFicha(id);

        Assert.Equal(fichaEsperada, ficha);
    }

    [Fact]
    public void ObtenerFicha_RegionDesconocida_DevuelveSinZona()
    {
        var servicio = new ClientesService(new List<Cliente>
        {
            new() { Id = "X-001", Nombre = "Cliente de prueba", Rut = "1.234.567-k", Region = "XV", Activo = true },
        });

        var ficha = servicio.ObtenerFicha("X-001");

        Assert.Equal("Cliente de prueba | 1234567-K | Sin zona | Activo", ficha);
    }
}
