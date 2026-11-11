# HU-02 · Detalle de un pedido

**Historia:** Como vendedor, quiero ver el detalle de un pedido (cliente, líneas, total y estado), para responderle al cliente sin entrar al sistema legado.

## Qué devuelve

La ruta nueva `GET /api/pedidos/{id}/detalle` devuelve un JSON con:

- Del pedido: `id`, `folio`, `fecha`, `estado` y `sucursal`.
- Del cliente: `clienteId` y `clienteNombre`.
- `lineas`: cada una con `numero`, `producto`, `cantidad`, `precioUnitario` y `subtotal` (`cantidad × precioUnitario`).
- `total`: la suma de los subtotales de las líneas.

Los montos son pesos chilenos (CLP), `decimal` sin decimales, **sin IVA**.

## Criterios de aceptación

- **CA-01 · Total desde las líneas.** Dado el pedido 8, cuando pido su detalle, entonces el `total` es 120100 y es igual a la suma de los subtotales de sus 3 líneas.
- **CA-02 · Contenido del detalle.** Dado el pedido 8, cuando pido su detalle, entonces trae el folio `PED-STGO-000008`, el estado `Entregado`, el cliente `C-001` (`Ferretería El Clavo`) y sus 3 líneas ordenadas por número.
- **CA-03 · Pedido inexistente.** Dado un id que no existe (por ejemplo 999), cuando pido su detalle, entonces la API responde 404.

## Restricciones

- La ruta `GET /api/pedidos/{id}` **no cambia**.
- La lógica (total y armado del detalle) vive en `PedidosWeb.Core`, con tests xUnit. La API solo la expone.
- Se construye en 3 incrementos, con un commit por incremento.

## Fuera de alcance

- IVA, descuentos y recargos (llegan con la migración del cálculo de totales, en la Unidad 5).
- Formato de moneda para mostrar en pantalla.
- Tests de integración HTTP: el incremento 3 se verifica en el navegador.
