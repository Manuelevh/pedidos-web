using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Api.Tests;

/// <summary>
/// Tests de integración de los endpoints que ya existen en Program.cs.
/// Levantan la API en memoria con WebApplicationFactory; no abren puertos.
/// </summary>
public class EndpointsExistentesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _cliente;

    public EndpointsExistentesTests(WebApplicationFactory<Program> fabrica)
    {
        _cliente = fabrica.CreateClient();
    }

    private sealed record RespuestaSalud(string Estado);

    [Fact]
    public async Task Salud_ApiArriba_Devuelve200ConEstadoOk()
    {
        var respuesta = await _cliente.GetAsync("/salud");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaSalud>();
        Assert.Equal("ok", cuerpo?.Estado);
    }

    [Fact]
    public async Task ObtenerPedido_IdExistente_Devuelve200ConElPedido()
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/8");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var pedido = await respuesta.Content.ReadFromJsonAsync<Pedido>();
        Assert.NotNull(pedido);
        Assert.Equal(8, pedido.Id);
        Assert.Equal("C-001", pedido.ClienteId);
    }

    [Fact]
    public async Task ObtenerPedido_IdInexistente_Devuelve404()
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
