# Matriz de decisión de Pedidos Legado

<!--
Etapa 3 · Decidir (modo por defecto).
1. Reparte los pesos entre los 5 criterios. Deben sumar 100 %.
2. Puntúa cada estrategia de 1 a 5 para cada módulo (5 = la mejor opción para ese criterio)
   y calcula el puntaje ponderado. Pídele a Claude que haga las cuentas, y revísalas.
3. La recomendación de cada módulo debe salir de la matriz, no de la intuición.
Punto de control E3: pesos que suman 100 %, al menos 4 módulos puntuados,
recomendación que se desprende de la matriz y resumen de 5 líneas.
-->

> Evidencia en `reglas-negocio.md` (#1–#18) y `riesgos.md` (RS-01–RS-12). Rutas relativas a `pedidos-web/`.

## Pesos

| Criterio | Qué mide | Peso |
|---|---|---|
| Costo | Cuánto cuesta en horas y dinero | 15 % |
| Complejidad | Qué tan difícil es técnicamente | 15 % |
| Riesgo | Qué puede salir mal en el camino | 30 % |
| Impacto | Cuánto valor entrega al negocio | 25 % |
| Personas | Conocimiento disponible en el equipo | 15 % |
| **Total** |  | **100 %** |

**Por qué estos pesos:**

- **Riesgo (30 %):** hay 6 riesgos Altos (total grabado ≠ mostrado, grabación sin transacción, SQL concatenado, entrada sin contraseña). Un error en un pedido es dinero mal cobrado.
- **Impacto (25 %):** el objetivo es el portal web; lo que no lo acerca puede esperar.
- **Costo y Complejidad (15 % c/u):** el legado es chico (unas 2.400 líneas) y la complejidad ya pesa en parte dentro del Riesgo.
- **Personas (15 %):** el equipo trabaja en .NET y nadie compila Delphi 7; castiga sobre todo a Refactorizar.

**Estrategias:**

- **Mantener:** se deja como está en Delphi, solo correcciones puntuales.
- **Refactorizar:** se mejora el código Delphi sin cambiar de plataforma.
- **Modernizar:** se lleva a .NET (`PedidosWeb.Core` / `Api`) conservando las reglas, con un test xUnit por regla.
- **Reemplazar:** se descarta el código y se usa una solución estándar o un diseño nuevo, sin arrastrar las reglas del legado.

**Escala (1–5, 5 = mejor opción para el criterio):** en Costo y Complejidad, 5 = más barato / más simple. En Riesgo, 5 = menos riesgo neto: el que sigue vivo (RS-xx) más el que introduce el cambio.

## Matriz

<!-- Anota el puntaje ponderado de cada estrategia (1–5 por criterio × peso). -->

| Módulo | Mantener | Refactorizar | Modernizar | Reemplazar | Recomendación |
|---|---|---|---|---|---|
| M1 Cálculo y utilidades (UCalculoTotales, UUtilidades) | 2,50 | 2,60 | **4,40** | 3,10 | **Modernizar** (primero) |
| M2 Toma y grabación de pedidos (UFrmPedidos) | 2,35 | 2,30 | **3,80** | 2,50 | **Modernizar** |
| M3 Clientes (UFrmClientes) | 2,35 | 2,75 | **4,15** | 2,85 | **Modernizar** |
| M4 Productos (UFrmProductos) | 3,35 | 2,75 | **4,05** | 3,15 | **Modernizar** (al final) |
| M5 Stock (UStock) | 2,35 | 2,30 | **3,70** | 2,95 | **Modernizar** (junto con M2) |
| M6 Informes y CSV contable (UFrmInformes) | 2,65 | 2,75 | 3,45 | **4,00** | **Reemplazar** |
| M7 Acceso, conexión y globales (UFrmPrincipal, UDMPrincipal, UGlobales) | 2,35 | 2,15 | 3,15 | **4,00** | **Reemplazar** |

Sensibilidad: con pesos iguales (20 % cada criterio) ninguna recomendación cambia.

### Detalle de puntajes (Costo · Complejidad · Riesgo · Impacto · Personas)

Puntaje ponderado = 0,15·C + 0,15·X + 0,30·R + 0,25·I + 0,15·P.

| Módulo | Mantener | Refactorizar | Modernizar | Reemplazar |
|---|---|---|---|---|
| M1 | 5·5·1·1·3 | 3·4·3·2·1 | 4·4·4·5·5 | 3·3·2·4·4 |
| M2 | 5·5·1·1·2 | 3·2·3·2·1 | 2·2·4·5·5 | 1·2·2·4·3 |
| M3 | 5·5·1·1·2 | 4·4·3·2·1 | 4·4·4·4·5 | 2·3·3·3·3 |
| M4 | 5·5·3·2·3 | 4·4·3·2·1 | 4·5·4·3·5 | 3·4·3·3·3 |
| M5 | 5·5·1·1·2 | 4·3·2·2·1 | 3·3·4·4·4 | 2·2·3·4·3 |
| M6 | 5·5·2·1·2 | 4·4·3·2·1 | 3·3·4·3·4 | 4·4·4·4·4 |
| M7 | 5·5·1·1·2 | 3·3·2·2·1 | 3·3·3·3·4 | 3·3·5·4·4 |

### Por qué cada recomendación

- **M1 Cálculo y utilidades:** 336 líneas de lógica pura, sin UI ni BD, y `CalculoTotales.cs` ya existe. Modernizar corrige RS-05 (`>` en pantalla vs `>=` al grabar) y RS-12 (dinero en `Double`). Reemplazar pierde reglas escondidas (OFE, redondeo `Trunc(x + 0.49)`).
- **M2 Toma y grabación de pedidos:** el módulo más grande (646 líneas, 12 reglas mezcladas con UI y SQL), por eso Costo y Complejidad bajos. En .NET se resuelven RS-02 y RS-04 (consultas parametrizadas, transacción, secuencias en vez de MAX+1). Mantener deja vivos tres riesgos Altos; reemplazar con un producto es caro y pierde reglas (corte 18:00, VIP con stock negativo, pedido mínimo).
- **M3 Clientes:** `ClientesService` ya está en .NET. Mantener deja `RutOk` rechazando RUT válidos (RS-06); Refactorizar es barato pero nadie trabaja en Delphi.
- **M4 Productos:** mantenedor simple sin riesgos Altos propios (Mantener queda segundo con 3,35). Gana Modernizar porque el portal necesita el catálogo (precio, categoría OFE); va al final.
- **M5 Stock:** chico, pero falta confirmar si `MOV_STOCK` existe en producción y el tope VIP de −20 con el negocio. Debe ir en la misma transacción que la grabación del pedido (RS-07), por eso se migra junto con M2.
- **M6 Informes y CSV contable:** reconstruir la grilla no aporta. Una herramienta de informes sobre PostgreSQL (U5) elimina los años de 2 dígitos (RS-11), el IVA fijo (RS-09) y el «Neto» que incluye flete. Hay que validar con contabilidad el formato del CSV.
- **M7 Acceso, conexión y globales:** modernizar el modelo actual (el nombre escrito define el nivel, RS-03) sería migrar la falla. Reemplazar por identidad estándar con roles y la clave fuera del código (RS-01). Mientras tanto, cambiar la clave `Distrib#2008` aunque siga el legado.

## Resumen ejecutivo

<!-- Exactamente 5 líneas, para alguien que no leerá el resto. -->

1. **Qué es y su estado:** Pedidos Legado es un sistema de escritorio en Delphi 7 + DBISAM usado hace 18 años; 16 archivos y unas 2.400 líneas, con 18 reglas de negocio repartidas en los formularios, sin tests ni transacciones.
2. **Hallazgo principal (con evidencia):** el total grabado puede diferir del mostrado: la grabación usa `>=` (UFrmPedidos.pas:296) y la pantalla `>` (UCalculoTotales.pas:77), y el aviso de diferencia nunca se activa (UGlobales.pas:34).
3. **Riesgo más urgente:** seguridad: clave de la base en el código (UDMPrincipal.pas:41), entrada sin contraseña (UFrmPrincipal.pas:46-50) y SQL armado con texto del usuario en 6 lugares.
4. **Estrategia y por dónde empezar:** modernizar a .NET cálculo, pedidos + stock, clientes y productos; reemplazar acceso e informes por soluciones estándar. Se empieza por el cálculo de totales (puro y ya iniciado).
5. **Próximo paso que se pide aprobar:** una historia para migrar el cálculo de totales con un test xUnit por regla (#1–#9), previa decisión del negocio sobre `>=` vs `>` y pedido mínimo $30.000 vs $25.000.
