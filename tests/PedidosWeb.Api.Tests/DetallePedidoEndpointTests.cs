using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PedidosWeb.Api.Tests;

// Criterios HTTP de specs/HU-03-detalle-pedido/spec.md.
public class DetallePedidoEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _cliente;

    public DetallePedidoEndpointTests(WebApplicationFactory<Program> fabrica)
    {
        _cliente = fabrica.CreateClient();
    }

    [Fact]
    public async Task Detalle_PedidoExistente_Responde200ConPedidoLineasYTotal() // CA-01 y CA-02
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/8/detalle");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var raiz = json.RootElement;
        Assert.Equal("PED-STGO-000008", raiz.GetProperty("folio").GetString());
        Assert.Equal("C-001", raiz.GetProperty("clienteId").GetString());
        Assert.Equal("Ferretería El Clavo", raiz.GetProperty("clienteNombre").GetString());
        Assert.Equal("Entregado", raiz.GetProperty("estado").GetString());
        Assert.Equal(
            new[] { 1, 2, 3 },
            raiz.GetProperty("lineas").EnumerateArray().Select(l => l.GetProperty("numero").GetInt32()));
        Assert.Equal(120100m, raiz.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task Detalle_PedidoInexistente_Responde404ConMensajeParaElVendedor() // CA-03
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/999/detalle");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal(
            "No encontramos el pedido 999. Revisa el número e intenta de nuevo.",
            json.RootElement.GetProperty("mensaje").GetString());
    }

    [Fact]
    public async Task RutaExistente_PedidoExistente_SigueRespondiendoElPedido() // CA-06
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/8");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal("PED-STGO-000008", json.RootElement.GetProperty("folio").GetString());
        Assert.Equal(120100m, json.RootElement.GetProperty("total").GetDecimal());
        Assert.False(json.RootElement.TryGetProperty("lineas", out _));
    }

    [Fact]
    public async Task RutaExistente_PedidoInexistente_SigueRespondiendo404SinCuerpo() // CA-06
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("", await respuesta.Content.ReadAsStringAsync());
    }
}
