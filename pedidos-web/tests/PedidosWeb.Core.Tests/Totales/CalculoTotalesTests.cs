using System.Globalization;
using PedidosWeb.Core.Totales;

namespace PedidosWeb.Core.Tests.Totales;

/// <summary>
/// Golden master de la migración del cálculo de totales: cada fila de
/// <c>migracion/golden-master.csv</c> es lo que calcula hoy el legado, salvo los defectos
/// corregidos que registra <c>docs/migracion/defectos-corregidos.md</c>.
/// Además, casos de borde que el golden master no cubre.
/// </summary>
public class CalculoTotalesTests
{
    private const string RutaGoldenMaster = "migracion/golden-master.csv";

    public static IEnumerable<object[]> CasosGoldenMaster()
    {
        var filas = File.ReadAllLines(BuscarGoldenMaster());
        var columnas = filas[0].Trim().Split(',');
        int Col(string nombre) => Array.IndexOf(columnas, nombre);

        foreach (var fila in filas.Skip(1).Where(f => !string.IsNullOrWhiteSpace(f)))
        {
            var c = fila.Trim().Split(',');
            decimal Monto(string nombre) => decimal.Parse(c[Col(nombre)], CultureInfo.InvariantCulture);

            yield return
            [
                int.Parse(c[Col("caso")], CultureInfo.InvariantCulture),
                c[Col("folio")],
                c[Col("tipo_cliente")],
                int.Parse(c[Col("region")], CultureInfo.InvariantCulture),
                Monto("flete_base"),
                Monto("tasa_iva"),
                c[Col("lineas")],
                Monto("subtotal"),
                Monto("base_descuento"),
                Monto("porc_descuento"),
                Monto("descuento"),
                Monto("neto"),
                Monto("flete"),
                Monto("iva"),
                Monto("total")
            ];
        }
    }

    // caso y folio no se usan en el cálculo: están para identificar cada caso en el explorador de tests.
    // Los montos van como decimal (serializable) para que xUnit muestre los 60 casos por separado;
    // con un ResultadoTotales como parámetro los agruparía en un solo test.
#pragma warning disable xUnit1026
    [Theory]
    [MemberData(nameof(CasosGoldenMaster))]
    public void Calcular_CasoGoldenMaster_IgualAlLegado(
        int caso, string folio, string tipoCliente, int region, decimal fleteBase, decimal tasaIva,
        string lineas, decimal subtotal, decimal baseDescuento, decimal porcDescuento, decimal descuento,
        decimal neto, decimal flete, decimal iva, decimal total)
#pragma warning restore xUnit1026
    {
        var esperado = new ResultadoTotales(subtotal, baseDescuento, porcDescuento, descuento, neto, flete, iva, total);

        var resultado = CalculoTotales.Calcular(LeerLineas(lineas), tipoCliente == "V", region, fleteBase, tasaIva);

        // Compara las 8 columnas calculadas del golden master (igualdad de record, por valor).
        Assert.Equal(esperado, resultado);
    }

    // R05 (D1 en defectos-corregidos.md): redondeo al peso comercial, ,50 sube.
    // Se prueba con el flete, que es el monto redondeado más directo de controlar.
    [Theory]
    [InlineData(13, "3500.49", "3500")]
    [InlineData(13, "3500.50", "3501")] // el legado daba 3500
    [InlineData(13, "3500.51", "3501")]
    [InlineData(12, "16490", "18964")]  // 16490 × 1,15 = 18963,50; el legado daba 18963
    public void Calcular_FleteConFraccion_RedondeaMitadHaciaArriba(int region, string fleteBase, string fleteEsperado)
    {
        var lineas = new List<LineaCalculo> { new(1, 10000m, "BEB") };

        var resultado = CalculoTotales.Calcular(
            lineas, esVip: false, region, decimal.Parse(fleteBase, CultureInfo.InvariantCulture), tasaIva: 0.19m);

        Assert.Equal(decimal.Parse(fleteEsperado, CultureInfo.InvariantCulture), resultado.Flete);
    }

    // Sin líneas no se cobra flete, ni siquiera en región 12 (UCalculoTotales.pas:116-117).
    // El golden master no tiene pedidos vacíos.
    [Fact]
    public void Calcular_SinLineas_FleteIvaYTotalEnCero()
    {
        var resultado = CalculoTotales.Calcular([], esVip: true, region: 12, fleteBase: 16490m, tasaIva: 0.19m);

        Assert.Equal(
            (0m, 0m, 0m, 0m),
            (resultado.Descuento, resultado.Flete, resultado.Iva, resultado.Total));
    }

