# Pedidos Web

Portal web de pedidos de la distribuidora. Es la modernización de **Pedidos Legado**, un sistema de escritorio hecho en Delphi con DBISAM que la empresa usa hace 18 años.

Este repositorio es el **caso hilo** del curso *Desarrollando con IA* (Vela LATAM). Crece unidad a unidad: la biblioteca de dominio (Unidad 1), una API en ASP.NET Core con 45 pedidos de ejemplo (Unidad 2), las líneas de cada pedido y un `ClientesService` heredado (Unidad 3), el código de Pedidos Legado para diagnosticarlo sin tocarlo (Unidad 4), el golden master para migrar el cálculo de totales (Unidad 5) y, en la **Unidad 6**, el **pipeline de CI** con Claude diagnosticando las fallas.

> Todo el contenido es ficticio y tiene fines educativos.

## Stack

| Pieza | Versión | Para qué |
|---|---|---|
| .NET SDK | 10 | Compilar y ejecutar |
| C# | el que trae el SDK | Lenguaje del proyecto |
| xUnit | 2.9 | Tests |
| JetBrains Rider | la versión vigente | IDE del curso |
| Claude Code | la versión vigente, con tu Claude Pro | Agente de código (lee `AGENTS.md`) |
| GitHub Actions | runners estándar (gratis en repos públicos) | CI en cada push y PR |
| Delphi 7 + DBISAM 4 | solo lectura · no se instala | El sistema legado que migras por partes |

## Estructura

```
pedidos-web/
├── AGENTS.md                         ← el tuyo (lo escribiste en la U2)
├── PedidosWeb.sln
├── .claude/                          ← U4: regla deny y subagente (U5: el hook)
├── .github/workflows/                ← U6: lo creas tú en la etapa 2 (ci.yml y claude.yml)
├── src/
│   ├── PedidosWeb.Core/              ← lógica de dominio (U5: Totales/)
│   └── PedidosWeb.Api/               ← U2: API REST (Minimal API)
├── tests/
│   └── PedidosWeb.Core.Tests/        ← U6: aquí suman tus tests de borde
├── legado/                           ← U4: código de Pedidos Legado, solo lectura
├── migracion/                        ← U5: el golden master
├── docs/
│   ├── diagnostico/                  ← U4: tu diagnóstico
│   ├── migracion/                    ← U5: defectos y plan de migración
│   └── calidad/                      ← U6: 1 plantilla                                  (nuevo)
│       └── registro-u6.md
└── specs/                            ← U2: una carpeta por historia
```

## Cómo empezar la Unidad 6

### A. Ya tienes tu repo `pedidos-web` (lo normal)

**No copies la carpeta completa**: pisarías tu `README.md`, tu `AGENTS.md`, tu código y tu trabajo de las unidades anteriores. La U6 solo **suma** una carpeta: `docs/calidad/`. No reemplaza nada. Los workflows de `.github/workflows/` los creas tú en el taller.

La forma recomendada es pedírselo a Claude Code, abierto en tu `pedidos-web`:

> Trae a pedidos-web solo lo nuevo de la Unidad 6 desde el repo de material (docs/calidad/). No toques nada más y explícame cada paso.

Si prefieres hacerlo tú, trabaja en la carpeta que contiene tu `pedidos-web` y el material del curso, uno al lado del otro:

```bash
# macOS, Linux o WSL
cd material-curso-ia-desarrolladores && git pull && cd ..
U6=material-curso-ia-desarrolladores/unidad-6/taller/pedidos-web
cp -r $U6/docs/calidad pedidos-web/docs/
```

```powershell
# Windows PowerShell
cd material-curso-ia-desarrolladores; git pull; cd ..
$U6 = "material-curso-ia-desarrolladores/unidad-6/taller/pedidos-web"
Copy-Item -Recurse "$U6/docs/calidad" pedidos-web/docs/
```

**No se copian:** `README.md`, `AGENTS.md`, `PedidosWeb.sln`, `src/`, `tests/`, `legado/`, `migracion/`, `specs/`, `.claude/` ni el resto de `docs/`.

Comprueba y guarda el punto de partida:

```bash
cd pedidos-web
dotnet test
git add .
git commit -m "chore: base de la Unidad 6 (registro de calidad)"
git push
```

**Resultado esperado:** `dotnet test` da el mismo resultado que al terminar la U5. La preparación no suma código C#.

**El CI parte en verde.** Si algo de la U5 quedó pendiente (por ejemplo, `GoldenMasterTests` en rojo porque `CalculoTotales` no está terminado), pídele a Claude Code que marque esos tests con `Skip` y un motivo. **No se borran**: quedan registrados como pendientes.

