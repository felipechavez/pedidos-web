# HU-02 · Tareas

> Derivadas de `plan.md`. Marca `[x]` al terminar cada una. Un test por criterio de aceptación.
>
> Estado al 2026-11-10: los 3 incrementos están implementados y commiteados. `dotnet test`: 35 de 35 en verde. Rutas comprobadas con la API corriendo en local.

## Incremento 1 · Total desde las líneas

- [x] **T-01** · Crear el record `LineaDetalle` (numero, producto, cantidad, precioUnitario, subtotal) — `src/PedidosWeb.Core/Pedidos/LineaDetalle.cs` — cubre: CA-01
- [x] **T-02** · Test `CalcularSubtotal_CantidadPorPrecio_DevuelveProducto` — `tests/PedidosWeb.Core.Tests/Pedidos/CalculoTotalesTests.cs` — cubre: CA-01
- [x] **T-03** · Test `CalcularTotal_LineasDelPedido8_Devuelve120100IgualASumaDeSubtotales` — `tests/PedidosWeb.Core.Tests/Pedidos/CalculoTotalesTests.cs` — cubre: CA-01
- [x] **T-04** · Test `CalcularTotal_SinLineas_DevuelveCero` (borde: pedido sin líneas) — `tests/PedidosWeb.Core.Tests/Pedidos/CalculoTotalesTests.cs` — cubre: CA-01
- [x] **T-05** · Implementar `CalculoTotales.CalcularSubtotal` y `CalcularTotal` — `src/PedidosWeb.Core/Pedidos/CalculoTotales.cs` — cubre: CA-01
- [x] **T-06** · `dotnet test` en verde
- [x] **T-07** · Commit (persona): `feat(HU-02): calcula subtotal y total desde las líneas` (`1a8f8b9`)

## Incremento 2 · Armado del detalle

- [x] **T-08** · Crear el record `DetallePedido` (datos del pedido, cliente, líneas y total) — `src/PedidosWeb.Core/Pedidos/DetallePedido.cs` — cubre: CA-02
- [x] **T-09** · Test `ObtenerDetalle_Pedido8_TraeFolioEstadoClienteY3LineasOrdenadas` — `tests/PedidosWeb.Core.Tests/Pedidos/DetallePedidoServicioTests.cs` — cubre: CA-02, CA-01
- [x] **T-10** · Test `ObtenerDetalle_IdInexistente_DevuelveNull` — `tests/PedidosWeb.Core.Tests/Pedidos/DetallePedidoServicioTests.cs` — cubre: CA-03
- [x] **T-11** · Test `ObtenerDetalle_PedidoSinLineas_DevuelveListaVaciaYTotalCero` (borde) — `tests/PedidosWeb.Core.Tests/Pedidos/DetallePedidoServicioTests.cs` — cubre: CA-01
- [x] **T-12** · Implementar `DetallePedidoServicio.ObtenerDetalle(id)`: líneas ordenadas por número, total desde `CalculoTotales`, `null` si el pedido no existe — `src/PedidosWeb.Core/Pedidos/DetallePedidoServicio.cs` — cubre: CA-01, CA-02, CA-03
- [x] **T-13** · `dotnet test` en verde
- [x] **T-14** · Commit (persona): `feat(HU-02): arma el detalle del pedido en Core` (`44ea633`)

## Incremento 3 · Exponer en la API

- [x] **T-15** · Registrar `ILineaPedidoRepositorio` y `DetallePedidoServicio` en DI — `src/PedidosWeb.Api/Program.cs` — cubre: CA-02
- [x] **T-16** · Mapear `GET /api/pedidos/{id:int}/detalle` (200 con el detalle o 404) — `src/PedidosWeb.Api/Program.cs` — cubre: CA-02, CA-03
- [x] **T-17** · `dotnet test` en verde
- [x] **T-18** · Verificar con `dotnet run --project src/PedidosWeb.Api`:
  - `/api/pedidos/8/detalle` → 200, `total` 120100, 3 líneas con `subtotal` — cubre: CA-01, CA-02
  - `/api/pedidos/999/detalle` → 404 — cubre: CA-03
  - `/api/pedidos/8` → igual que antes (restricción: la ruta no cambia)
- [x] **T-19** · Commit (persona): `feat(HU-02): expone GET /api/pedidos/{id}/detalle` (`2518c66`)

## Cierre

- [x] Todos los CA tienen test o verificación con evidencia.
- [ ] Definition of Done de `AGENTS.md` cumplida (falta que una persona lea y entienda el diff completo de la HU-02).

## Trazabilidad

| CA | Tareas | Test / verificación |
|---|---|---|
| CA-01 · Total desde las líneas | T-01 a T-05, T-09, T-11, T-12, T-18 | `CalcularTotal_LineasDelPedido8_Devuelve120100IgualASumaDeSubtotales`, `ObtenerDetalle_Pedido8_TraeFolioEstadoClienteY3LineasOrdenadas`, `/api/pedidos/8/detalle` → `total` 120100 |
| CA-02 · Contenido del detalle | T-08, T-09, T-12, T-15, T-16, T-18 | `ObtenerDetalle_Pedido8_TraeFolioEstadoClienteY3LineasOrdenadas`, `/api/pedidos/8/detalle` → 200 |
| CA-03 · Pedido inexistente | T-10, T-12, T-16, T-18 | `ObtenerDetalle_IdInexistente_DevuelveNull`, `/api/pedidos/999/detalle` → 404 |
