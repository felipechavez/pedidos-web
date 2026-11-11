using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

public class PedidoServicioTests
{
    private readonly PedidoServicio _servicio = new(new PedidoRepositorioEnMemoria());

    // CA-01
    [Fact]
    public void ListarPedidos_SinFiltros_DevuelvePrimeros10De45PorFechaDesc()
    {
        var resultado = _servicio.ListarPedidos(new ConsultaPedidos());

        Assert.Equal(10, resultado.Datos.Count);
        Assert.Equal(45, resultado.TotalRegistros);
        Assert.Equal(5, resultado.TotalPaginas);
        Assert.Equal(1, resultado.Pagina);

        // Verificar que están ordenados por fecha descendente
        for (var i = 0; i < resultado.Datos.Count - 1; i++)
            Assert.True(resultado.Datos[i].Fecha >= resultado.Datos[i + 1].Fecha);
    }

    // CA-02
    [Fact]
    public void ListarPedidos_ClienteC001_Devuelve5PedidosPorClienteId()
    {
        var resultado = _servicio.ListarPedidos(new ConsultaPedidos { Cliente = "C-001" });

        Assert.Equal(5, resultado.TotalRegistros);
        var ids = resultado.Datos.Select(p => p.Id).OrderBy(id => id).ToList();
        Assert.Equal(new[] { 8, 16, 24, 32, 40 }, ids);
    }

    // CA-03
    [Fact]
    public void ListarPedidos_ClienteClavo_Devuelve5PorNombreParcialCaseInsensitive()
    {
        var resultado = _servicio.ListarPedidos(new ConsultaPedidos { Cliente = "clavo" });

        Assert.Equal(5, resultado.TotalRegistros);
        var ids = resultado.Datos.Select(p => p.Id).OrderBy(id => id).ToList();
        Assert.Equal(new[] { 8, 16, 24, 32, 40 }, ids);
    }

    // CA-04
    [Fact]
    public void ListarPedidos_EstadoPendiente_Devuelve5Pendientes()
    {
        var resultado = _servicio.ListarPedidos(new ConsultaPedidos { Estado = "Pendiente" });

        Assert.Equal(5, resultado.TotalRegistros);
        var ids = resultado.Datos.Select(p => p.Id).OrderBy(id => id).ToList();
        Assert.Equal(new[] { 9, 18, 27, 36, 45 }, ids);
        Assert.All(resultado.Datos, p => Assert.Equal("Pendiente", p.Estado));
    }

    // CA-05
    [Fact]
    public void ListarPedidos_RangoFechas_DevuelveSoloPedidosEnRango()
    {
        var desde = new DateOnly(2026, 10, 1);
        var hasta = new DateOnly(2026, 10, 15);

        var resultado = _servicio.ListarPedidos(new ConsultaPedidos
        {
            FechaDesde = desde,
            FechaHasta = hasta
        });

        Assert.All(resultado.Datos, p =>
        {
            Assert.True(p.Fecha >= desde);
            Assert.True(p.Fecha <= hasta);
        });
        Assert.True(resultado.TotalRegistros > 0);
    }

    // CA-06
    [Fact]
    public void ListarPedidos_ClienteC002YEstadoConfirmado_DevuelveSoloId5()
    {
        var resultado = _servicio.ListarPedidos(new ConsultaPedidos
        {
            Cliente = "C-002",
            Estado = "Confirmado"
        });

        Assert.Equal(1, resultado.TotalRegistros);
        Assert.Equal(5, resultado.Datos[0].Id);
    }

    // CA-07
    [Fact]
    public void ListarPedidos_Pagina2_DevuelveSegundaPaginaDe10()
    {
        var pagina1 = _servicio.ListarPedidos(new ConsultaPedidos { Pagina = 1 });
        var pagina2 = _servicio.ListarPedidos(new ConsultaPedidos { Pagina = 2 });

        Assert.Equal(10, pagina2.Datos.Count);
        Assert.Equal(2, pagina2.Pagina);
        Assert.Equal(45, pagina2.TotalRegistros);

        // No hay solapamiento entre páginas
        var idsPagina1 = pagina1.Datos.Select(p => p.Id).ToHashSet();
        Assert.All(pagina2.Datos, p => Assert.DoesNotContain(p.Id, idsPagina1));
    }

