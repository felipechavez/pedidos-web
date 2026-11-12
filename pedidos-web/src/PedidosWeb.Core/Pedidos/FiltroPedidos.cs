namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Filtros de dominio sobre listas de pedidos (HU-03).
/// </summary>
public static class FiltroPedidos
{
    /// <summary>
    /// Devuelve solo los pedidos cuya <see cref="Pedido.Fecha"/> está entre <paramref name="desde"/>
    /// y <paramref name="hasta"/>, ambos límites inclusive.
    /// </summary>
    /// <exception cref="ArgumentNullException">Si <paramref name="pedidos"/> es nulo.</exception>
    /// <exception cref="ArgumentException">Si <paramref name="desde"/> es mayor que <paramref name="hasta"/>.</exception>
    public static IReadOnlyList<Pedido> EntreFechas(IEnumerable<Pedido> pedidos, DateOnly desde, DateOnly hasta)
    {
        ArgumentNullException.ThrowIfNull(pedidos);

        if (desde > hasta)
            throw new ArgumentException(
                $"La fecha 'desde' ({desde:yyyy-MM-dd}) no puede ser mayor que la fecha 'hasta' ({hasta:yyyy-MM-dd}).",
                nameof(desde));

        return pedidos.Where(p => p.Fecha >= desde && p.Fecha <= hasta).ToList();
    }
}
