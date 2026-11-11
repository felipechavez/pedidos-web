namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Parámetros de consulta para listar pedidos con filtros, ordenamiento y paginación.
/// </summary>
public sealed record ConsultaPedidos
{
    public string? Cliente { get; init; }
    public string? Estado { get; init; }
    public DateOnly? FechaDesde { get; init; }
    public DateOnly? FechaHasta { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamañoPagina { get; init; } = 10;
    public string OrdenarPor { get; init; } = "fecha";
    public bool Ascendente { get; init; } = false;

    public static readonly IReadOnlySet<string> EstadosValidos =
        new HashSet<string> { "Pendiente", "Confirmado", "Despachado", "Entregado", "Anulado" };

    public static readonly IReadOnlySet<int> TamañosPermitidos =
        new HashSet<int> { 10, 20, 30, 100 };

    public static readonly IReadOnlySet<string> CamposDeOrden =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id", "folio", "clienteId", "clienteNombre", "fecha", "estado", "total", "sucursal"
        };
}
