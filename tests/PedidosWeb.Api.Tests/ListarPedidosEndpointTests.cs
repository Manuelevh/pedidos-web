using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Api.Tests;

/// <summary>
/// HU-01: contrato HTTP de GET /api/pedidos y criterios de error CA20 a CA24 (400 con ProblemDetails).
/// </summary>
public class ListarPedidosEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _cliente;

    public ListarPedidosEndpointTests(WebApplicationFactory<Program> fabrica)
    {
        _cliente = fabrica.CreateClient();
    }

    private async Task AssertErrorDeValidacion(string url, string parametro)
    {
        var respuesta = await _cliente.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problema);
        Assert.True(problema.Errors.ContainsKey(parametro),
            $"Se esperaba un error para «{parametro}»; llegaron: {string.Join(", ", problema.Errors.Keys)}");
    }

    [Fact]
    public async Task Listar_Contrato_SinParametros_Devuelve200ConItemsYMetadatos()
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var pagina = await respuesta.Content.ReadFromJsonAsync<PaginaPedidos>();
        Assert.NotNull(pagina);
        Assert.Equal(45, pagina.Total);
        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(20, pagina.TamanoPagina);
        Assert.Equal(20, pagina.Items.Count);
        Assert.Equal(21, pagina.Items[0].Id);
    }

    [Theory]
    [InlineData("/api/pedidos?desde=2026-10-01", "hasta")]
    [InlineData("/api/pedidos?hasta=2026-10-31", "desde")]
    public async Task Listar_CA20_SoloUnaFecha_Devuelve400(string url, string parametro)
    {
        await AssertErrorDeValidacion(url, parametro);
    }

    [Fact]
    public async Task Listar_CA21_FechasInvertidas_Devuelve400()
    {
        await AssertErrorDeValidacion("/api/pedidos?desde=2026-10-31&hasta=2026-10-01", "desde");
    }

    [Fact]
    public async Task Listar_CA22_FormatoDeFechaNoAceptado_Devuelve400()
    {
        await AssertErrorDeValidacion("/api/pedidos?desde=2026/10/01&hasta=2026-10-31", "desde");
    }

    [Fact]
    public async Task Listar_CA23_EstadoNoValido_Devuelve400()
    {
        await AssertErrorDeValidacion("/api/pedidos?estado=Cancelado", "estado");
    }

    [Theory]
    [InlineData("/api/pedidos?pagina=0", "pagina")]
    [InlineData("/api/pedidos?tamanoPagina=0", "tamanoPagina")]
    [InlineData("/api/pedidos?tamanoPagina=101", "tamanoPagina")]
    public async Task Listar_CA24_PaginacionFueraDeRango_Devuelve400(string url, string parametro)
    {
        await AssertErrorDeValidacion(url, parametro);
    }

    // HU-02 CA11: «numero» que no es número ni folio válido (specs/HU-02-buscar-por-numero/spec.md).
    [Theory]
    [InlineData("ABC")]
    [InlineData("PED-X-1")]
    [InlineData("PED-ÑUÑO-1")]
    [InlineData("0")]
    [InlineData("1000000")]
    [InlineData("PED-ANT-0")]
    [InlineData("PED-ANT-1000000")]
    public async Task Listar_HU02_CA11_NumeroConFormatoInvalido_Devuelve400(string numero)
    {
        await AssertErrorDeValidacion($"/api/pedidos?numero={Uri.EscapeDataString(numero)}", "numero");
    }

    // HU-02: el parámetro nuevo llega del query string al filtro.
    [Fact]
    public async Task Listar_HU02_FolioEnLaUrl_Devuelve200ConEsePedido()
    {
        var pagina = await _cliente.GetFromJsonAsync<PaginaPedidos>("/api/pedidos?numero=PED-ANT-000009");

        Assert.NotNull(pagina);
        Assert.Equal(1, pagina.Total);
        Assert.Equal(9, pagina.Items[0].Id);
    }
}
