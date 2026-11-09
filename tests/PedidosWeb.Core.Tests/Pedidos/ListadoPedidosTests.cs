using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

/// <summary>
/// HU-01: un test por criterio de aceptación de lógica (CA1 a CA19 y CA25).
/// Usa los 45 pedidos de ejemplo de PedidoRepositorioEnMemoria.
/// Los criterios de error (CA20 a CA24) se prueban en PedidosWeb.Api.Tests.
/// </summary>
public class ListadoPedidosTests
{
    // Orden completo esperado (RN7), según la spec.
    private static readonly int[] OrdenCompleto =
    [
        21, 42, 2, 23, 44, 4, 25, 6, 27, 8, 29, 10, 31, 12, 33, 14, 35, 16, 37, 18,
        39, 20, 41, 1, 22, 43, 3, 24, 45, 5, 26, 7, 28, 9, 30, 11, 32, 13, 34, 15,
        36, 17, 38, 19, 40
    ];

    private readonly ListadoPedidos _listado = new(new PedidoRepositorioEnMemoria());

    private PaginaPedidos ListarValido(ConsultaPedidos consulta)
    {
        var resultado = _listado.Listar(consulta);

        Assert.True(resultado.EsValido, string.Join("; ", resultado.Errores.Keys));
        return resultado.Pagina!;
    }

    private static int[] Ids(PaginaPedidos pagina) => pagina.Items.Select(p => p.Id).ToArray();

    [Fact]
    public void Listar_CA1_SinParametros_DevuelvePrimeraPaginaDe20ConAnulados()
    {
        var pagina = ListarValido(new ConsultaPedidos());

        Assert.Equal(45, pagina.Total);
        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(20, pagina.TamanoPagina);
        Assert.Equal(OrdenCompleto.Take(20), Ids(pagina));
        Assert.Contains(2, Ids(pagina));
        Assert.Contains(29, Ids(pagina));
    }

    [Fact]
    public void Listar_CA2_ClienteIdC001_DevuelveSus5Pedidos()
    {
        var pagina = ListarValido(new ConsultaPedidos(ClienteId: "C-001"));

        Assert.Equal(5, pagina.Total);
        Assert.Equal([8, 16, 24, 32, 40], Ids(pagina));
    }

    [Fact]
    public void Listar_CA3_ClienteIdInexistente_DevuelveVacioSinError()
    {
        var pagina = ListarValido(new ConsultaPedidos(ClienteId: "C-999"));

        Assert.Equal(0, pagina.Total);
        Assert.Empty(pagina.Items);
    }

