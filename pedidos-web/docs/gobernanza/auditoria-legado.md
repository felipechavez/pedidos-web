# Auditoría del legado · pedidos-web

> Alcance: `legado/src/UFrmPedidos.pas` y `legado/src/UDMPrincipal.pas`. Las otras unidades se usaron solo para confirmar dónde se valida cada regla.
> Los hallazgos los propuso el subagente analista-legado. Cada archivo:línea se abrió y se contrastó con el código antes de marcarlo como Validado o Descartado.
> Las rutas son relativas a `pedidos-web/`.

## Hallazgos

| ID | Hallazgo | Evidencia (archivo:línea) | Tipo | Severidad | Estado | Argumento |
|---|---|---|---|---|---|---|
| AU-01 | La búsqueda de cliente arma el SQL concatenando el RUT, y `ValidaRut` no impide la inyección | legado/src/UFrmPedidos.pas:102; legado/src/UFrmPedidos.pas:91; legado/src/UUtilidades.pas:67; legado/src/UUtilidades.pas:73-74 | Seguridad | Alta | Validado | La línea 102 concatena `'WHERE C.RUT = ''' + vRut + ''''`. `ValidaRut` solo es un filtro de forma: `StrToIntDef(vCuerpo[i], 0)` convierte letras y comillas en 0. Con suma 0, el DV esperado es `'0'`, así que un texto como `x' OR 'a'='a' OR 'b'='0` pasa la validación y llega entero al WHERE. Además, `LimpiaRut` valida una versión sin espacios, pero al SQL va el texto original. |
| AU-02 | La búsqueda de producto arma el SQL concatenando el código | legado/src/UFrmPedidos.pas:152 | Seguridad | Alta | Validado | `'FROM PRODUCTOS WHERE CODIGO = ''' + Trim(edtCodProducto.Text) + ''''` no pasa por validación ni escape. Por verificar en DBISAM: si acepta `UNION`, se podría agregar al pedido una fila con PRECIO o CATEGORIA inventados. |
| AU-03 | El INSERT del pedido concatena la observación y el usuario | legado/src/UFrmPedidos.pas:261-262 | Seguridad | Alta | Validado | `vObs` sale del memo y `gUsuario` del `InputBox` (UFrmPrincipal.pas:46), y ninguno se escapa. `EscapaComillas` está definida (UUtilidades.pas:164), pero no se llama en ninguna parte de `legado/`. Una comilla en la observación rompe el INSERT. |
| AU-04 | El DataModule ejecuta cualquier cadena SQL ya armada, sin parámetros ni manejo de errores | legado/src/UDMPrincipal.pas:85; legado/src/UDMPrincipal.pas:95-96 | Seguridad | Media | Validado | `LeerParametro` concatena `AClave`, y `EjecutarSQL` asigna `qryGeneral.SQL.Text := ASQL` y ejecuta. No hay `ParamByName` ni `Params` en `legado/`. Es la causa común de AU-01 a AU-03. Hoy `LeerParametro` solo recibe constantes (UFrmPrincipal.pas:62, 65). |
| AU-05 | Clave de la base de datos escrita en el código | legado/src/UDMPrincipal.pas:41; legado/src/UDMPrincipal.pas:59 | Seguridad | Alta | Validado | `CLAVE_BD = 'Distrib#2008'` se usa en `Session.AddPassword(CLAVE_BD)`. Queda compilada en el ejecutable y es la misma en todas las instalaciones. |
| AU-06 | Ruta de datos escrita en el código, presentada como riesgo de seguridad («carpeta compartida» cuyo acceso depende del sistema de archivos) | legado/src/UDMPrincipal.pas:60; legado/src/UGlobales.pas:36; legado/src/UDMPrincipal.dfm:12 | Seguridad | Baja | Descartado | Las líneas solo muestran una ruta local fija: `gRutaDatos := 'C:\PEDIDOS\DATOS'` y `Directory = 'C:\PEDIDOS\DATOS'`. Nada en el código dice que la carpeta sea compartida ni cómo se controla el acceso. El acceso a la base lo protege la clave de UDMPrincipal.pas:59, que ya está en AU-05. Como seguridad, no se confirma. Como configuración en duro, es mantenibilidad. |
| AU-07 | No hay autenticación: el nivel se obtiene escribiendo un nombre, y la toma de pedidos no revisa permisos | legado/src/UFrmPrincipal.pas:46-50; legado/src/UFrmPrincipal.pas:87-91; legado/src/UFrmPedidos.pas:261 | Seguridad | Alta | Validado | `if (gUsuario = 'ADMIN') or (gUsuario = 'JEFEVENTAS') then gNivelUsuario := 9;` no pide contraseña. `mnuPedidosClick` abre el formulario sin revisar el nivel, y `UFrmPedidos` nunca lee `gNivelUsuario`. El campo `PEDIDOS.USUARIO` guarda el nombre que el usuario escribe, así que no sirve para auditar. |
| AU-08 | Grabar un pedido no usa transacción ni manejo de errores | legado/src/UFrmPedidos.pas:255; legado/src/UFrmPedidos.pas:262-263; legado/src/UFrmPedidos.pas:272; legado/src/UFrmPedidos.pas:320; legado/src/UFrmPedidos.pas:324 | Integridad | Alta | Validado | `// TODO transaccion`. La cabecera se inserta con `TOTAL` 0 (`''', 0)'`), luego se insertan las líneas, se hace el `UPDATE` del total y se descuenta stock, cada paso por separado. No hay `try/except` en `btnGrabarClick` ni `StartTransaction`, `Commit` o `Rollback` en `legado/`. Si algo falla a mitad de camino, queda un pedido incompleto o con total 0. |
| AU-09 | El folio se calcula como MAX+1 | legado/src/UFrmPedidos.pas:249-251; legado/db/esquema-dbisam.sql:52 | Integridad | Media | Validado | `SELECT MAX(FOLIO)` seguido de `+ 1` no tiene bloqueo. Si dos puestos graban a la vez, obtienen el mismo folio. La `PRIMARY KEY ("FOLIO")` rechaza el segundo INSERT con una excepción que nadie captura (ver AU-08). El mismo patrón se usa para `ID_CLIENTE` en UFrmClientes.pas:160-163. |
| AU-10 | El bloqueo por deuda vencida solo existe en la pantalla de pedidos, y se evita editando la deuda en el maestro de clientes | legado/src/UFrmPedidos.pas:111-112; legado/src/UFrmPedidos.pas:204; legado/src/UFrmClientes.dfm:51-59; legado/src/UFrmClientes.pas:122-153 | Regla de negocio | Alta | Validado | La regla `SALDO_VENCIDO > 0` y `DaysBetween(...) > 60` se evalúa solo al pulsar «Buscar». Al grabar solo se revisa `FIdCliente = 0` y la base no tiene restricciones. `grdClientes` es un `TDBGrid` sin `ReadOnly`, y `tblCliBeforePost` no valida `SALDO_VENCIDO` ni `FECHA_VENCIMIENTO`. El menú Clientes se abre sin revisar el nivel (UFrmPrincipal.pas:75-79), así que cualquier usuario puede dejar la deuda en 0 y vender. |
| AU-11 | Los clientes inactivos pueden comprar | legado/src/UFrmPedidos.pas:99-102; legado/src/UFrmClientes.pas:93 | Regla de negocio | Media | Validado | El SELECT del cliente no filtra por `ACTIVO`. En todo `legado/src` ese campo solo aparece cuando se le asigna `True` al crear un cliente. |
| AU-12 | El pedido mínimo está escrito en el código y solo se valida en la pantalla | legado/src/UFrmPedidos.pas:231; legado/src/UFrmPrincipal.pas:62; legado/src/UFrmPrincipal.pas:65 | Regla de negocio | Media | Validado | `if (vSubtotal < 30000) and (FRegion <> 13) then` usa el subtotal bruto y exime a la región 13. `PEDIDO_MINIMO` nunca se lee: `LeerParametro` solo se llama con `'TASA_IVA'` e `'IMPRESORA'`. La base no lo valida. |
| AU-13 | El descuento por volumen que se graba usa `>=` y el de la pantalla usa `>`, así que el total grabado difiere del que vio el vendedor | legado/src/UFrmPedidos.pas:296-299; legado/src/UCalculoTotales.pas:77-79; legado/src/UFrmPedidos.pas:327-331; legado/src/UGlobales.pas:34 | Regla de negocio | Alta | Validado | Al grabar: `if vBase >= 1000000` / `>= 500000`. En pantalla: `if ABase > 1000000` / `> 500000`. Con una base exacta de 500.000 o 1.000.000 se graba un descuento que no se mostró. El aviso solo sale `if gModoPrueba`, y `gModoPrueba := False`, así que la diferencia nunca se avisa. |
| AU-14 | El pedido se graba aunque no haya stock, y el stock se lee y se escribe sin bloqueo | legado/src/UFrmPedidos.pas:324; legado/src/UStock.pas:38-51; legado/src/UStock.pas:69-72 | Regla de negocio | Media | Validado | `DescontarStockPedido` corre después de insertar cabecera y líneas. Si falta stock, solo muestra `ShowMessage('Sin stock suficiente…')` y el pedido queda en estado `'C'`. Los VIP pueden dejar el stock hasta −20 (`if vNuevo < -20 then Exit;`). La lectura y el UPDATE van separados, sin bloqueo. |
| AU-15 | El registro de movimientos de stock oculta todos los errores | legado/src/UStock.pas:84-90 | Integridad | Media | Validado | `try DMPrincipal.EjecutarSQL('INSERT INTO MOV_STOCK …') except end;` descarta cualquier falla. `MOV_STOCK` no aparece en `legado/db/esquema-dbisam.sql`, así que la trazabilidad puede estar fallando siempre sin que nadie lo vea. |
| AU-16 | En diciembre, con Navidad activa, se graba un 2% de descuento que no se mostró en pantalla | legado/src/UFrmPedidos.pas:304-305; legado/src/UGlobales.pas:33 | Regla de negocio | Baja | Descartado | `if gDescNavidad and (MonthOf(vFecha) = 12)` nunca se cumple. La única asignación de `gDescNavidad` en `legado/src` es `gDescNavidad := False; // promo navidad 2009` (UGlobales.pas:33), y `DESC_NAVIDAD` de PARAMETROS no se lee (ver AU-12). Es código muerto, no una diferencia real de totales. La diferencia real es la de AU-13. |

