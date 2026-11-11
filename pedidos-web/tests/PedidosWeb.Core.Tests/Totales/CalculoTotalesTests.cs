using System.Globalization;
using PedidosWeb.Core.Totales;

namespace PedidosWeb.Core.Tests.Totales;

/// <summary>
/// Golden master de la migración del cálculo de totales: cada fila de
/// <c>migracion/golden-master.csv</c> es lo que calcula hoy el legado, salvo los defectos
/// corregidos que registra <c>docs/migracion/defectos-corregidos.md</c>.
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
            yield return
            [
                int.Parse(c[Col("caso")], CultureInfo.InvariantCulture),
                c[Col("folio")],
                c[Col("tipo_cliente")],
                int.Parse(c[Col("region")], CultureInfo.InvariantCulture),
                decimal.Parse(c[Col("flete_base")], CultureInfo.InvariantCulture),
                decimal.Parse(c[Col("tasa_iva")], CultureInfo.InvariantCulture),
                c[Col("lineas")],
                decimal.Parse(c[Col("descuento")], CultureInfo.InvariantCulture),
                decimal.Parse(c[Col("flete")], CultureInfo.InvariantCulture),
                decimal.Parse(c[Col("iva")], CultureInfo.InvariantCulture),
                decimal.Parse(c[Col("total")], CultureInfo.InvariantCulture)
            ];
        }
    }

    // caso y folio no se usan en el cálculo: están para identificar cada caso en el explorador de tests.
#pragma warning disable xUnit1026
    [Theory]
    [MemberData(nameof(CasosGoldenMaster))]
    public void Calcular_CasoGoldenMaster_IgualAlLegado(
        int caso, string folio, string tipoCliente, int region, decimal fleteBase, decimal tasaIva,
        string lineas, decimal descuento, decimal flete, decimal iva, decimal total)
#pragma warning restore xUnit1026
    {
        var resultado = CalculoTotales.Calcular(LeerLineas(lineas), tipoCliente == "V", region, fleteBase, tasaIva);

        Assert.Equal(
            (descuento, flete, iva, total),
            (resultado.Descuento, resultado.Flete, resultado.Iva, resultado.Total));
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
