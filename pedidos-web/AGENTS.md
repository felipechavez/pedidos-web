# AGENTS.md · Pedidos Web

Instrucciones para agentes de IA que trabajan en este repositorio (Claude Code, Codex, Copilot y otros). Las personas del equipo también las siguen.

## Proyecto

Portal web de pedidos de una distribuidora. Moderniza **Pedidos Legado** (Delphi + DBISAM) hacia .NET y, desde la Unidad 5, PostgreSQL. El detalle del negocio está en `README.md`.

## Stack y comandos

- .NET 10, C#, xUnit. IDE de referencia: JetBrains Rider.
- Compilar: `dotnet build`
- Tests: `dotnet test`
- Ejecuta `dotnet test` después de cada cambio y antes de decir que terminaste.

## Reglas del equipo

1. **Idioma.** Responde, comenta el código y escribe los mensajes en español de Chile. Clases y métodos del dominio van en español (`FormatearFolio`, `CalcularTotal`).
2. **Plan antes de editar.** Antes de modificar un archivo, explica tu plan en máximo 5 viñetas y espera mi aprobación.
3. **Nada de commit, push ni merge.** Eso lo hace siempre una persona, después de leer el diff.
4. **Cada cambio de comportamiento lleva su test xUnit.** Si cambias una regla, el test que la prueba cambia en el mismo cambio.
5. **No agregues paquetes NuGet** sin preguntar primero.
6. **Solo trabajas en `src/`, `tests/` y `docs/`.** No toques la configuración del IDE (`.idea/`), ni `AGENTS.md`, ni archivos fuera del repositorio.
7. **Dinero en pesos chilenos.** Los montos son enteros: usa `decimal` sin decimales y nunca `double` ni `float`.
8. **Si una regla del negocio no está escrita, pregunta.** No la inventes ni la supongas.

## Estructura

- `src/PedidosWeb.Core/`: lógica de dominio, sin dependencias de infraestructura.
- `tests/PedidosWeb.Core.Tests/`: tests xUnit. Un archivo de tests por clase.
- `docs/`: documentación del equipo (por ejemplo, `docs/mapa-sdlc.md`).

## Estilo de código

- Identificadores del dominio en español de Chile (`FormatearFolio`, `ObtenerTodos`).
- Dinero: `decimal` (enteros CLP), nunca `double` ni `float`.
- Fechas: `DateOnly` (sin hora; zona Chile).
- Core contiene lógica de dominio pura, sin dependencias de infraestructura.
- Api contiene endpoints (minimal API) y configuración de DI.

## Tests

- Framework: xUnit 2.9. Viven en `tests/PedidosWeb.Core.Tests/`.
- Convención de nombre: `Metodo_Escenario_ResultadoEsperado`.
- Un archivo de tests por clase, espejando la estructura de carpetas de `src/`.
- Ejecutar `dotnet test` después de cada cambio.

## Seguridad

- Nunca secretos, claves ni cadenas de conexión en el código fuente.

## Reglas de Spec-Driven Development

- Toda funcionalidad parte de una spec aprobada en `specs/<historia>/spec.md`.
- Un test xUnit por criterio de aceptación.
- No hacer commit sin tests en verde.
- El agente puede correr tests, explorar archivos y compilar sin pedir permiso.
- Los errores de compilación se informan al desarrollador sin resolverlos.

## Definition of Done

- Compila sin warnings nuevos.
- `dotnet test` en verde.
- Cada regla del README tocada tiene su test.
- Una persona leyó y entendió el diff.
