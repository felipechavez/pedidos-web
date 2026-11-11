using PedidosWeb.Core.Lineas;

namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Arma el detalle de un pedido juntando sus datos con sus líneas (HU-02).
/// </summary>
public sealed class DetallePedidoServicio
{
    private readonly IPedidoRepositorio _pedidos;
    private readonly ILineaPedidoRepositorio _lineas;

    public DetallePedidoServicio(IPedidoRepositorio pedidos, ILineaPedidoRepositorio lineas)
    {
        _pedidos = pedidos;
        _lineas = lineas;
    }

    /// <summary>
    /// Detalle del pedido, con el total calculado desde sus líneas (no se usa <c>Pedido.Total</c>).
    /// Devuelve <c>null</c> si el pedido no existe. Un pedido sin líneas trae lista vacía y total 0.
    /// </summary>
    public DetallePedido? ObtenerDetalle(int id)
    {
        if (_pedidos.ObtenerPorId(id) is not { } pedido)
            return null;

        var lineasPedido = _lineas.ObtenerPorPedido(id);

        var lineas = lineasPedido
            .Select(l => new LineaDetalle(
                l.Numero,
                l.Producto,
                l.Cantidad,
                l.PrecioUnitario,
                CalculoTotales.CalcularSubtotal(l)))
            .ToList();

        return new DetallePedido(
            pedido.Id,
            pedido.Folio,
            pedido.Fecha,
            pedido.Estado,
            pedido.Sucursal,
            pedido.ClienteId,
            pedido.ClienteNombre,
            lineas,
            CalculoTotales.CalcularTotal(lineasPedido));
    }
}
