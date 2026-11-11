# HU-02 · Detalle de un pedido

**Historia:** Como vendedor, quiero ver el detalle de un pedido (cliente, líneas, total y estado), para responderle al cliente sin entrar al sistema legado.

## Problema

Cuando un cliente pregunta por su pedido, el vendedor tiene que abrir Pedidos Legado (Delphi + DBISAM) para ver qué productos lleva. La API de Pedidos Web solo entrega la cabecera (`GET /api/pedidos/{id}`): folio, cliente, estado y total, pero no las líneas. Sin las líneas, el portal no alcanza para responder.

## Resultado esperado

Con una sola consulta (`GET /api/pedidos/{id}/detalle`) el vendedor ve el pedido completo: folio, fecha, estado, sucursal, cliente, cada línea con su subtotal y el total. Ya no necesita el legado para responder a un cliente sobre un pedido. Por ejemplo, el pedido 8 muestra sus 3 líneas y un total de $120.100.

## Por qué ahora

- En la Unidad 3 llegan las líneas de los 45 pedidos (`src/PedidosWeb.Core/Lineas/`): por primera vez hay datos para armar el detalle.
- Calcular el total desde las líneas deja lista la base para migrar el cálculo de totales del legado (IVA, descuentos y recargos) en la Unidad 5.

## Alcance

- Entra:
  - Ruta nueva `GET /api/pedidos/{id}/detalle` con datos del pedido, del cliente, las líneas con su subtotal y el total.
  - Total calculado como la suma de `cantidad × precioUnitario` de las líneas (CLP, sin decimales, sin IVA).
  - Respuesta 404 si el pedido no existe.
  - Lógica en `PedidosWeb.Core` con tests xUnit.
- No entra:
  - Cambios en `GET /api/pedidos/{id}`.
  - IVA, descuentos y recargos (Unidad 5).
  - Formato de moneda para mostrar en pantalla.
  - Tests de integración HTTP.

## Preguntas abiertas

- Ninguna. El pedido sin líneas (lista vacía y total 0) y el pedido inexistente (Core devuelve `null`, la API responde 404) quedaron resueltos en las decisiones de `plan.md`.
