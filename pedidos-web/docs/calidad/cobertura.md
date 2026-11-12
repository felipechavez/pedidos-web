# Cobertura de código

La cobertura de los tests xUnit se mide con **coverlet** (`coverlet.collector`, referenciado en `tests/PedidosWeb.Core.Tests`) y se presenta con **ReportGenerator**, declarado como herramienta local en `dotnet-tools.json`.

## Generar el reporte en local

Desde la carpeta `pedidos-web/`:

```bash
# 1. Restaurar las herramientas locales (solo la primera vez)
dotnet tool restore

# 2. Correr los tests recolectando cobertura
dotnet test PedidosWeb.sln --settings tests/coverlet.runsettings --results-directory TestResults

# 3. Convertir el XML de coverlet en un reporte HTML
dotnet tool run reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:TestResults/coverage-report" "-reporttypes:Html;TextSummary"
```

Abre `TestResults/coverage-report/index.html` en el navegador. El resumen en texto queda en `TestResults/coverage-report/Summary.txt`.

`TestResults/` está en `.gitignore`: el reporte nunca se sube al repositorio. Bórralo antes de volver a medir para no mezclar corridas anteriores.

## Qué se mide

La configuración vive en `tests/coverlet.runsettings`:

- Se incluyen los ensamblados `PedidosWeb.*` y se excluyen los de tests (`*.Tests`).
- Se ignora el código generado y lo marcado con `[ExcludeFromCodeCoverage]`.
- Las propiedades automáticas (`{ get; set; }`) no cuentan como líneas a cubrir.

Hoy solo `PedidosWeb.Core` tiene tests, por lo que `PedidosWeb.Api` no aparece en el reporte.

## En CI

El workflow `.github/workflows/ci.yml` ejecuta los tests con cobertura en cada push y pull request a `main`:

- El resumen de cobertura aparece en la página del run (*Summary*).
- El reporte HTML completo se descarga como artifact `reporte-cobertura` (se conserva 14 días).

La cobertura se informa, pero **no** bloquea el build: no hay un umbral mínimo definido.

## Línea base (11-11-2026)

| Métrica | Valor |
|---|---|
| Líneas | 97 % (422 de 435) |
| Ramas | 74,5 % (79 de 106) |
| Métodos | 100 % (37 de 37) |

Clases con menor cobertura de líneas: `FolioFormatter` (70 %), `ClientesService` (90,6 %) y `PedidoServicio` (91,2 %).
