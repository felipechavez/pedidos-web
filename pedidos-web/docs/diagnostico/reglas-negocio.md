# Reglas de negocio de Pedidos Legado

<!--
Etapa 2 · Encontrar (modo por defecto + subagente analista-legado).
Una regla es una decisión del negocio escondida en el código: un monto, un porcentaje, una condición, un tope.
- Archivo:línea: dónde está. Sin esta evidencia, la regla no cuenta.
- Condición: cuándo se aplica. Efecto: qué cambia.
- Verificada: «Sí» solo si TÚ abriste el archivo en esa línea y leíste el código.
  Si la regla aparece en más de un lugar, anota todos.
Punto de control E2: al menos 8 reglas, y al menos 5 verificadas leyendo el código.
-->

> Hallazgos del subagente analista-legado, contrastados con el código (no con los comentarios).
> «Sí (IA)» = el agente abrió la línea y leyó el código. Pásalo a «Sí» cuando lo revises tú.
> Rutas relativas a `pedidos-web/`.

| # | Regla | Archivo:línea | Condición | Efecto | Verificada |
|---|---|---|---|---|---|
| 1 | Descuento por volumen **al grabar** | legado/src/UFrmPedidos.pas:296-301 | Base sin OFE `>=` 1.000.000 → 8 %; `>=` 500.000 → 5 %; si no, 0 % | % de descuento del pedido grabado | Sí (IA) |
| 2 | Descuento por volumen **en pantalla** (vista previa) | legado/src/UCalculoTotales.pas:77-82 | Base sin OFE `>` 1.000.000 → 8 %; `>` 500.000 → 5 % | Solo visual. Con base exacta 500.000 o 1.000.000 la pantalla muestra menos descuento que lo grabado (ver #1) | Sí (IA) |
| 3 | Cliente VIP suma 3 % de descuento | legado/src/UFrmPedidos.pas:119; legado/src/UFrmPedidos.pas:302-303; legado/src/UCalculoTotales.pas:88-89 | `TIPO = 'V'` (comparación exacta, sensible a mayúsculas) | +0,03 al % de descuento | Sí (IA) |
| 4 | Tope de descuento 10 % | legado/src/UFrmPedidos.pas:306-307; legado/src/UCalculoTotales.pas:90-91 | % acumulado `> 0.10` | Queda en 0,10 (8 % + 3 % VIP → 10 %) | Sí (IA) |
| 5 | Descuento Navidad +2 % (inactivo en la práctica) | legado/src/UFrmPedidos.pas:304-305; legado/src/UGlobales.pas:33 | `gDescNavidad` y mes de la fecha del pedido = 12 | +0,02 solo al grabar. `gDescNavidad` solo se asigna `False` y el parámetro DESC_NAVIDAD no se lee en ninguna parte | Sí (IA) |
| 6 | Líneas OFE no entran a la base de descuento | legado/src/UCalculoTotales.pas:70; legado/src/UFrmPedidos.pas:284-287 | Categoría ≠ 'OFE' | OFE paga precio completo. Pantalla: categoría guardada al agregar la línea, con `Trim` y `UpperCase`. Grabado: categoría actual en PRODUCTOS, sin normalizar | Sí (IA) |
| 7 | Redondeo «al peso» `Trunc(x + 0.49)` | legado/src/UCalculoTotales.pas:50; legado/src/UUtilidades.pas:30 | Se aplica a descuento, flete e IVA (no a subtotal ni total) | Sube solo con fracción ≥ 0,51; con ,50 baja (no es «mitad hacia arriba») | Sí (IA) |
| 8 | Flete por región, +15 % en regiones 11 y 12 | legado/src/UFrmPedidos.pas:121; legado/src/UFrmPedidos.pas:312-315; legado/src/UCalculoTotales.pas:97-100 | Región 11 (Aysén) o 12 (Magallanes) | REGIONES.FLETE_BASE × 1,15; en otras regiones × 1. En pantalla el flete es 0 sin líneas (UCalculoTotales.pas:116-117) | Sí (IA) |
| 9 | IVA sobre neto + flete | legado/src/UFrmPedidos.pas:318; legado/src/UCalculoTotales.pas:105; legado/src/UFrmPrincipal.pas:62-64; legado/src/UGlobales.pas:31 | Tasa = PARAMETROS.TASA_IVA / 100 (por defecto 0,19) | IVA redondeado y sumado al total | Sí (IA) |
| 10 | Pedido mínimo $30.000, salvo región 13 | legado/src/UFrmPedidos.pas:231 | Subtotal bruto (incluye OFE, antes del descuento) `< 30000` y región ≠ 13 | Bloquea la grabación. PARAMETROS.csv:3 dice 25.000, pero el código no lo lee: manda el 30.000 escrito en el código | Sí (IA) |
| 11 | Bloqueo por deuda vencida | legado/src/UFrmPedidos.pas:111-112 | `SALDO_VENCIDO > 0` **y** `DaysBetween(Date, FECHA_VENCIMIENTO) > 60` | No deja vender. `DaysBetween` no tiene signo: un vencimiento a más de 60 días en el futuro también bloquea | Sí (IA) |
| 12 | Límites de las líneas del pedido | legado/src/UFrmPedidos.pas:143; legado/src/UFrmPedidos.pas:209; legado/src/UFrmPedidos.pas:214; legado/src/UFrmPedidos.pas:221 | Cantidad ≤ 0; 0 líneas; más de 40 líneas; precio ≤ 0 | Rechaza (40 líneas sí se permite) | Sí (IA) |
| 13 | Corte a las 18:00 → siguiente día hábil | legado/src/UUtilidades.pas:124; legado/src/UUtilidades.pas:133-137; legado/src/UFrmPedidos.pas:240 | Hora del PC ≥ 18 | Fecha = siguiente día de lunes a viernes, sin feriados. Antes de las 18:00 de un sábado o domingo, el pedido queda con la fecha de ese día | Sí (IA) |
| 14 | Descuento de stock; VIP puede quedar en negativo | legado/src/UStock.pas:40-50; legado/src/UStock.pas:69-72 | Normal: stock nuevo < 0; VIP: stock nuevo < −20 | No descuenta esa línea y solo muestra un mensaje; el pedido ya está grabado | Sí (IA) |
| 15 | Permisos por nivel de usuario | legado/src/UFrmPrincipal.pas:49-50; legado/src/UFrmPrincipal.pas:95; legado/src/UFrmClientes.pas:143; legado/src/UFrmProductos.pas:44 | Nivel < 2: no graba clientes VIP ni ve informes. Nivel < 9: restricción en Productos (línea 44) | Solo ADMIN y JEFEVENTAS reciben nivel 9; ningún código asigna nivel 2 | Si |
| 16 | Cliente con pedidos no se puede eliminar | legado/src/UFrmClientes.pas:107; legado/src/UFrmClientes.pas:207-210 | `COUNT(*)` de PEDIDOS del cliente > 0 | Bloquea la eliminación | Sí (IA) |
| 17 | Validaciones del maestro de clientes | legado/src/UFrmClientes.pas:90-91; legado/src/UFrmClientes.pas:137; legado/src/UFrmClientes.pas:148-149 | Cliente nuevo; TIPO ∉ {N, V}; región < 1 o > 16 | Parte como tipo N y región 13; rechaza tipo o región inválidos | Sí (IA) |
| 18 | Observación tope 200 caracteres; estado siempre 'C' | legado/src/UFrmPedidos.pas:257-258; legado/src/UFrmPedidos.pas:262 | Siempre al grabar | Corta la observación y graba ESTADO = 'C'. No encontré código que grabe 'A' (anulado), aunque PEDIDOS.csv:19 lo tiene | Sí (IA) |

## Contradicciones comentario/README vs código (manda el código)

- **legado/src/UCalculoTotales.pas:4.** Dice «descuento por volumen 5% sobre 400.000». El código usa `> 500000` (línea 79) y además 8 % sobre 1.000.000 (línea 77).
- **legado/src/UCalculoTotales.pas:5.** Dice «NO TOCAR sin hablar con contabilidad», como si fuera el cálculo oficial. Pero el total que se graba lo calcula `UFrmPedidos.pas:296-319`; esta unidad solo alimenta la pantalla.
- **legado/src/UFrmPedidos.pas:326.** Dice «aviso a vendedor si el total cambio». El aviso solo sale si `gModoPrueba` (línea 330) está activo, y solo se le asigna `False` (UGlobales.pas:34).
- **legado/src/UFrmPedidos.pas:334.** Dice «log simple». No escribe ningún log: muestra el aviso «Primer pedido del cliente» (línea 342).
- **legado/src/UFrmPedidos.pas:110.** Dice «con deuda vencida no se le vende». El código exige además más de 60 días de diferencia, contados sin signo.
- **legado/src/UStock.pas:32.** Dice «devuelve False si no alcanza». Para VIP devuelve True aunque el stock quede negativo, hasta −20.
- **legado/src/UGlobales.pas:13.** Define «2 = supervisor». Ningún código asigna el nivel 2.
- **legado/src/UGlobales.pas:15.** Dice «flete por defecto» (`gFleteBase` = 3500). No encontré ningún uso: el flete sale de REGIONES.FLETE_BASE.
- **legado/src/UFrmInformes.pas:91.** Dice «el total viene con iva». El total también trae flete, así que el «Neto» del informe incluye el flete.