Otros puntos que el subagente reportó fuera de la tabla, no clasificados: tope de 40 líneas y precio mayor que 0 solo en la UI (UFrmPedidos.pas:214, 221), `FloatToStr` dentro del SQL (UFrmPedidos.pas:271, 320), `DaysBetween` sin signo (UFrmPedidos.pas:112), el flag de cliente VIP depende del nivel declarado al entrar (UFrmClientes.pas:143), `qryGeneral` y `qryAux` compartidas (UDMPrincipal.pas:94).

## Cruce con el diagnóstico de la U4

- **Coinciden:**
  - AU-01, AU-02, AU-03 y AU-04 con RS-02.
  - AU-05 con RS-01.
  - AU-07 con RS-03.
  - AU-08 y AU-09 con RS-04.
  - AU-11 con RS-08.
  - AU-12 con RS-09.
  - AU-13 con RS-05.
  - AU-14 y AU-15 con RS-07.
- **Nuevos en esta auditoría:**
  - AU-10: el bloqueo por deuda se evita editando el maestro de clientes.
  - AU-01 agrega a RS-02 que `ValidaRut` no frena la inyección.
  - AU-07 agrega que `UFrmPedidos` no revisa ningún nivel.
  - AU-08 agrega la falta de `try/except` al grabar.
- **De la U4 que el subagente no vio:**
  - RS-06: validadores de RUT incompatibles y la k minúscula.
  - RS-11: años de 2 dígitos y fecha tomada del reloj del PC.
  - RS-10 y RS-12 solo aparecieron entre los puntos que quedaron fuera de la tabla.
