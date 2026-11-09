# Plan: HU-01 · Listar pedidos con filtros

> Spec de origen: `specs/HU-01-listar-pedidos/spec.md` (APROBADA 9-nov-2026, con RN9 y CA25 agregadas ese día)
> · Autor: agente (Claude Code) · Revisado por: PO / dev
> Estado: aprobado por el PO el 9-nov-2026 · Implementado el 9-nov-2026 (52 tests en verde)
>
> Nota de implementación: el CA1 de la spec decía que la página 1 incluía el anulado 20, pero ese pedido cae en
> la página 2. Se corrigió la spec (no el código) y queda anotado en el mismo CA1.

## Enfoque
La lógica vive en `PedidosWeb.Core`, y la API solo traduce HTTP a Core y de vuelta.
1. **`ConsultaPedidos`**: recibe los parámetros tal como llegan, todos como texto, para poder informar cada error
   de formato por su nombre (RN6, RN8).
2. **`ListadoPedidos.Listar(consulta)`**: valida (RN5, RN6, RN8, RN9) y, si todo está bien, filtra (RN1 a RN6),
   ordena (RN7) y pagina (RN8) sobre `IPedidoRepositorio.ObtenerTodos()`. Devuelve un `ResultadoListado` con la
   página o con los errores por parámetro.
3. **`GET /api/pedidos`** en `Program.cs`: arma la `ConsultaPedidos` desde la query y responde 200 con
   `PaginaPedidos` o 400 con `Results.ValidationProblem(errores)`.

## Decisiones técnicas
- Parámetros como `string?` en el endpoint, sin `int?` ni `DateOnly?`. Descartado el binding tipado: con
  `pagina=abc` ASP.NET responde 400 sin ProblemDetails por parámetro, y la spec pide ese formato.
- Validación en Core y no en el endpoint: las reglas se prueban con tests unitarios sin levantar HTTP.
- Errores como diccionario `parámetro → mensajes` dentro de `ResultadoListado`, con la misma forma que espera
  `ValidationProblem`. Descartadas las excepciones: un filtro inválido es flujo normal, no una falla.
- Nombre sin tildes (RN4): `Normalize(FormD)` y quitar marcas `NonSpacingMark`, que convierte `ñ` en `n` y
  `í` en `i`; luego `Contains` con `StringComparison.OrdinalIgnoreCase`. Solo .NET base, sin paquetes NuGet.
- Estados (RN5): lista fija `EstadosPedido.Validos` con los 5 textos de la spec y comparación `OrdinalIgnoreCase`.
- Fechas (RN6): `DateOnly.TryParseExact` con `["yyyy-MM-dd", "dd-MM-yyyy"]` y `CultureInfo.InvariantCulture`.
- RN9: cada parámetro pasa por `string.IsNullOrWhiteSpace` antes de usarse; vacío equivale a `null`.
- Orden (RN7): `OrderByDescending(Fecha).ThenByDescending(Id)`, y después `Skip/Take`. `total` se cuenta antes
  de paginar.
- Respuesta: `record PaginaPedidos(IReadOnlyList<Pedido> Items, int Total, int Pagina, int TamanoPagina)`. El JSON
  por defecto (camelCase) da `items`, `total`, `pagina` y `tamanoPagina` sin configurar nada.
- Registro: `ListadoPedidos` como singleton, igual que el repositorio, porque no guarda estado.

## Archivos afectados

| Archivo | Nuevo / Modificado | Para qué |
|---|---|---|
| `src/PedidosWeb.Core/Pedidos/EstadosPedido.cs` | Nuevo | Lista de estados válidos (RN5) |
| `src/PedidosWeb.Core/Pedidos/ConsultaPedidos.cs` | Nuevo | Parámetros crudos de la consulta |
| `src/PedidosWeb.Core/Pedidos/PaginaPedidos.cs` | Nuevo | Forma de la respuesta 200 |
| `src/PedidosWeb.Core/Pedidos/ResultadoListado.cs` | Nuevo | Página o errores por parámetro |
| `src/PedidosWeb.Core/Pedidos/ListadoPedidos.cs` | Nuevo | Validar, filtrar, ordenar y paginar (RN1 a RN9) |
| `src/PedidosWeb.Api/Program.cs` | Modificado | Registrar `ListadoPedidos` y mapear `GET /api/pedidos` |
| `tests/PedidosWeb.Core.Tests/Pedidos/ListadoPedidosTests.cs` | Nuevo | CA1 a CA19 y CA25 (lógica) |
| `tests/PedidosWeb.Api.Tests/ListarPedidosEndpointTests.cs` | Nuevo | CA20 a CA24 (400 + ProblemDetails) y contrato 200 |
| `src/PedidosWeb.Core/Pedidos/IPedidoRepositorio.cs` | Usado, no modificado | Fuente de los 45 pedidos |
| `README.md`, `specs/README.md` | Modificado | Estado de HU-01 y conteo de tests |

No se agregan paquetes NuGet ni proyectos, y no se tocan `.sln`, `.csproj` ni los datos de ejemplo.

## Trazabilidad: un test por criterio

| Criterios | Dónde | Cómo |
|---|---|---|
| CA1 a CA19, CA25 | `ListadoPedidosTests` | Con `PedidoRepositorioEnMemoria` real; compara `Total` y la lista exacta de `Id` en orden |
| CA20 a CA24 | `ListarPedidosEndpointTests` | `GET` con `WebApplicationFactory`; 400, `application/problem+json` y la clave del parámetro en `errors` |
| Contrato 200 | `ListarPedidosEndpointTests` | `GET /api/pedidos` deserializa `items`, `total`, `pagina` y `tamanoPagina` |

CA24 y otros criterios con varios casos usan un `[Theory]`: un test por criterio, con varias filas de datos.

## Riesgos

| Riesgo | Mitigación |
|---|---|
| Quitar tildes cambia `ñ` por `n` y alguien lo considera un error. | Decisión del PO (RN4); queda cubierta por CA6. |
| Comparaciones que dependen de la cultura del equipo (Rider en es-CL, CI en inglés). | `OrdinalIgnoreCase` e `InvariantCulture` en todo; nada de `ToUpper()` sin cultura. |
| El desempate por `Id` (RN7) no se prueba con los datos de ejemplo. | Se implementa igual y se deja anotado en la spec. Si se quiere probar, hace falta un repositorio falso en el test (sin tocar los datos). |
| `/api/pedidos` choca con `/api/pedidos/{id:int}`. | No choca: la restricción `:int` exige un segmento más. Lo prueban los tests existentes del detalle. |
| `pagina` muy grande (ej.: `2147483647`) desborda `Skip`. | Calcular el salto en `long` o devolver vacío si `(pagina-1)*tamanoPagina >= total`. |

## Pasos
1. `EstadosPedido`, `ConsultaPedidos`, `PaginaPedidos` y `ResultadoListado` → compila.
2. `ListadoPedidos`: filtros, orden y paginación con los tests CA1 a CA19 → `dotnet test` en verde.
3. Validaciones y RN9 en `ListadoPedidos`, con el test CA25 → en verde.
4. Endpoint en `Program.cs` con los tests de API (CA20 a CA24 y contrato) → en verde.
5. Probar a mano con `dotnet run --project src/PedidosWeb.Api` los ejemplos de la spec y actualizar los README.

## Fuera del plan
Crear, editar o anular pedidos; base de datos real; filtros por sucursal, por total o por varios estados;
otro orden; autenticación; interfaz web (sección «Fuera de alcance» de la spec).