    // La categoría se normaliza con Trim + mayúsculas (UCalculoTotales.pas:70).
    // Sin normalizar, la base sería 610.000 y se aplicaría 5 %.
    [Theory]
    [InlineData("ofe")]
    [InlineData(" OFE ")]
    [InlineData("Ofe")]
    public void Calcular_CategoriaOfeSinNormalizar_QuedaFueraDeLaBaseDescuento(string categoriaOferta)
    {
        var lineas = new List<LineaCalculo>
        {
            new(20, 30000m, categoriaOferta),
            new(1, 10000m, "BEB"),
        };

        var resultado = CalculoTotales.Calcular(lineas, esVip: false, region: 13, fleteBase: 3500m, tasaIva: 0.19m);

        Assert.Equal((10000m, 0m), (resultado.BaseDescuento, resultado.Descuento));
    }

    // Un peso alrededor de los umbrales: con estos valores «>» y «>=» dan lo mismo,
    // así que el test no depende de la decisión pendiente de H1 (defectos-corregidos.md).
    [Theory]
    [InlineData(500001, "0.05", 25000)]  // 25.000,05
    [InlineData(999999, "0.05", 50000)]  // 49.999,95
    [InlineData(1000001, "0.08", 80000)] // 80.000,08
    public void Calcular_BaseUnPesoAlrededorDelUmbral_AplicaTramoDeVolumen(
        int baseDescuento, string porcentajeEsperado, int descuentoEsperado)
    {
        var lineas = new List<LineaCalculo> { new(1, baseDescuento, "BEB") };

        var resultado = CalculoTotales.Calcular(lineas, esVip: false, region: 13, fleteBase: 3500m, tasaIva: 0.19m);

        Assert.Equal(
            (decimal.Parse(porcentajeEsperado, CultureInfo.InvariantCulture), (decimal)descuentoEsperado),
            (resultado.PorcentajeDescuento, resultado.Descuento));
    }

    // VIP con solo ofertas: el +3 % se aplica sobre base 0. El porcentaje informado queda en 0,03
    // aunque el descuento sea 0, igual que el legado (UCalculoTotales.pas:88-89).
    // Pendiente con negocio: si la pantalla debe mostrar ese 3 %.
    [Fact]
    public void Calcular_VipConSoloOfertas_DescuentoCero()
    {
        var lineas = new List<LineaCalculo> { new(10, 35990m, "OFE") };

        var resultado = CalculoTotales.Calcular(lineas, esVip: true, region: 13, fleteBase: 3500m, tasaIva: 0.19m);

        Assert.Equal(
            (0m, 0.03m, 0m, 69046m, 432446m),
            (resultado.BaseDescuento, resultado.PorcentajeDescuento, resultado.Descuento, resultado.Iva, resultado.Total));
    }

    // La tasa sale de PARAMETROS.TASA_IVA (regla #9); el golden master siempre usa 0,19,
    // así que no detectaría una tasa fija en el código.
    [Theory]
    [InlineData("0.19", 19665, 123165)]
    [InlineData("0.10", 10350, 113850)]
    [InlineData("0", 0, 103500)]
    public void Calcular_TasaIvaDistinta_UsaLaTasaRecibida(string tasaIva, int ivaEsperado, int totalEsperado)
    {
        var lineas = new List<LineaCalculo> { new(1, 100000m, "BEB") };

        var resultado = CalculoTotales.Calcular(
            lineas, esVip: false, region: 13, fleteBase: 3500m,
            decimal.Parse(tasaIva, CultureInfo.InvariantCulture));

        Assert.Equal(((decimal)ivaEsperado, (decimal)totalEsperado), (resultado.Iva, resultado.Total));
    }

    /// <summary>Convierte <c>5x8210xBEB|30x5000xCON</c> en líneas de cálculo.</summary>
    private static List<LineaCalculo> LeerLineas(string lineas) =>
        lineas.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Split('x'))
            .Select(p => new LineaCalculo(
                int.Parse(p[0], CultureInfo.InvariantCulture),
                decimal.Parse(p[1], CultureInfo.InvariantCulture),
                p[2]))
            .ToList();

    /// <summary>Sube desde la carpeta de ejecución del test hasta encontrar el golden master del repo.</summary>
    private static string BuscarGoldenMaster()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var ruta = Path.Combine(dir.FullName, RutaGoldenMaster);
            if (File.Exists(ruta))
                return ruta;
        }

        throw new FileNotFoundException($"No se encontró {RutaGoldenMaster} subiendo desde {AppContext.BaseDirectory}.");
    }
}
