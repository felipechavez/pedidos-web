using PedidosWeb.Core.Lineas;
using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

public class CalculoTotalesTests
{
    [Fact]
    public void CalcularSubtotal_CantidadPorPrecio_DevuelveProducto()
    {
        var linea = new LineaPedido(8, 1, "Harina para pan 25 kg", 3, 21500m);

        var subtotal = CalculoTotales.CalcularSubtotal(linea);

        Assert.Equal(64500m, subtotal);
    }

    // CA-01
    [Fact]
    public void CalcularTotal_LineasDelPedido8_Devuelve120100IgualASumaDeSubtotales()
    {
        var lineas = new LineaPedidoRepositorioEnMemoria().ObtenerPorPedido(8);

        var total = CalculoTotales.CalcularTotal(lineas);

        Assert.Equal(3, lineas.Count);
        Assert.Equal(120100m, total);
        Assert.Equal(lineas.Sum(CalculoTotales.CalcularSubtotal), total);
    }

    [Fact]
    public void CalcularTotal_SinLineas_DevuelveCero()
    {
        var total = CalculoTotales.CalcularTotal(Array.Empty<LineaPedido>());

        Assert.Equal(0m, total);
    }
}