> Marca con Skip, y el motivo «Pendiente U5», los tests que hoy fallan porque esa parte no está terminada. No borres ni cambies ningún test.

### B. No tienes tu repo de las unidades anteriores

Sigue el paso 1 del README de `unidad-1/taller/pedidos-web` (crear tu repo **público** en GitHub), pero copia esta carpeta (`unidad-6/taller/pedidos-web`) en vez de la de la Unidad 1. En este camino el `AGENTS.md` viene vacío, no hay `Totales/` y los 3 tests de `FolioFormatterTests` fallan con `NotImplementedException`: márcalos con `Skip` («Pendiente U1») antes del primer push.

---

## Taller de la Unidad 6: lo que encuentras en el repo

La misión: **que ningún cambio llegue a `main` sin pruebas.** Trabajo individual, 80 minutos.

| Etapa | Min | Dónde queda tu trabajo |
|---|---|---|
| Preparación | 5 | `docs/calidad/registro-u6.md` en tu repo y `dotnet test` sin rojos |
| 1 · Tests que sí prueban | 20 | Tests de borde en `tests/PedidosWeb.Core.Tests/`, cada uno probado rompiendo el código a mano |
| 2 · Pipeline seguro | 25 | `.github/workflows/ci.yml` (verde en Actions) y `.github/workflows/claude.yml` en `main` |
| 3 · La falla | 25 | Un PR con la falla, el diagnóstico de `@claude`, tu corrección y el merge en verde |
| Cierre | 5 | — |

La guía del taller, con todo el recorrido, está en el [sitio del curso](https://ffelipecuevasc.github.io/curso-ia-desarrolladores/) → Unidad 6.

### Reglas del taller

- **Las fallas se rompen a mano en Rider**, nunca pidiéndoselas a Claude: el hook de la U5 le devolvería el error y Claude lo «arreglaría».
- **El token nunca va en un archivo.** Va como secreto del repo: `CLAUDE_CODE_OAUTH_TOKEN` en *Settings › Secrets and variables › Actions*.
- **Claude diagnostica; tú corriges y haces merge con el CI en verde.**

### La credencial de Claude (etapa 2)

1. Instala la [Claude GitHub App](https://github.com/apps/claude) solo en tu repo `pedidos-web`.
2. En tu terminal: `claude setup-token`. Te entrega un token ligado a tu Claude Pro.
3. En GitHub: *Settings › Secrets and variables › Actions › New repository secret*, con el nombre `CLAUDE_CODE_OAUTH_TOKEN`.

Cada `@claude` gasta la cuota de tu suscripción. Solo puede dispararlo quien tiene permiso de escritura en tu repo, y los PR desde forks no reciben tus secretos.

### La plantilla (`docs/calidad/`)

| Plantilla | Etapas | Qué completas |
|---|---|---|
| `registro-u6.md` | Todas | Tests revisados y borrados, el pipeline, el diagnóstico de `@claude` y tu decisión (aceptar, ajustar o rechazar) |

### Si te trabas

- **`claude.yml` no funciona:** usa la **carpeta** `unidad-6/taller/u6-respaldo` del repo de material (no es una rama). Su README explica cómo traerlo.
- **La App o el token fallan:** haz la etapa 3 en local: `dotnet test 2>&1 | claude -p "Explica en 3 líneas por qué falla"`.
- **GitHub Actions está caído:** ejecuta `dotnet build` y `dotnet test` en local y anótalo en el registro.

### Extensión opcional

Un workflow programado (`schedule`) con un `prompt` que resuma tus PR abiertos (modo automatización). En repos públicos, GitHub desactiva el schedule tras 60 días sin actividad.

## Licencias

- Claude Pro (incluye Claude Code). El token de GitHub Actions gasta tu cuota.
- JetBrains Rider y .NET 10 SDK.
- GitHub gratis con tu repo **público**: los runners estándar de Actions no tienen costo.
- No se necesita licencia de Delphi ni de DBISAM: el legado no se compila.

## Talleres anteriores

- **Unidad 1** (`FormatearFolio()` y `docs/mapa-sdlc.md`): ver el README de `unidad-1/taller/pedidos-web`.
- **Unidad 2** (API, 45 pedidos y HU-01 listar pedidos): ver el README de `unidad-2/taller/pedidos-web`.
- **Unidad 3** (HU-02 detalle del pedido, refactor de `ClientesService` y Skill `documentar-modulo`): ver el README de `unidad-3/taller/pedidos-web`.
- **Unidad 4** (diagnóstico de Pedidos Legado): ver el README de `unidad-4/taller/pedidos-web`.
- **Unidad 5** (migración del cálculo de totales con golden master): ver el README de `unidad-5/taller/pedidos-web`.
