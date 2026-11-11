# Inventario de Pedidos Legado

<!--
Etapa 1 · Entender (plan mode).
Una fila por cada archivo de legado/src/ (.pas y .dfm). Puedes agregar legado/PedidosLegado.dpr al final.
- Tipo: Formulario, Data module, Unidad de lógica, Utilidades, Globales…
- Responsabilidad: qué hace, en una frase y con tus palabras.
- Líneas: pídele a Claude que las cuente con una herramienta; no las estimes a ojo.
Punto de control E1: están todos los archivos de legado/src/.
-->

> Líneas contadas con `wc -l` sobre `legado/` (16 archivos de `legado/src/` + el `.dpr`).

| Archivo | Tipo | Responsabilidad | Líneas |
|---|---|---|---|
| `legado/src/UCalculoTotales.pas` | Unidad de lógica | Calcula los totales del pedido en memoria: subtotal, base de descuento (sin categoría `OFE`), % de descuento por volumen y VIP (tope 10 %), flete por región, IVA y total. No usa pantallas ni base de datos. | 147 |
| `legado/src/UDMPrincipal.pas` | Data module | Abre y cierra la conexión DBISAM (con clave fija), lee parámetros de la tabla `PARAMETROS` y ejecuta SQL de escritura para el resto del sistema. | 100 |
| `legado/src/UDMPrincipal.dfm` | Data module (diseño) | Declara la base `PEDIDOS` (`C:\PEDIDOS\DATOS`), 5 tablas (CLIENTES, PRODUCTOS, PEDIDOS, PEDIDO_LINEAS, PARAMETROS) y 2 consultas compartidas (`qryGeneral`, `qryAux`). | 65 |
| `legado/src/UFrmClientes.pas` | Formulario | Mantenedor de clientes: busca por razón social, crea con valores por defecto, valida antes de grabar (RUT, tipo N/V, VIP solo jefatura, región 1-16) y no deja eliminar clientes con pedidos. | 215 |
| `legado/src/UFrmClientes.dfm` | Formulario (diseño) | Pantalla de clientes: buscador, grilla, campos editables enlazados a su propia tabla `tblCli` (CLIENTES) y botones Nuevo/Grabar/Eliminar/Cerrar. | 213 |
| `legado/src/UFrmInformes.pas` | Formulario | Informe de ventas por rango de fechas, cliente y estado: suma totales, separa neto e IVA y exporta a CSV para contabilidad. | 137 |
| `legado/src/UFrmInformes.dfm` | Formulario (diseño) | Pantalla del informe: filtros (desde, hasta, cliente, solo confirmados), grilla, resumen y su propia consulta `qryInforme`. | 160 |
| `legado/src/UFrmPedidos.pas` | Formulario | Toma de pedidos: busca al cliente por RUT (bloquea si tiene deuda vencida > 60 días), agrega productos, muestra totales, valida y graba pedido y líneas, recalcula el total con SQL y descuenta stock. | 420 |
| `legado/src/UFrmPedidos.dfm` | Formulario (diseño) | Pantalla de toma de pedidos: datos del cliente, ingreso de productos, grilla de líneas, totales, observaciones y botones. | 226 |
| `legado/src/UFrmPrincipal.pas` | Formulario | Ventana principal: pide el usuario y define su nivel, conecta la base, carga parámetros (IVA, impresora) y abre las demás pantallas desde el menú (informes solo con nivel ≥ 2). | 104 |
| `legado/src/UFrmPrincipal.dfm` | Formulario (diseño) | Menú principal (Archivo, Maestros, Ventas) y barra de estado. | 62 |
| `legado/src/UFrmProductos.pas` | Formulario | Mantenedor de productos: filtra por descripción o bajo stock mínimo, valida antes de grabar (código, precio > 0, categoría de 3 letras) y oculta Eliminar a quien no es admin. | 105 |
| `legado/src/UFrmProductos.dfm` | Formulario (diseño) | Pantalla de productos: filtros, grilla, navegador y su propia tabla `tblProd` (PRODUCTOS). | 98 |
| `legado/src/UGlobales.pas` | Globales | Variables globales compartidas (usuario, nivel, tasa IVA, flete base, descuento navidad, ruta de datos, impresora) y sus valores iniciales. | 41 |
| `legado/src/UStock.pas` | Unidad de lógica | Consulta y descuenta stock por producto (los VIP pueden quedar hasta -20) y registra cada movimiento en `MOV_STOCK`. | 93 |
| `legado/src/UUtilidades.pas` | Utilidades | Funciones puras de apoyo: redondeo y formato de pesos, validación y formato de RUT, fechas (día hábil, fecha del pedido según hora de cierre 18:00), textos. | 189 |
| `legado/PedidosLegado.dpr` | Programa | Punto de entrada: registra todas las unidades, crea el data module y la ventana principal. | 24 |

