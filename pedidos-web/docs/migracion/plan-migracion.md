# Plan de migración · Pedidos Legado → Pedidos Web

> Parte de `docs/diagnostico/matriz-decision.md` (M1–M7), `riesgos.md` (RS-xx) y `defectos-corregidos.md` (D1, H1).

## 1. Etapas

Una función a la vez (strangler fig). Cada etapa termina con el legado apagado para esa función.

1. **Cálculo de totales en C#** (M1, `src/PedidosWeb.Core/Totales/`): hecho en la Unidad 5. Golden master de 60 casos y R05 corregido (D1). Queda pendiente la decisión H1.
2. **Clientes** (M3): sobre `ClientesService`, con un solo validador de RUT (RS-06) y bloqueo de clientes inactivos (RS-08). Es de bajo riesgo y deja listo el maestro que usa la etapa 3.
3. **Toma y grabación de pedidos + stock** (M2 + M5) sobre PostgreSQL: pantalla y grabación usan el mismo `CalculoTotales` (cierra RS-05), con transacción, secuencias y consultas parametrizadas (RS-02, RS-04, RS-07). Requiere H1 decidida y el acceso de la etapa 5 listo.
4. **Productos** (M4): catálogo con precio y categoría (OFE).
5. **Reemplazos:** acceso con identidad estándar y roles (M7, RS-01 y RS-03), listo **antes** de abrir la etapa 3. Después, informes y CSV contable con una herramienta sobre PostgreSQL (M6, RS-09 y RS-11), con el formato validado por Contabilidad.

## 2. Convivencia

- Hasta la etapa 3, DBISAM es la fuente de verdad y PostgreSQL recibe una copia diaria. Desde la etapa 3, PostgreSQL manda y el legado queda en solo lectura.
- **Comparación automática:** `dotnet test` corre el golden master en cada cambio (hook y CI).
- **Conciliación en sombra:** cada pedido que graba el legado se recalcula con `CalculoTotales` y se compara con `PEDIDOS.TOTAL`. Las únicas diferencias aceptadas son las explicadas en `defectos-corregidos.md`: D1 (±2 pesos, solo con fracción ,50) y H1 (base exacta en 500.000 o 1.000.000, hasta que se decida). Cualquier otra diferencia es un incidente y frena el avance.

## 3. Riesgos

| Riesgo | Mitigación |
|---|---|
| El portal cobra distinto que el legado en pedidos con base exacta en 500.000 o 1.000.000 (H1, RS-05): 428.400 en 13 de 60 pedidos | Decisión escrita de Comercial y Contabilidad antes de la etapa 3; si cambia el contrato, se actualizan el golden master y `defectos-corregidos.md` |
| Diferencias de 1–2 pesos por D1 generan reclamos o descuadres contables | Avisar a Contabilidad antes de la etapa 3; la conciliación las lista y explica una por una |
| Pedidos a medio grabar, folios duplicados o stock inconsistente (RS-04, RS-07) | Migrar M2 y M5 juntos, en una sola transacción y con secuencias; confirmar si `MOV_STOCK` existe en producción |
| El legado sigue expuesto mientras convive (RS-01, RS-02, RS-03) | Cambiar ya la clave `Distrib#2008`; el legado solo en la red interna; el portal no se abre sin la identidad de la etapa 5 |
| Reglas sin dueño o contradictorias (pedido mínimo 30.000 vs 25.000, Navidad, VIP con stock −20) | Decisión escrita del negocio por regla antes de migrarla; un test xUnit por regla |

## 4. Criterio de corte

- **Se apaga el legado para una etapa** cuando: (a) los tests y el golden master están en verde; (b) hubo un cierre mensual completo de conciliación en sombra sin diferencias no explicadas; (c) firma el dueño del negocio de la función, y Contabilidad si toca montos; (d) las decisiones pendientes de la etapa están cerradas (H1 para la etapa 3).
- **Vuelta atrás:** durante el primer cierre mensual después de cada corte, el legado sigue instalado y el menú del portal puede devolver a los usuarios a la pantalla antigua. En la etapa 3, que cambia la fuente de verdad, se prepara y ensaya antes del corte un script que devuelve a DBISAM los pedidos grabados en PostgreSQL.
- El legado se desinstala cuando todas las etapas cumplen su corte y pasa un cierre contable sin incidentes.
