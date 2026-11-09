# HU-01 · Listar pedidos con filtros

Estado: APROBADA por el PO el 9-nov-2026.

## Historia de usuario
Como vendedor, quiero listar pedidos filtrando por cliente, estado y rango de fechas.

## Objetivo
Que el vendedor encuentre rápido los pedidos que le interesan sin revisar uno por uno. La API expone un
listado paginado de pedidos con filtros opcionales y combinables por cliente (código o nombre), estado y rango
de fechas, ordenado del más reciente al más antiguo, sobre los 45 pedidos de ejemplo en memoria.

## Contrato
`GET /api/pedidos?clienteId=&cliente=&estado=&desde=&hasta=&pagina=&tamanoPagina=`

| Parámetro | Tipo | Significado |
|---|---|---|
| `clienteId` | texto | Código exacto del cliente (RN3). |
| `cliente` | texto | Parte del nombre del cliente (RN4). |
| `estado` | texto | Un estado válido (RN5). |
| `desde`, `hasta` | fecha | Rango inclusivo; van juntos (RN6). |
| `pagina` | entero | Página pedida, desde 1. Por defecto 1 (RN8). |
| `tamanoPagina` | entero | Pedidos por página, de 1 a 100. Por defecto 20 (RN8). |

Respuesta 200:
```json
{ "items": [ /* pedidos */ ], "total": 45, "pagina": 1, "tamanoPagina": 20 }
```
- `items`: pedidos de la página, con los campos de `Pedido` (`id`, `folio`, `clienteId`, `clienteNombre`,
  `fecha`, `estado`, `total`, `sucursal`), igual que el detalle `GET /api/pedidos/{id}`.
- `total`: cantidad de pedidos que cumplen los filtros, antes de paginar.

## Reglas de negocio
- RN1. Todos los filtros son opcionales. Sin filtros, se listan todos los pedidos, incluidos los `Anulado`.
- RN2. Si vienen varios filtros, un pedido aparece solo si cumple todos (Y).
- RN3. `clienteId`: coincidencia exacta con `ClienteId` (ej.: `C-001`). Un código que no existe no es error:
  devuelve 200 con `items` vacío y `total` 0.
- RN4. `cliente`: el nombre del cliente contiene el texto, ignorando mayúsculas y tildes (incluida la `ñ`, que
  se compara como `n`). Ej.: `botilleria` encuentra «Botillería Los Andes»; `DONA ROSA`, «Minimarket Doña Rosa».
- RN5. `estado`: solo se aceptan `Pendiente`, `Confirmado`, `Despachado`, `Entregado` y `Anulado`, sin
  distinguir mayúsculas (`entregado` vale). Un solo estado por consulta. Otro texto es error 400.
- RN6. `desde` y `hasta`: inclusivos y obligatorios juntos (si viene uno solo, error 400).
  - Formatos aceptados: `yyyy-MM-dd` (2026-10-01) y `dd-MM-yyyy` (01-10-2026). Otro formato es error 400.
  - Si `desde` es posterior a `hasta`, error 400.
- RN7. Orden: `Fecha` descendente; si empatan, `Id` descendente. Se ordena antes de paginar.
- RN8. Paginación: `pagina` ≥ 1 (por defecto 1) y `tamanoPagina` de 1 a 100 (por defecto 20). Fuera de rango,
  error 400. Una página más allá de la última responde 200 con `items` vacío y el `total` real.
- RN9. Un parámetro vacío o solo con espacios (ej.: `estado=`) se trata como no enviado. Agregada por el PO
  el 9-nov-2026, al preparar el plan.

## Criterios de aceptación
Datos: `PedidoRepositorioEnMemoria` (45 pedidos, 2-sep a 31-oct de 2026). Orden completo esperado (RN7):
21, 42, 2, 23, 44, 4, 25, 6, 27, 8, 29, 10, 31, 12, 33, 14, 35, 16, 37, 18, 39, 20, 41, 1, 22, 43, 3, 24, 45,
5, 26, 7, 28, 9, 30, 11, 32, 13, 34, 15, 36, 17, 38, 19, 40.

Filtros y orden
- [x] CA1. Sin parámetros: `total` 45, `pagina` 1, `tamanoPagina` 20; `items` son los 20 primeros del orden
      completo (21 … 18), incluidos los anulados 2 y 29. (Corregido al implementar: decía también el 20,
      pero ese pedido cae en la página 2.)
