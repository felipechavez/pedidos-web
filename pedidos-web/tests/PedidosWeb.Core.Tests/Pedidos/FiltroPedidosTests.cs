using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

public class FiltroPedidosTests
{
    private static readonly DateOnly Desde = new(2026, 3, 1);
    private static readonly DateOnly Hasta = new(2026, 3, 31);

    private static Pedido CrearPedido(int id, DateOnly fecha) =>
        new(id, $"PED-STGO-{id:000000}", "C-001", "Ferretería El Clavo", fecha, "Pendiente", 10000m, "STGO");

    // CA-01
    [Fact]
    public void EntreFechas_PedidoConFechaIgualADesde_LoIncluye()
    {
        var enDesde = CrearPedido(1, Desde);
        var antes = CrearPedido(2, Desde.AddDays(-1));

        var resultado = FiltroPedidos.EntreFechas([enDesde, antes], Desde, Hasta);

        Assert.Equal([enDesde], resultado);
    }

    // CA-02
    [Fact]
    public void EntreFechas_PedidoConFechaIgualAHasta_LoIncluye()
    {
        var enHasta = CrearPedido(1, Hasta);
        var despues = CrearPedido(2, Hasta.AddDays(1));

        var resultado = FiltroPedidos.EntreFechas([enHasta, despues], Desde, Hasta);

        Assert.Equal([enHasta], resultado);
    }

    // CA-03
    [Fact]
    public void EntreFechas_PedidosFueraDelRango_NoLosIncluye()
    {
        var antes = CrearPedido(1, Desde.AddDays(-1));
        var dentro = CrearPedido(2, new DateOnly(2026, 3, 15));
        var despues = CrearPedido(3, Hasta.AddDays(1));

        var resultado = FiltroPedidos.EntreFechas([antes, dentro, despues], Desde, Hasta);

        Assert.Equal([dentro], resultado);
    }

    // CA-04
    [Fact]
    public void EntreFechas_DesdeMayorQueHasta_LanzaArgumentExceptionConMensaje()
    {
        var pedidos = new[] { CrearPedido(1, Desde) };

        var ex = Assert.Throws<ArgumentException>(() => FiltroPedidos.EntreFechas(pedidos, Hasta, Desde));

        Assert.Contains("'desde' (2026-03-31) no puede ser mayor que la fecha 'hasta' (2026-03-01)", ex.Message);
        Assert.Equal("desde", ex.ParamName);
    }

    // Contrato: lista nula se rechaza
    [Fact]
    public void EntreFechas_PedidosNulo_LanzaArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => FiltroPedidos.EntreFechas(null!, Desde, Hasta));

        Assert.Equal("pedidos", ex.ParamName);
    }

    // Límite: lista vacía
    [Fact]
    public void EntreFechas_ListaVacia_DevuelveListaVacia()
    {
        var resultado = FiltroPedidos.EntreFechas([], Desde, Hasta);

        Assert.Empty(resultado);
    }

    // Límite: rango de un solo día (desde == hasta es válido)
    [Fact]
    public void EntreFechas_DesdeIgualAHasta_IncluyeSoloPedidosDeEseDia()
    {
        var dia = new DateOnly(2026, 3, 15);
        var diaAnterior = CrearPedido(1, dia.AddDays(-1));
        var delDia = CrearPedido(2, dia);
        var diaSiguiente = CrearPedido(3, dia.AddDays(1));

        var resultado = FiltroPedidos.EntreFechas([diaAnterior, delDia, diaSiguiente], dia, dia);

        Assert.Equal([delDia], resultado);
    }
}
