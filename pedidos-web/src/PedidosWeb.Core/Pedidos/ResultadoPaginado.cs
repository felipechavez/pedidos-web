namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Resultado paginado genérico con metadatos de paginación.
/// </summary>
public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Datos,
    int Pagina,
    int TamañoPagina,
    int TotalRegistros,
    int TotalPaginas);
