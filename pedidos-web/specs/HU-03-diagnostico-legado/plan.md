# HU-03 · Plan del diagnóstico del legado

> Etapa 1 «Entender» de la Unidad 4. Este plan llena `docs/diagnostico/inventario.md` y `docs/diagnostico/dependencias.md` a partir del código de `legado/`. Aún no se ejecuta: espera aprobación.

## Contexto

Hay que llenar dos plantillas vacías en `docs/diagnostico/` a partir del código de `legado/`:

- `inventario.md`: cada `.pas`, `.dfm` (y el `.dpr`) más cada tabla, con su responsabilidad y sus líneas.
- `dependencias.md`: diagrama Mermaid de quién usa a quién, con la base DBISAM como nodo.

Todo sale del código (cláusulas `uses`, componentes de los `.dfm`, SQL embebido y `legado/db/esquema-dbisam.sql`).
**No** se usa `unidad-4/taller/u4-respaldo/` (es la solución de respaldo del curso).

## Hallazgos de la exploración (con evidencia)

**Archivos y líneas** (contadas con `wc -l`):

| Archivo | Líneas | Archivo | Líneas |
|---|---|---|---|
| PedidosLegado.dpr | 24 | UFrmPedidos.pas / .dfm | 420 / 226 |
| UCalculoTotales.pas | 147 | UFrmPrincipal.pas / .dfm | 104 / 62 |
| UDMPrincipal.pas / .dfm | 100 / 65 | UFrmProductos.pas / .dfm | 105 / 98 |
| UFrmClientes.pas / .dfm | 215 / 213 | UGlobales.pas | 41 |
| UFrmInformes.pas / .dfm | 137 / 160 | UStock.pas | 93 |
| UUtilidades.pas | 189 | esquema-dbisam.sql | 74 |

**Dependencias (`uses` de unidades propias + acceso a la BD):**

- `PedidosLegado.dpr` crea `DMPrincipal` y `FrmPrincipal` (dpr:21-22).
- `UFrmPrincipal` → UDMPrincipal, UGlobales, UFrmClientes, UFrmProductos, UFrmPedidos, UFrmInformes (pas:38-39).
- `UDMPrincipal` → UGlobales (pas:36); dueño de la conexión DBISAM y de 5 tablas + 2 queries (dfm:9-60).
- `UFrmClientes` → UDMPrincipal, UGlobales (pas:55); además su propio `tblCli` sobre CLIENTES (dfm:200-204) y SQL a PEDIDOS (pas:207).
- `UFrmProductos` → solo UGlobales (pas:36); su propio `tblProd` sobre PRODUCTOS (dfm:85-89). **No pasa por el data module.**
- `UFrmPedidos` → UCalculoTotales (pas:9), UDMPrincipal, UGlobales, UUtilidades, UStock (pas:67); SQL a CLIENTES+REGIONES, PRODUCTOS, PEDIDOS, PEDIDO_LINEAS (pas:101-320).
- `UFrmInformes` → UGlobales, UUtilidades (pas:46); su propio `qryInforme` (dfm:149) con JOIN PEDIDOS/CLIENTES (pas:66). **No pasa por el data module.**
- `UStock` → UDMPrincipal, UGlobales (pas:15); SQL a PRODUCTOS, PEDIDO_LINEAS y **MOV_STOCK** (pas:85).
- `UCalculoTotales` (solo SysUtils) y `UUtilidades` (SysUtils, DateUtils): **puras**, sin formularios ni BD.
- `UGlobales`: sin dependencias; la usan casi todas (6 unidades).

**Tablas** (`esquema-dbisam.sql`): REGIONES, CLIENTES, PRODUCTOS, PEDIDOS, PEDIDO_LINEAS, PARAMETROS (+ CSV de datos para las 6).
**MOV_STOCK se usa en UStock.pas:85, pero no existe en el esquema ni en los CSV.**

**Cosas que no calzan con `legado/README.md`** (para «Lo que me llamó la atención»):

- El README dice Paradox vía BDE y Crystal Reports 8; el código usa DBISAM 4.24 (UDMPrincipal.dfm:10, comentario «antes era alias BDE» en UDMPrincipal.pas:58) y el informe es una grilla con exportación a CSV, sin Crystal.
- La tabla MOV_STOCK se referencia, pero no existe en el esquema.
- El cálculo de totales está duplicado: UFrmPedidos.pas:308-327 recalcula con `RedondeaPesos` y compara contra `CalcularTotales` (que usa `Calc3`). Extra (riesgo, no inventario): clave de BD fija en UDMPrincipal.pas:41 y SQL concatenado.

## Pasos de ejecución

1. **Confirmar responsabilidades**: leer completos UFrmClientes, UFrmPedidos, UFrmInformes, UStock y UCalculoTotales para redactar cada responsabilidad en una frase (sin suponer).
2. **Escribir `docs/diagnostico/inventario.md`**:
   - Tabla principal `Archivo | Tipo | Responsabilidad | Líneas` con las 16 filas de `legado/src/` + `PedidosLegado.dpr` al final (tipos: Formulario, Formulario (diseño), Data module, Unidad de lógica, Utilidades, Globales, Programa).
   - Segunda tabla **«Tablas de la base (DBISAM)»**: `Tabla | Responsabilidad | Usada por | Filas de datos (CSV)` para las 6 tablas + una fila MOV_STOCK marcada «no existe en el esquema». Filas del CSV contadas con `wc -l` (menos el encabezado). Opcional: fila para `esquema-dbisam.sql` (74 líneas).
   - «Lo que me llamó la atención»: las 3 observaciones de arriba, con archivo:línea.
3. **Escribir `docs/diagnostico/dependencias.md`**: reemplazar el bloque `flowchart LR` vacío por:
   - Flechas sólidas `A --> B` = `uses`.
   - Nodo base `BD[(DBISAM · C:\PEDIDOS\DATOS)]`; `DM --> BD` sólida; flechas punteadas `-.->|tabla|` para los accesos directos que saltan el data module (UFrmClientes, UFrmProductos, UFrmInformes, UFrmPedidos, UStock), etiquetadas con las tablas.
   - `classDef` para destacar las unidades puras (UCalculoTotales, UUtilidades) y el hub UGlobales.
   - Leyenda breve bajo el diagrama y respuestas a las 2 preguntas guía (UGlobales como hub; UCalculoTotales/UUtilidades como candidatas a migrar primero a `PedidosWeb.Core`, porque no dependen de UI ni de BD).
4. No se toca nada de `legado/`, `src/` ni `tests/`; solo los dos `.md` de `docs/diagnostico/`. Sin commit (lo hace una persona).

## Verificación

- Recontar con `wc -l legado/src/* legado/PedidosLegado.dpr` y comparar contra la columna Líneas.
- `ls legado/src` vs. filas del inventario: deben estar las 16 (punto de control E1).
- `grep "CREATE TABLE"` en el esquema vs. tabla de BD: las 6 + MOV_STOCK.
- Cada flecha del diagrama tiene su `uses` o SQL en el código (grep de los nombres de unidad).
- Abrir `dependencias.md` en el preview Markdown de VS Code para confirmar que el Mermaid renderiza y que aparece el nodo BD (punto de control E1).