## Tablas de la base (DBISAM)

> Esquema en `legado/db/esquema-dbisam.sql` (74 líneas). Filas de datos = líneas del CSV en `legado/db/datos/` menos el encabezado.

| Tabla | Responsabilidad | Usada por | Filas de datos |
|---|---|---|---|
| `REGIONES` | Regiones con su flete base. | UFrmPedidos (JOIN al buscar cliente) | 16 |
| `CLIENTES` | Clientes: RUT, razón social, tipo (N/V), región, saldo vencido y estado activo. | UDMPrincipal, UFrmClientes, UFrmPedidos, UFrmInformes | 30 |
| `PRODUCTOS` | Catálogo: código, descripción, categoría, precio, stock y stock mínimo. | UDMPrincipal, UFrmProductos, UFrmPedidos, UStock | 40 |
| `PEDIDOS` | Cabecera del pedido: folio, fecha, cliente, usuario, estado, observación y total. | UDMPrincipal, UFrmPedidos, UFrmClientes, UFrmInformes | 60 |
| `PEDIDO_LINEAS` | Líneas del pedido: producto, cantidad y precio unitario. | UDMPrincipal, UFrmPedidos, UStock | 169 |
| `PARAMETROS` | Configuración clave/valor (TASA_IVA, PEDIDO_MINIMO, IMPRESORA, VERSION, DESC_NAVIDAD, EMPRESA). | UDMPrincipal (`LeerParametro`), UFrmPrincipal | 6 |
| `MOV_STOCK` | Registro de movimientos de stock. **No existe en el esquema ni en los CSV.** | UStock (`legado/src/UStock.pas:85`) | — |

## Lo que me llamó la atención

- **El README no describe este sistema.** `legado/README.md` habla de Paradox vía BDE y de Crystal Reports 8, pero el código usa DBISAM 4.24 (`legado/src/UDMPrincipal.dfm:10`; «antes era alias BDE» en `legado/src/UDMPrincipal.pas:58`) y el informe es una grilla con exportación a CSV, sin Crystal (`legado/src/UFrmInformes.pas:101`).
- **El descuento está escrito dos veces y no coincide.** `UCalculoTotales.pas:77-79` usa `>` 1.000.000 / `>` 500.000 y no aplica navidad; `UFrmPedidos.pas:296-304` usa `>=` y suma navidad. El total que se graba es el de `UFrmPedidos`, y la pantalla muestra el de `UCalculoTotales`. Además, el comentario de `UCalculoTotales.pas:4` dice «5 % sobre 400.000», y ninguna de las dos versiones usa 400.000.
- **Hay parámetros en la base que el código ignora y valores fijos en el código.** `PARAMETROS` tiene `PEDIDO_MINIMO = 25000` y `DESC_NAVIDAD`, pero `UFrmPedidos.pas:231` exige $30.000 fijo y nunca se lee `DESC_NAVIDAD` (`gDescNavidad` queda en `False`, `UGlobales.pas:33`). `UFrmInformes.pas:92` usa 0,19 fijo en vez de la tasa leída. A eso se suma que `MOV_STOCK` no existe: el `INSERT` de `UStock.pas:85` falla en silencio (`except` vacío).
