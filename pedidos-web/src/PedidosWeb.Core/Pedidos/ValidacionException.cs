namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Excepción lanzada cuando una consulta de pedidos no pasa las validaciones de negocio.
/// </summary>
public sealed class ValidacionException : Exception
{
    public string Detalle { get; }

    public ValidacionException(string detalle) : base(detalle)
    {
        Detalle = detalle;
    }
}
