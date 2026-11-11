# Defectos corregidos · migración del cálculo de totales

> Regla: si un valor esperado de `migracion/golden-master.csv` cambia, ese caso aparece aquí.
> Rutas relativas a `pedidos-web/`. Montos en CLP.

## D1 · R05 · Redondeo al peso (CORREGIDO)

- **Qué hace el legado:** redondea con `Trunc(valor + 0.49)` (`legado/src/UCalculoTotales.pas:50`, `Calc3`; la misma fórmula está en `legado/src/UUtilidades.pas:30`, `RedondeaPesos`, que usa la grabación en `legado/src/UFrmPedidos.pas:308`, `:313` y `:318`). Una fracción de exactamente ,50 **baja** en vez de subir. Se aplica al descuento, al flete y al IVA. Es la regla #7 de `docs/diagnostico/reglas-negocio.md`.
- **Corrección:** `CalculoTotales.RedondearAlPeso` usa `Math.Round(valor, 0, MidpointRounding.AwayFromZero)`: desde ,50 sube al peso siguiente (`src/PedidosWeb.Core/Totales/CalculoTotales.cs`). Lo prueba `Calcular_FleteConFraccion_RedondeaMitadHaciaArriba` (,49 / ,50 / ,51 y flete de la región 12).
- **Por qué:** el redondeo al peso que se espera (y que usa la facturación) sube desde ,50. Con el legado, **todos los pedidos de las regiones 11 y 12** tienen un flete con fracción ,50 (14.990 × 1,15 = 17.238,50 y 16.490 × 1,15 = 18.963,50), así que todos quedan un peso abajo. Además, un descuento o un IVA que termina justo en ,50 se redondea mal. Contabilidad debe confirmar que este criterio coincide con el de la facturación electrónica.
- **Casos del golden master que cambian:** 13 de 60.

  | Caso | Folio | Origen de la fracción ,50 | Cambio | Total |
  |---|---|---|---|---|
  | 2 | 1002 | IVA 36.964,50 | IVA 36.964 → 36.965 | 231.514 → 231.515 |
  | 6 | 1006 | Descuento 25.767,50 | Descuento 25.767 → 25.768; neto −1 | 1.041.908 → 1.041.907 |
  | 8 | 1008 | IVA 100.120,50 | IVA 100.120 → 100.121 | 627.070 → 627.071 |
  | 18 | 1018 | Flete región 12 | Flete 18.963 → 18.964 | 180.360 → 180.361 |
  | 21 | 1021 | Flete región 11 | Flete 17.238 → 17.239; arrastra IVA 248.778 → 248.779 | 1.558.138 → 1.558.140 |
  | 23 | 1023 | Flete región 12 | Flete 18.963 → 18.964 | 599.716 → 599.717 |
  | 25 | 1025 | Descuento 46.425,50 | Descuento 46.425 → 46.426; neto −1; arrastra IVA 410.542 → 410.541 | 2.571.287 → 2.571.285 |
  | 27 | 1027 | Flete región 11 | Flete 17.238 → 17.239 | 1.510.417 → 1.510.418 |
  | 36 | 1036 | Flete región 12 | Flete 18.963 → 18.964 | 1.833.007 → 1.833.008 |
  | 37 | 1037 | Flete región 12 | Flete 18.963 → 18.964 | 724.114 → 724.115 |
  | 42 | 1042 | Flete región 12 | Flete 18.963 → 18.964 | 1.153.066 → 1.153.067 |
  | 46 | 1046 | Flete región 11 | Flete 17.238 → 17.239 | 1.708.835 → 1.708.836 |
  | 54 | 1054 | Flete región 11 | Flete 17.238 → 17.239 | 704.391 → 704.392 |

  No siempre sube el total: cuando la fracción está en el descuento (casos 6 y 25), el cliente paga menos.
- **Suma de la columna total:** 58.202.135 → 58.202.144 (+9).
- **Commit:** `fix(totales): R05 redondeo al peso comercial (,50 sube)`. Va separado del commit «igual al legado» (`dcc9b41`).
- **Convivencia:** mientras el legado siga grabando pedidos, `RedondeaPesos` mantiene el defecto. La conciliación debe aceptar diferencias de −2 a +2 pesos **solo** en pedidos donde el descuento, el flete o el IVA terminan en ,50 (todos los de las regiones 11 y 12), y debe explicarlas recalculando con ambos redondeos. Cualquier otra diferencia es un incidente.

## H1 · Umbrales del descuento por volumen: `>` en pantalla, `>=` al grabar (HALLAZGO, NO CORREGIDO)

- **Qué encontraste:** el legado tiene dos versiones del descuento por volumen, y una tercera en un comentario:
  - Pantalla: `> 1000000` → 8 % y `> 500000` → 5 % (`legado/src/UCalculoTotales.pas:77` y `:79`). Esto es lo que replican el golden master y el C# (`src/PedidosWeb.Core/Totales/CalculoTotales.cs`, `PorcentajeVolumen`).
  - Grabación: `>= 1000000` y `>= 500000` (`legado/src/UFrmPedidos.pas:296` y `:298`). **Este es el total que quedó en la base:** `legado/db/datos/PEDIDOS.csv` coincide con `>=` en los 13 folios afectados (por ejemplo, el folio 1008 grabó 597.320 y el golden master dice 627.070).
  - Comentario: «descuento por volumen 5% sobre 400.000 (acta comercial 2011)» (`legado/src/UCalculoTotales.pas:4`). Ninguna versión del código usa 400.000.
- **Efecto:** 13 de los 60 pedidos tienen la base del descuento **exactamente** en el umbral:
  - Base 500.000 (0 % → 5 %, o 3 % → 8 % si es VIP): folios 1008, 1011, 1019, 1023, 1050 y 1055. Son 29.750 por pedido.
  - Base 1.000.000 (5 % → 8 %): folios 1026, 1032, 1035, 1039, 1042, 1048 y 1060. Son 35.700 por pedido.
  - En total, el C# cobra **428.400 más** que lo grabado por el legado en esos 13 pedidos (la diferencia es la misma con el redondeo de D1 o sin él). Hoy la pantalla le mostró al vendedor un total mayor que el que se grabó, y el aviso de diferencia nunca sale (`legado/src/UGlobales.pas:34`, `gModoPrueba = False`).
  - Si el umbral del 5 % fuera «más de 400.000», como dice el comentario, cambiarían 9 casos: los 6 de base 500.000 y además los folios 1017, 1020 y 1057. Sería 254.409 menos que el C# actual.
- **Quién decide:** Comercial (dueño del acta de 2011), junto con Contabilidad. El C# replica la versión de **pantalla** (`>`), porque ese es el contrato del golden master. Hay que decidirlo **antes de la etapa 3** del plan: si el portal pasa a grabar pedidos con `>`, los clientes que quedan justo en el umbral pagarán más que hoy.
