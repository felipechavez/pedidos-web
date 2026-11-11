# HU-02 · Plan de implementación

> Plan de la spec `spec.md` de esta carpeta. Tres incrementos, un commit por incremento (lo hace una persona).

## Decisiones

- El `total` del detalle se calcula desde las líneas; no se usa `Pedido.Total`.
- Las líneas del detalle son un record aparte (`LineaDetalle`): llevan `subtotal` y no llevan `pedidoId`.
- Pedido existente sin líneas: detalle normal con `lineas` vacía y `total` 0.
- Core devuelve `null` si el pedido no existe; la API lo traduce a 404.
- Sin paquetes NuGet nuevos. `GET /api/pedidos/{id}` no cambia.

## Incremento 1 · Total desde las líneas

| Qué | Dónde |
|---|---|
| `LineaDetalle` (numero, producto, cantidad, precioUnitario, subtotal) | `src/PedidosWeb.Core/Pedidos/LineaDetalle.cs` |
| `CalculoTotales.CalcularSubtotal` y `CalcularTotal` | `src/PedidosWeb.Core/Pedidos/CalculoTotales.cs` |
| Tests: subtotal, **CA-01** (pedido 8 = 120100), sin líneas = 0 | `tests/PedidosWeb.Core.Tests/Pedidos/CalculoTotalesTests.cs` |

Commit sugerido: `feat(HU-02): calcula subtotal y total desde las líneas`

## Incremento 2 · Armado del detalle

| Qué | Dónde |
|---|---|
| `DetallePedido` (datos del pedido, cliente, líneas y total) | `src/PedidosWeb.Core/Pedidos/DetallePedido.cs` |
| `DetallePedidoServicio.ObtenerDetalle(id)` | `src/PedidosWeb.Core/Pedidos/DetallePedidoServicio.cs` |
| Tests: **CA-02** (pedido 8), **CA-03** (id 999 → null), sin líneas | `tests/PedidosWeb.Core.Tests/Pedidos/DetallePedidoServicioTests.cs` |

Commit sugerido: `feat(HU-02): arma el detalle del pedido en Core`

## Incremento 3 · Exponer en la API

- En `src/PedidosWeb.Api/Program.cs`: registrar `ILineaPedidoRepositorio` y `DetallePedidoServicio`, y mapear `GET /api/pedidos/{id:int}/detalle` (200 o 404).
- Sin tests HTTP (fuera de alcance): se verifica en el navegador.

Commit sugerido: `feat(HU-02): expone GET /api/pedidos/{id}/detalle`

## Verificación

1. `dotnet test` en verde al cierre de cada incremento.
2. En el navegador (`dotnet run --project src/PedidosWeb.Api`):
   - `/api/pedidos/8/detalle` → 200, `total` 120100, 3 líneas con `subtotal`.
   - `/api/pedidos/999/detalle` → 404.
   - `/api/pedidos/8` → igual que antes.
