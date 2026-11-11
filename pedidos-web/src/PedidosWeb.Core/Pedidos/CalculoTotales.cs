using PedidosWeb.Core.Lineas;

namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Cálculo de subtotales y totales de un pedido a partir de sus líneas.
/// Montos en pesos chilenos (CLP), sin IVA, descuentos ni recargos (eso llega en la Unidad 5).
/// </summary>
public static class CalculoTotales
{
    /// <summary>Subtotal de una línea: cantidad × precio unitario.</summary>
    public static decimal CalcularSubtotal(LineaPedido linea) =>
        linea.Cantidad * linea.PrecioUnitario;

    /// <summary>Total del pedido: suma de los subtotales de sus líneas. Sin líneas, el total es 0.</summary>
    public static decimal CalcularTotal(IEnumerable<LineaPedido> lineas) =>
        lineas.Sum(CalcularSubtotal);
}