    [Fact]
    public void Listar_CA4_NombreClavo_DevuelveLosPedidosDeC001()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "Clavo"));

        Assert.Equal([8, 16, 24, 32, 40], Ids(pagina));
    }

    [Fact]
    public void Listar_CA5_NombreSinTildeNiMayusculas_EncuentraBotilleria()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "botilleria"));

        Assert.Equal(6, pagina.Total);
        Assert.Equal([42, 2, 10, 18, 26, 34], Ids(pagina));
    }

    [Fact]
    public void Listar_CA6_NombreConNEnVezDeEnie_EncuentraDonaRosa()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "DONA ROSA"));

        Assert.Equal(6, pagina.Total);
        Assert.All(pagina.Items, p => Assert.Equal("C-002", p.ClienteId));
    }

    [Fact]
    public void Listar_CA7_EstadoEntregado_Devuelve15Pedidos()
    {
        var pagina = ListarValido(new ConsultaPedidos(Estado: "Entregado"));

        Assert.Equal(15, pagina.Total);
        Assert.Equal([42, 44, 4, 6, 8, 31, 33, 35, 22, 24, 26, 13, 15, 17, 40], Ids(pagina));
    }

    [Fact]
    public void Listar_CA8_EstadoEnMinusculas_DevuelveLoMismoQueCA7()
    {
        var minusculas = ListarValido(new ConsultaPedidos(Estado: "entregado"));
        var exacto = ListarValido(new ConsultaPedidos(Estado: "Entregado"));

        Assert.Equal(Ids(exacto), Ids(minusculas));
    }

    [Fact]
    public void Listar_CA9_EstadoAnulado_Devuelve5Pedidos()
    {
        var pagina = ListarValido(new ConsultaPedidos(Estado: "Anulado"));

        Assert.Equal(5, pagina.Total);
        Assert.Equal([2, 29, 20, 11, 38], Ids(pagina));
    }

    [Theory]
    [InlineData("2026-10-01", "2026-10-31", 23)]
    [InlineData("2026-09-01", "2026-09-30", 22)]
    public void Listar_CA10_RangoDeUnMes_DevuelveLosPedidosDelMes(string desde, string hasta, int esperado)
    {
        var pagina = ListarValido(new ConsultaPedidos(Desde: desde, Hasta: hasta));

        Assert.Equal(esperado, pagina.Total);
    }

    [Fact]
    public void Listar_CA11_RangoDeUnDia_IncluyeLosLimites()
    {
        var pagina = ListarValido(new ConsultaPedidos(Desde: "2026-09-02", Hasta: "2026-09-02"));

        Assert.Equal([40], Ids(pagina));
    }

    [Fact]
    public void Listar_CA12_FormatoChileno_DevuelveLoMismoQueIso()
    {
        var chileno = ListarValido(new ConsultaPedidos(Desde: "01-10-2026", Hasta: "31-10-2026", TamanoPagina: "100"));
        var iso = ListarValido(new ConsultaPedidos(Desde: "2026-10-01", Hasta: "2026-10-31", TamanoPagina: "100"));

        Assert.Equal(23, chileno.Total);
        Assert.Equal(Ids(iso), Ids(chileno));
    }

    [Fact]
    public void Listar_CA13_ClienteYEstado_DevuelveLaInterseccion()
    {
        var pagina = ListarValido(new ConsultaPedidos(ClienteId: "C-001", Estado: "Entregado"));

        Assert.Equal([8, 24, 40], Ids(pagina));
    }

    [Fact]
    public void Listar_CA14_ClienteYRango_DevuelveLaInterseccion()
    {
        var pagina = ListarValido(new ConsultaPedidos(ClienteId: "C-001", Desde: "2026-10-01", Hasta: "2026-10-31"));

        Assert.Equal([8, 16], Ids(pagina));
    }

    [Fact]
    public void Listar_CA15_CodigoYNombreDeClientesDistintos_DevuelveVacio()
    {
        var pagina = ListarValido(new ConsultaPedidos(ClienteId: "C-001", Cliente: "Doña Rosa"));

        Assert.Equal(0, pagina.Total);
        Assert.Empty(pagina.Items);
    }

    [Fact]
    public void Listar_CA16_Pagina2_DevuelveLosSiguientes20()
    {
        var pagina = ListarValido(new ConsultaPedidos(Pagina: "2"));

        Assert.Equal([39, 20, 41, 1, 22, 43, 3, 24, 45, 5, 26, 7, 28, 9, 30, 11, 32, 13, 34, 15], Ids(pagina));
    }

    [Fact]
    public void Listar_CA17_Pagina3_DevuelveLosUltimos5()
    {
        var pagina = ListarValido(new ConsultaPedidos(Pagina: "3"));

        Assert.Equal(45, pagina.Total);
        Assert.Equal([36, 17, 38, 19, 40], Ids(pagina));
    }

    [Fact]
    public void Listar_CA18_PaginaMasAllaDeLaUltima_DevuelveVacioConTotalReal()
    {
        var pagina = ListarValido(new ConsultaPedidos(Pagina: "5"));

        Assert.Equal(45, pagina.Total);
        Assert.Equal(5, pagina.Pagina);
        Assert.Empty(pagina.Items);
    }

    [Fact]
    public void Listar_CA19_TamanoPagina100_DevuelveLos45EnOrden()
    {
        var pagina = ListarValido(new ConsultaPedidos(TamanoPagina: "100"));

        Assert.Equal(OrdenCompleto, Ids(pagina));
    }

    [Fact]
    public void Listar_CA25_ParametrosVacios_SeTratanComoNoEnviados()
    {
        var conVacios = ListarValido(new ConsultaPedidos(Estado: "", Cliente: " "));
        var sinParametros = ListarValido(new ConsultaPedidos());
        Assert.Equal(Ids(sinParametros), Ids(conVacios));
        Assert.Equal(45, conVacios.Total);

        // desde vacío deja solo hasta: es error (CA20).
        var soloHasta = _listado.Listar(new ConsultaPedidos(Desde: "", Hasta: "2026-10-31"));
        Assert.False(soloHasta.EsValido);
        Assert.True(soloHasta.Errores.ContainsKey("desde"));
    }

    // ---------------------------------------------------------------------------------------------
    // HU-02 · Buscar por número o folio (specs/HU-02-buscar-por-numero/spec.md).
    // El pedido 9 es PED-ANT-000009, cliente C-006, estado Pendiente.
    // CA11 (formatos inválidos → 400) está en PedidosWeb.Api.Tests.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Listar_HU02_CA1_Numero_DevuelveEsePedido()
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: "9"));

        Assert.Equal(1, pagina.Total);
        Assert.Equal([9], Ids(pagina));
    }

    [Fact]
    public void Listar_HU02_CA2_NumeroConCeros_DevuelveEsePedido()
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: "000009"));

        Assert.Equal([9], Ids(pagina));
    }

    [Fact]
    public void Listar_HU02_CA3_FolioCompleto_DevuelveEsePedido()
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: "PED-ANT-000009"));

        Assert.Equal(1, pagina.Total);
        Assert.Equal([9], Ids(pagina));
    }

    [Fact]
    public void Listar_HU02_CA4_FolioEnMinusculasConEspacios_DevuelveEsePedido()
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: " ped-ant-000009 "));

        Assert.Equal([9], Ids(pagina));
    }

    [Fact]
    public void Listar_HU02_CA5_FolioSinCeros_DevuelveEsePedido()
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: "PED-ANT-9"));

        Assert.Equal([9], Ids(pagina));
    }

    [Fact]
    public void Listar_HU02_CA6_FolioConSucursalQueNoCorresponde_DevuelveVacio()
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: "PED-STGO-000009"));

        Assert.Equal(0, pagina.Total);
        Assert.Empty(pagina.Items);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("PED-ANT-000999")]
    public void Listar_HU02_CA7_NumeroOFolioInexistente_DevuelveVacioSinError(string numero)
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: numero));

        Assert.Equal(0, pagina.Total);
        Assert.Empty(pagina.Items);
    }

    [Theory]
    [InlineData("Pendiente", 1)]
    [InlineData("Entregado", 0)]
    public void Listar_HU02_CA8_NumeroYEstado_SeCombinanConY(string estado, int esperado)
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: "9", Estado: estado));

        Assert.Equal(esperado, pagina.Total);
    }

    [Fact]
    public void Listar_HU02_CA9_NumeroYClienteDeOtroPedido_DevuelveVacio()
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: "9", ClienteId: "C-001"));

        Assert.Equal(0, pagina.Total);
    }

    [Fact]
    public void Listar_HU02_CA10_NumeroVacio_SeTrataComoNoEnviado()
    {
        var pagina = ListarValido(new ConsultaPedidos(Numero: ""));

        Assert.Equal(45, pagina.Total);
    }
}
