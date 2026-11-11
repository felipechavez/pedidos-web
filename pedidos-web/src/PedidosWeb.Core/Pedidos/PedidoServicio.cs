namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Servicio de dominio que filtra, ordena y pagina pedidos.
/// </summary>
public sealed class PedidoServicio
{
    private readonly IPedidoRepositorio _repositorio;

    public PedidoServicio(IPedidoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public ResultadoPaginado<Pedido> ListarPedidos(ConsultaPedidos consulta)
    {
        Validar(consulta);

        var pedidos = Filtrar(_repositorio.ObtenerTodos(), consulta);
        pedidos = Ordenar(pedidos, consulta.OrdenarPor, consulta.Ascendente);

        var totalRegistros = pedidos.Count;
        var totalPaginas = totalRegistros == 0
            ? 0
            : (int)Math.Ceiling((double)totalRegistros / consulta.TamañoPagina);

        if (totalPaginas > 0 && consulta.Pagina > totalPaginas)
            throw new ValidacionException(
                $"La página {consulta.Pagina} excede el total de páginas ({totalPaginas}).");

        var datos = pedidos
            .Skip((consulta.Pagina - 1) * consulta.TamañoPagina)
            .Take(consulta.TamañoPagina)
            .ToList();

        return new ResultadoPaginado<Pedido>(datos, consulta.Pagina, consulta.TamañoPagina,
            totalRegistros, totalPaginas);
    }

    private static void Validar(ConsultaPedidos consulta)
    {
        if (consulta.Pagina < 1)
            throw new ValidacionException("La página debe ser mayor o igual a 1.");

        if (!ConsultaPedidos.TamañosPermitidos.Contains(consulta.TamañoPagina))
            throw new ValidacionException(
                $"El tamaño de página {consulta.TamañoPagina} no es válido. " +
                $"Tamaños permitidos: {string.Join(", ", ConsultaPedidos.TamañosPermitidos.Order())}.");

        if (consulta.Estado is not null && !ConsultaPedidos.EstadosValidos.Contains(consulta.Estado))
            throw new ValidacionException(
                $"El estado '{consulta.Estado}' no es válido. " +
                $"Estados permitidos: {string.Join(", ", ConsultaPedidos.EstadosValidos)}.");

        if (consulta.FechaDesde.HasValue && consulta.FechaHasta.HasValue
            && consulta.FechaDesde.Value > consulta.FechaHasta.Value)
            throw new ValidacionException("La fecha desde no puede ser mayor que la fecha hasta.");

        if (!ConsultaPedidos.CamposDeOrden.Contains(consulta.OrdenarPor))
            throw new ValidacionException(
                $"El campo de orden '{consulta.OrdenarPor}' no es válido. " +
                $"Campos permitidos: {string.Join(", ", ConsultaPedidos.CamposDeOrden)}.");
    }

    private static List<Pedido> Filtrar(IReadOnlyList<Pedido> pedidos, ConsultaPedidos consulta)
    {
        IEnumerable<Pedido> resultado = pedidos;

        if (consulta.Cliente is { } cliente)
        {
            resultado = resultado.Where(p =>
                p.ClienteId.Equals(cliente, StringComparison.OrdinalIgnoreCase) ||
                p.ClienteNombre.Contains(cliente, StringComparison.OrdinalIgnoreCase));
        }

        if (consulta.Estado is { } estado)
        {
            resultado = resultado.Where(p => p.Estado == estado);
        }

        if (consulta.FechaDesde is { } desde)
        {
            resultado = resultado.Where(p => p.Fecha >= desde);
        }

        if (consulta.FechaHasta is { } hasta)
        {
            resultado = resultado.Where(p => p.Fecha <= hasta);
        }

        return resultado.ToList();
    }

    private static List<Pedido> Ordenar(List<Pedido> pedidos, string campo, bool ascendente)
    {
        Func<Pedido, object> selector = campo.ToLowerInvariant() switch
        {
            "id" => p => p.Id,
            "folio" => p => p.Folio,
            "clienteid" => p => p.ClienteId,
            "clientenombre" => p => p.ClienteNombre,
            "fecha" => p => p.Fecha,
            "estado" => p => p.Estado,
            "total" => p => p.Total,
            "sucursal" => p => p.Sucursal,
            _ => p => p.Fecha // nunca llega aquí por la validación previa
        };

        return ascendente
            ? pedidos.OrderBy(selector).ToList()
            : pedidos.OrderByDescending(selector).ToList();
    }
}
