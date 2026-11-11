# Riesgos de Pedidos Legado

<!--
Etapa 2 · Encontrar.
Un riesgo es algo que puede salir mal en producción o al migrar.
- Evidencia: archivo:línea. Sin evidencia, es una sospecha, no un riesgo.
- Categoría: Seguridad · Datos e integridad · Fechas · Mantenibilidad.
- Prioridad: Alta · Media · Baja (¿qué tan grave es y qué tan probable?).
Punto de control E2: al menos 5 riesgos con evidencia y prioridad.
-->

> Hallazgos del subagente analista-legado, contrastados con el código. Lo marcado «por verificar» depende de algo que no se puede confirmar leyendo el código (configuración regional, comportamiento del motor DBISAM o la base de producción).
> Rutas relativas a `pedidos-web/`.

| ID | Riesgo | Evidencia (archivo:línea) | Categoría | Prioridad |
|---|---|---|---|---|
| RS-01 | Clave de la base de datos escrita en el código (`CLAVE_BD = 'Distrib#2008'`) | legado/src/UDMPrincipal.pas:41; legado/src/UDMPrincipal.pas:59 | Seguridad | Alta |
| RS-02 | SQL armado concatenando texto ingresado por el usuario (inyección SQL). `EscapaComillas` existe (UUtilidades.pas:164) pero nadie la llama | legado/src/UFrmClientes.pas:76-77; legado/src/UFrmInformes.pas:69; legado/src/UFrmPedidos.pas:102; legado/src/UFrmPedidos.pas:152; legado/src/UFrmPedidos.pas:259-262; legado/src/UDMPrincipal.pas:85 | Seguridad | Alta |
| RS-03 | Entrada sin contraseña: el nivel se asigna según el nombre que se escribe ("ADMIN" o "JEFEVENTAS" → nivel 9); con el campo vacío entra como VENTAS | legado/src/UFrmPrincipal.pas:46-50 | Seguridad | Alta |
| RS-04 | Grabación sin transacción (`// TODO transaccion`). Folio e ID de cliente = MAX+1, con riesgo de choque entre usuarios simultáneos. El pedido se inserta con TOTAL 0 y se actualiza al final, así que un error a mitad de camino deja un pedido con total 0 | legado/src/UFrmPedidos.pas:249-251; legado/src/UFrmPedidos.pas:255; legado/src/UFrmPedidos.pas:262; legado/src/UFrmPedidos.pas:320; legado/src/UFrmClientes.pas:160-163 | Datos e integridad | Alta |
| RS-05 | El total grabado puede ser distinto del mostrado en pantalla: `>=` contra `>`, Navidad solo al grabar y distinto origen de la categoría OFE. La diferencia solo se avisa con `gModoPrueba`, que nunca es True | legado/src/UFrmPedidos.pas:296-305; legado/src/UFrmPedidos.pas:330; legado/src/UCalculoTotales.pas:77-79; legado/src/UGlobales.pas:34 | Datos e integridad | Alta |
| RS-06 | Dos validadores de RUT incompatibles. `RutOk` (maestro de clientes) invierte los dígitos 0 y K: rechaza RUT válidos como el del cliente 7 (…05-K) y el del cliente 11 (…13-0), comprobados a mano. `ValidaRut` (pedidos) no acepta la k minúscula: el cliente 18 (`76500019-k`) no se puede buscar. Además, la búsqueda compara el RUT exactamente como se escribe | legado/src/UFrmClientes.pas:193-196; legado/src/UUtilidades.pas:73-79; legado/src/UFrmPedidos.pas:102; legado/db/datos/CLIENTES.csv:8; legado/db/datos/CLIENTES.csv:12; legado/db/datos/CLIENTES.csv:19 | Datos e integridad | Alta |
| RS-07 | Stock leído y escrito sin bloqueo; si falta stock el pedido no se revierte. Se inserta en MOV_STOCK, que no está en `esquema-dbisam.sql`, y los errores se tragan con un `except` vacío. Ya hay stock negativo en los datos (producto 9 con −12). Por verificar si MOV_STOCK existe en la base de producción | legado/src/UStock.pas:38-51; legado/src/UStock.pas:69-72; legado/src/UStock.pas:84-90; legado/db/datos/PRODUCTOS.csv:10 | Datos e integridad | Media |
| RS-08 | Clientes inactivos pueden comprar: la búsqueda del pedido no consulta ACTIVO (el cliente 30, con ACTIVO=FALSE, tiene el pedido 1020) | legado/src/UFrmPedidos.pas:99-102; legado/db/datos/CLIENTES.csv:31; legado/db/datos/PEDIDOS.csv:21 | Datos e integridad | Media |
| RS-09 | Parámetros que el código no lee y tasas fijas en el código: PEDIDO_MINIMO (25.000) y DESC_NAVIDAD nunca se leen (el mínimo es 30.000 fijo); el informe usa IVA 0,19 fijo en vez de TASA_IVA | legado/db/datos/PARAMETROS.csv:3; legado/db/datos/PARAMETROS.csv:6; legado/src/UFrmPedidos.pas:231; legado/src/UFrmInformes.pas:92 | Mantenibilidad | Media |
| RS-10 | `DaysBetween` no tiene signo: un vencimiento a más de 60 días en el futuro bloquea al cliente. Por verificar: si FECHA_VENCIMIENTO es nula con saldo > 0, ¿DBISAM devuelve 0 (30/12/1899) y bloquea? | legado/src/UFrmPedidos.pas:111-112 | Fechas | Media |
| RS-11 | Años de 2 dígitos con pivote en 50 (`dd/mm/yy`) en los filtros del informe y en el CSV para contabilidad. `StrAFecha` no maneja errores de formato. La fecha sale del reloj del PC y no considera feriados | legado/src/UUtilidades.pas:100-107; legado/src/UUtilidades.pas:112; legado/src/UUtilidades.pas:124; legado/src/UFrmInformes.pas:52-53; legado/src/UFrmInformes.pas:63-64; legado/src/UFrmInformes.pas:119; legado/src/UFrmPedidos.pas:240 | Fechas | Media |
| RS-12 | Dinero en `Double` y `FloatToStr` dentro del SQL. Por verificar: con la configuración regional chilena (coma decimal), ¿un monto con decimales rompe el SQL? Además, la lógica de totales y de redondeo está duplicada en dos unidades | legado/src/UCalculoTotales.pas:12; legado/src/UCalculoTotales.pas:19-26; legado/src/UFrmPedidos.pas:199; legado/src/UFrmPedidos.pas:271; legado/src/UFrmPedidos.pas:320; legado/src/UCalculoTotales.pas:50; legado/src/UUtilidades.pas:30 | Mantenibilidad | Media |
