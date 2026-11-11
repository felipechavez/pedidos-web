namespace PedidosWeb.Core.Totales;

/// <summary>Línea que entra al cálculo de totales. Montos en pesos chilenos (CLP).</summary>
/// <param name="Cantidad">Unidades pedidas.</param>
/// <param name="PrecioUnitario">Precio unitario en CLP.</param>
/// <param name="Categoria">Código de categoría del producto (por ejemplo, <c>OFE</c>, <c>BEB</c>).</param>
public sealed record LineaCalculo(int Cantidad, decimal PrecioUnitario, string Categoria);

/// <summary>Resultado del cálculo de totales de un pedido. Montos en CLP.</summary>
/// <param name="PorcentajeDescuento">Porcentaje como fracción (0.05 = 5 %).</param>
public sealed record ResultadoTotales(
    decimal Subtotal,
    decimal BaseDescuento,
    decimal PorcentajeDescuento,
    decimal Descuento,
    decimal Neto,
    decimal Flete,
    decimal Iva,
    decimal Total);

/// <summary>
/// Cálculo de totales del pedido migrado desde <c>legado/src/UCalculoTotales.pas</c>
/// (<c>CalcularTotales</c>). El contrato es <c>migracion/golden-master.csv</c>.
/// </summary>
public static class CalculoTotales
{
    /// <summary>Categoría que no entra a la base del descuento.</summary>
    private const string CategoriaOferta = "OFE";

    /// <summary>Calcula subtotal, descuento, flete, IVA y total del pedido.</summary>
    /// <param name="lineas">Líneas del pedido.</param>
    /// <param name="esVip">Si el cliente es VIP (tipo <c>V</c>).</param>
    /// <param name="region">Código de región del cliente.</param>
    /// <param name="fleteBase">Flete base de la región, en CLP.</param>
    /// <param name="tasaIva">Tasa de IVA como fracción (0.19).</param>
    public static ResultadoTotales Calcular(
        IReadOnlyList<LineaCalculo> lineas,
        bool esVip,
        int region,
        decimal fleteBase,
        decimal tasaIva)
    {
        var subtotal = CalcularSubtotal(lineas);
        var baseDescuento = CalcularBaseDescuento(lineas);
        var porcentajeDescuento = PorcentajeDescuento(baseDescuento, esVip);
        var descuento = RedondearAlPeso(baseDescuento * porcentajeDescuento);
        var neto = subtotal - descuento;
        // Sin líneas no se cobra flete (UCalculoTotales.pas, CalcularTotales).
        var flete = lineas.Count == 0 ? 0m : CalcularFlete(fleteBase, region);
        var iva = CalcularIva(neto, flete, tasaIva);
        var total = neto + flete + iva;

        return new ResultadoTotales(subtotal, baseDescuento, porcentajeDescuento, descuento, neto, flete, iva, total);
    }

    /// <summary>
    /// Redondeo al peso comercial: una fracción de ,50 o más sube al peso siguiente.
    /// Corrige R05: el legado (<c>Calc3</c>, <c>Trunc(valor + 0.49)</c>) bajaba la fracción ,50.
    /// Ver <c>docs/migracion/defectos-corregidos.md</c> (D1).
    /// </summary>
    /// <remarks>Los montos del cálculo nunca son negativos, así que <c>AwayFromZero</c> equivale a «mitad hacia arriba».</remarks>
    internal static decimal RedondearAlPeso(decimal valor) => Math.Round(valor, 0, MidpointRounding.AwayFromZero);

    /// <summary>Suma de cantidad × precio de todas las líneas (sin redondeo, igual que el legado).</summary>
    internal static decimal CalcularSubtotal(IReadOnlyList<LineaCalculo> lineas) =>
        lineas.Sum(l => l.Cantidad * l.PrecioUnitario);

    /// <summary>Suma de las líneas que entran al descuento: todas menos la categoría OFE.</summary>
    internal static decimal CalcularBaseDescuento(IReadOnlyList<LineaCalculo> lineas) =>
        lineas
            .Where(l => !string.Equals(l.Categoria.Trim(), CategoriaOferta, StringComparison.OrdinalIgnoreCase))
            .Sum(l => l.Cantidad * l.PrecioUnitario);

    /// <summary>Descuento por volumen: más de 1.000.000 → 8 %, más de 500.000 → 5 %, si no 0 %.</summary>
    internal static decimal PorcentajeVolumen(decimal baseDescuento) =>
        baseDescuento > 1_000_000m ? 0.08m
        : baseDescuento > 500_000m ? 0.05m
        : 0m;

    /// <summary>Volumen + 3 % si el cliente es VIP, con tope de 10 %.</summary>
    internal static decimal PorcentajeDescuento(decimal baseDescuento, bool esVip)
    {
        var porcentaje = PorcentajeVolumen(baseDescuento);
        if (esVip)
            porcentaje += 0.03m;
        return porcentaje > 0.10m ? 0.10m : porcentaje;
    }

    /// <summary>Flete de la región; las regiones 11 y 12 tienen un recargo de 15 %.</summary>
    internal static decimal CalcularFlete(decimal fleteBase, int region) =>
        region is 11 or 12
            ? RedondearAlPeso(fleteBase * 1.15m)
            : RedondearAlPeso(fleteBase);

    /// <summary>IVA sobre neto + flete, redondeado con la regla del legado.</summary>
    internal static decimal CalcularIva(decimal neto, decimal flete, decimal tasaIva) =>
        RedondearAlPeso((neto + flete) * tasaIva);
}
