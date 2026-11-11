using PedidosWeb.Core.Lineas;
using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

public class DetallePedidoServicioTests
{
    private readonly DetallePedidoServicio _servicio =
        new(new PedidoRepositorioEnMemoria(), new LineaPedidoRepositorioEnMemoria());

    // CA-02
    [Fact]
    public void ObtenerDetalle_Pedido8_TraeFolioEstadoClienteY3LineasOrdenadas()
    {
        var detalle = _servicio.ObtenerDetalle(8);

        Assert.NotNull(detalle);
        Assert.Equal(8, detalle.Id);
        Assert.Equal("PED-STGO-000008", detalle.Folio);
        Assert.Equal("Entregado", detalle.Estado);
        Assert.Equal("C-001", detalle.ClienteId);
        Assert.Equal("Ferretería El Clavo", detalle.ClienteNombre);
        Assert.Equal(new[] { 1, 2, 3 }, detalle.Lineas.Select(l => l.Numero));
        Assert.All(detalle.Lineas, l => Assert.Equal(l.Cantidad * l.PrecioUnitario, l.Subtotal));
        Assert.Equal(120100m, detalle.Total);
    }

    // CA-03
    [Fact]
    public void ObtenerDetalle_IdInexistente_DevuelveNull()
    {
        var detalle = _servicio.ObtenerDetalle(999);

        Assert.Null(detalle);
    }

    [Fact]
    public void ObtenerDetalle_PedidoSinLineas_DevuelveListaVaciaYTotalCero()
    {
        var servicio = new DetallePedidoServicio(new PedidoRepositorioEnMemoria(), new LineasVacias());

        var detalle = servicio.ObtenerDetalle(8);

        Assert.NotNull(detalle);
        Assert.Empty(detalle.Lineas);
        Assert.Equal(0m, detalle.Total);
    }

    /// <summary>Repositorio de prueba: ningún pedido tiene líneas.</summary>
    private sealed class LineasVacias : ILineaPedidoRepositorio
    {
        public IReadOnlyList<LineaPedido> ObtenerPorPedido(int pedidoId) => Array.Empty<LineaPedido>();
    }
}
