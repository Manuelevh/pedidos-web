# Plan: HU-02 · Buscar pedido por número o folio

> Spec de origen: `specs/HU-02-buscar-por-numero/spec.md` (APROBADA 9-nov-2026) · Autor: agente (Claude Code)
> · Revisado por: PO / dev
> Estado: aprobado por el PO el 9-nov-2026, en la misma instrucción que pidió implementar
> · Implementado el 9-nov-2026 (72 tests en verde)

## Enfoque
Se extiende lo que ya hizo HU-01, sin piezas nuevas de infraestructura.
1. **`ConsultaPedidos`** suma `Numero` como último parámetro, con valor por defecto `null`, para no romper
   a quien ya la usa.
2. **`ListadoPedidos`** interpreta `numero` (RN1, RN2, RN5) con un método privado `LeerNumero`, que devuelve un
   `Id` o un folio normalizado, o registra el error en `errores["numero"]`. Después filtra con Y junto al resto
   (RN3, RN4, RN6).
3. **`Program.cs`** agrega `string? numero` al endpoint `GET /api/pedidos` y lo pasa a la consulta.

## Decisiones técnicas
- El folio se normaliza con `FolioFormatter.FormatearFolio(numero, sucursal)`, que ya existe, y se compara con
  `Pedido.Folio` de forma exacta (RN3). Descartado comparar por partes: duplicaría las reglas del folio.
- Primero se pasa a mayúsculas con `ToUpperInvariant` y se quitan los espacios de los extremos. Después se usa la
  expresión regular `^PED-([A-Z]{2,4})-([0-9]+)$`, con `[0-9]` y no `\d` (que acepta dígitos de otros alfabetos),
  y sin `IgnoreCase`, para que `Ñ` no pase como letra válida.
- Número: solo `[0-9]+`, convertido con `int.TryParse(NumberStyles.None, InvariantCulture)`. El valor debe estar
  entre 1 y 999999. Un número tan largo que no cabe en `int` también es error 400.
- Número sin folio: se filtra por `Pedido.Id`. Es el mismo número que el correlativo del folio en los datos.
- Sin clases nuevas ni paquetes NuGet, y sin tocar `.sln` ni `.csproj`.

## Archivos afectados

| Archivo | Nuevo / Modificado | Para qué |
|---|---|---|
| `src/PedidosWeb.Core/Pedidos/ConsultaPedidos.cs` | Modificado | Parámetro `Numero` |
| `src/PedidosWeb.Core/Pedidos/ListadoPedidos.cs` | Modificado | `LeerNumero` y filtro por id o folio (RN1 a RN6) |
| `src/PedidosWeb.Core/Folios/FolioFormatter.cs` | Usado, no modificado | Normalizar el folio |
| `src/PedidosWeb.Api/Program.cs` | Modificado | Parámetro `numero` en `GET /api/pedidos` |
| `tests/PedidosWeb.Core.Tests/Pedidos/ListadoPedidosTests.cs` | Modificado | HU-02 CA1 a CA10 |
| `tests/PedidosWeb.Api.Tests/ListarPedidosEndpointTests.cs` | Modificado | HU-02 CA11 (400) |
| `README.md`, `specs/README.md` | Modificado | Estado y ejemplos |

## Trazabilidad
| Criterios | Dónde |
|---|---|
| CA1 a CA10 | `ListadoPedidosTests`, tests con nombre `Listar_HU02_CAx_...` |
| CA11 | `ListarPedidosEndpointTests`, un `[Theory]` con los 7 valores inválidos |

## Riesgos

| Riesgo | Mitigación |
|---|---|
| Confundir los CA de HU-01 y HU-02 en el mismo archivo de tests. | Prefijo `HU02` en el nombre de cada test nuevo. |
| Con datos reales, el `Id` podría no ser el correlativo del folio. | Hoy coinciden en los 45 pedidos. Si cambia en la Unidad 5 (PostgreSQL), se revisa RN1 con el PO. |

## Pasos
1. `ConsultaPedidos` y `LeerNumero` con el filtro, más los tests CA1 a CA10 → en verde.
2. Endpoint y test CA11 → en verde.
3. Probar a mano con la API levantada y actualizar los README.

## Fuera del plan
Búsqueda parcial, varios números a la vez y otros endpoints (sección «Fuera de alcance» de la spec).