- [x] CA2. `clienteId=C-001`: `total` 5; `items` 8, 16, 24, 32, 40.
- [x] CA3. `clienteId=C-999`: 200, `total` 0, `items` vacío.
- [x] CA4. `cliente=Clavo`: los mismos 5 pedidos de C-001.
- [x] CA5. `cliente=botilleria`: `total` 6; `items` 42, 2, 10, 18, 26, 34 (ignora mayúsculas y tildes).
- [x] CA6. `cliente=DONA ROSA`: `total` 6, todos de C-002 (la `ñ` se compara como `n`).
- [x] CA7. `estado=Entregado`: `total` 15; `items` 42, 44, 4, 6, 8, 31, 33, 35, 22, 24, 26, 13, 15, 17, 40.
- [x] CA8. `estado=entregado`: mismo resultado que CA7.
- [x] CA9. `estado=Anulado`: `total` 5; `items` 2, 29, 20, 11, 38.
- [x] CA10. `desde=2026-10-01&hasta=2026-10-31`: `total` 23; `desde=2026-09-01&hasta=2026-09-30`: `total` 22.
- [x] CA11. `desde=2026-09-02&hasta=2026-09-02`: solo el pedido 40 (límites inclusivos).
- [x] CA12. `desde=01-10-2026&hasta=31-10-2026`: mismo resultado que `2026-10-01` a `2026-10-31`.
- [x] CA13. `clienteId=C-001&estado=Entregado`: `items` 8, 24, 40.
- [x] CA14. `clienteId=C-001&desde=2026-10-01&hasta=2026-10-31`: `items` 8, 16.
- [x] CA15. `clienteId=C-001&cliente=Doña Rosa`: `total` 0 (filtros en Y).

Paginación
- [x] CA16. `pagina=2`: `items` 39, 20, 41, 1, 22, 43, 3, 24, 45, 5, 26, 7, 28, 9, 30, 11, 32, 13, 34, 15.
- [x] CA17. `pagina=3`: `items` 36, 17, 38, 19, 40; `total` 45.
- [x] CA18. `pagina=5`: 200, `items` vacío, `total` 45.
- [x] CA19. `tamanoPagina=100`: los 45 pedidos en una página.

Errores (400 con ProblemDetails)
- [x] CA20. Solo `desde=2026-10-01` o solo `hasta=2026-10-31`.
- [x] CA21. `desde=2026-10-31&hasta=2026-10-01` (fechas invertidas).
- [x] CA22. `desde=2026/10/01&hasta=2026-10-31` (formato no aceptado).
- [x] CA23. `estado=Cancelado` (estado no válido).
- [x] CA24. `pagina=0`, `tamanoPagina=0` o `tamanoPagina=101`.

Parámetros vacíos
- [x] CA25. `estado=&cliente=%20` responde igual que sin parámetros (CA1). `desde=&hasta=2026-10-31` es error
      400, porque solo queda `hasta` (CA20).

Nota: los datos no tienen fechas repetidas, así que el desempate por `Id` (RN7) no se puede verificar con ellos.

## Ejemplos de entrada y salida
| Entrada | Salida |
|---|---|
| `GET /api/pedidos` | 200 · `total` 45 · `items` 21 … 18 (20) |
| `GET /api/pedidos?clienteId=C-001&estado=Entregado` | 200 · `total` 3 · `items` 8, 24, 40 |
| `GET /api/pedidos?cliente=clavo&desde=01-10-2026&hasta=31-10-2026` | 200 · `total` 2 · `items` 8, 16 |
| `GET /api/pedidos?clienteId=C-999` | 200 · `total` 0 · `items` [] |
| `GET /api/pedidos?pagina=5` | 200 · `total` 45 · `items` [] |
| `GET /api/pedidos?desde=2026-10-01` | 400 · ProblemDetails |
| `GET /api/pedidos?estado=Cancelado` | 400 · ProblemDetails |

## Errores esperados
Todos responden **400** con `ProblemDetails` de validación (RFC 9457, `Results.ValidationProblem` de
ASP.NET Core): un título y un mensaje por parámetro con problemas en `errors`. Ejemplo:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "estado": [ "El estado debe ser Pendiente, Confirmado, Despachado, Entregado o Anulado." ] }
}
```

| Caso | Parámetro en `errors` |
|---|---|
| Solo `desde` o solo `hasta` | `desde` / `hasta` |
| `desde` posterior a `hasta` | `desde` |
| Fecha con otro formato | el parámetro con problemas |
| Estado no válido | `estado` |
| `pagina` < 1 | `pagina` |
| `tamanoPagina` fuera de 1..100 | `tamanoPagina` |

No son errores: un `clienteId` inexistente, un nombre sin coincidencias y una página más allá de la última.

## Fuera de alcance
- Crear, editar o anular pedidos.
- Base de datos real (llega en la Unidad 5); se usa el repositorio en memoria.
- Filtros por sucursal o por total, y filtrar por varios estados a la vez.
- Elegir otro orden.
- Autenticación y permisos por vendedor.
- Interfaz web del portal.

## Preguntas abiertas
Ninguna. Decisiones tomadas por el PO el 9-nov-2026:
estados válidos (los 5 de los datos, uno por consulta, sin distinguir mayúsculas); anulados visibles por defecto;
nombre sin distinguir mayúsculas ni tildes; fechas `yyyy-MM-dd` o `dd-MM-yyyy`; fechas invertidas → 400;
cliente inexistente → 200 vacío; errores 400 con ProblemDetails; paginación 20 por defecto y 100 como máximo;
página fuera de rango → 200 vacío; respuesta con metadatos; ruta `GET /api/pedidos` con parámetros en español;
tests de la API en `tests/PedidosWeb.Api.Tests/`.