    // CA-08
    [Fact]
    public void ListarPedidos_EstadoInvalido_LanzaValidacionException()
    {
        var ex = Assert.Throws<ValidacionException>(() =>
            _servicio.ListarPedidos(new ConsultaPedidos { Estado = "Invalido" }));

        Assert.Contains("Invalido", ex.Detalle);
        Assert.Contains("Estados permitidos", ex.Detalle);
    }

    // CA-09
    [Fact]
    public void ListarPedidos_FiltrosSinResultados_DevuelveListaVaciaConTotal0()
    {
        var resultado = _servicio.ListarPedidos(new ConsultaPedidos
        {
            Cliente = "C-001",
            Estado = "Anulado"
        });

        Assert.Empty(resultado.Datos);
        Assert.Equal(0, resultado.TotalRegistros);
        Assert.Equal(0, resultado.TotalPaginas);
    }

    // CA-10
    [Fact]
    public void ListarPedidos_Pagina0_LanzaValidacionException()
    {
        var ex = Assert.Throws<ValidacionException>(() =>
            _servicio.ListarPedidos(new ConsultaPedidos { Pagina = 0 }));

        Assert.Contains("página", ex.Detalle, StringComparison.OrdinalIgnoreCase);
    }

    // CA-11
    [Fact]
    public void ListarPedidos_TamañoPagina50_LanzaValidacionException()
    {
        var ex = Assert.Throws<ValidacionException>(() =>
            _servicio.ListarPedidos(new ConsultaPedidos { TamañoPagina = 50 }));

        Assert.Contains("tamaño", ex.Detalle, StringComparison.OrdinalIgnoreCase);
    }

    // RN3 — Fechas invertidas
    [Fact]
    public void ListarPedidos_FechasDesdeMayorQueHasta_LanzaValidacionException()
    {
        var ex = Assert.Throws<ValidacionException>(() =>
            _servicio.ListarPedidos(new ConsultaPedidos
            {
                FechaDesde = new DateOnly(2026, 10, 15),
                FechaHasta = new DateOnly(2026, 10, 1)
            }));

        Assert.Contains("fecha", ex.Detalle, StringComparison.OrdinalIgnoreCase);
    }

    // RN7 — Ordenar por total
    [Fact]
    public void ListarPedidos_OrdenarPorTotal_DevuelvePedidosOrdenadosPorTotal()
    {
        var resultado = _servicio.ListarPedidos(new ConsultaPedidos
        {
            OrdenarPor = "total",
            Ascendente = true
        });

        for (var i = 0; i < resultado.Datos.Count - 1; i++)
            Assert.True(resultado.Datos[i].Total <= resultado.Datos[i + 1].Total);
    }

    // RN6+err — Página mayor que total de páginas
    [Fact]
    public void ListarPedidos_PaginaMayorQueTotalPaginas_LanzaValidacionException()
    {
        var ex = Assert.Throws<ValidacionException>(() =>
            _servicio.ListarPedidos(new ConsultaPedidos { Pagina = 99 }));

        Assert.Contains("excede", ex.Detalle, StringComparison.OrdinalIgnoreCase);
    }

    // RN7+err — Campo de orden inválido
    [Fact]
    public void ListarPedidos_OrdenarPorCampoInvalido_LanzaValidacionException()
    {
        var ex = Assert.Throws<ValidacionException>(() =>
            _servicio.ListarPedidos(new ConsultaPedidos { OrdenarPor = "xyz" }));

        Assert.Contains("xyz", ex.Detalle);
        Assert.Contains("Campos permitidos", ex.Detalle);
    }
}
