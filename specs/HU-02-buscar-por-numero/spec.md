# HU-02 · Buscar pedido por número o folio

Estado: APROBADA por el PO el 9-nov-2026.

## Historia de usuario
Como vendedor, quiero buscar un pedido por su número o por su folio completo (por ejemplo `PED-ANT-000009`),
para encontrarlo rápido cuando un cliente me lo dicta o lo leo de una factura.

## Objetivo
El vendedor tiene a mano el número del pedido o el folio impreso en la factura y el correo. El listado de HU-01
suma un parámetro `numero` que acepta cualquiera de los dos, con las variantes que el vendedor suele escribir
(minúsculas, espacios, sin ceros a la izquierda), y se combina con los demás filtros.

## Contrato
`GET /api/pedidos?numero=...` es el mismo endpoint, la misma respuesta y los mismos errores de HU-01
(ver `specs/HU-01-listar-pedidos/spec.md`). El parámetro nuevo es:

| Parámetro | Tipo | Significado |
|---|---|---|
| `numero` | texto | Número del pedido (`9`) o folio (`PED-ANT-000009`). Ver RN1 a RN5. |

El detalle por id que ya existía, `GET /api/pedidos/{id}`, no cambia.

## Reglas de negocio
- RN1. `numero` acepta dos formas:
  - **Número:** solo dígitos, con valor entre 1 y 999999 (igual que la regla R1 del folio). Se aceptan ceros a la
    izquierda: `9`, `0009` y `000009` buscan el pedido con `Id` 9.
  - **Folio:** `PED-{SUCURSAL}-{NUMERO}`, donde `{SUCURSAL}` tiene de 2 a 4 letras A–Z (regla R3 del folio) y
    `{NUMERO}` es un número como el de arriba, con o sin ceros a la izquierda.
- RN2. El folio no distingue mayúsculas y se le quitan los espacios del inicio y del final:
  ` ped-ant-000009 ` vale lo mismo que `PED-ANT-000009`.
- RN3. Un folio se compara completo, después de normalizarlo al formato del legado (`PED-ANT-9` pasa a
  `PED-ANT-000009`). Si la sucursal no corresponde al número (por ejemplo `PED-STGO-000009`, si el 9 es de ANT),
  no se encuentra nada.
- RN4. Un número o folio con formato válido que no existe no es error: responde 200 con `items` vacío y `total` 0,
  igual que un `clienteId` inexistente en HU-01.
- RN5. Si `numero` no es un número ni un folio válido (por ejemplo `ABC`, `PED-X-1`, `0` o `1000000`), responde
  error 400 con ProblemDetails, con `numero` en `errors`.
- RN6. `numero` se combina con los filtros de HU-01 con Y. Vacío o solo espacios cuenta como no enviado
  (RN9 de HU-01).

## Criterios de aceptación
Datos: el pedido 9 es `PED-ANT-000009`, cliente C-006, estado Pendiente.

- [x] CA1. `numero=9`: `total` 1, `items` [9].
- [x] CA2. `numero=000009`: `total` 1, `items` [9].
- [x] CA3. `numero=PED-ANT-000009`: `total` 1, `items` [9].
- [x] CA4. `numero=%20ped-ant-000009%20` (minúsculas y espacios): `items` [9].
- [x] CA5. `numero=PED-ANT-9` (sin ceros): `items` [9].
- [x] CA6. `numero=PED-STGO-000009` (sucursal que no corresponde): 200, `total` 0.
- [x] CA7. `numero=999` y `numero=PED-ANT-000999` (no existen): 200, `total` 0.
- [x] CA8. `numero=9&estado=Pendiente`: `items` [9]. `numero=9&estado=Entregado`: `total` 0 (filtros en Y).
- [x] CA9. `numero=9&clienteId=C-001`: `total` 0 (el 9 es de C-006).
- [x] CA10. `numero=` (vacío): igual que sin parámetros (`total` 45).
- [x] CA11. 400 con `numero` en `errors` para: `ABC`, `PED-X-1` (sucursal de 1 letra), `PED-ÑUÑO-1`,
      `0`, `1000000`, `PED-ANT-0` y `PED-ANT-1000000`.

## Ejemplos de entrada y salida
| Entrada | Salida |
|---|---|
| `GET /api/pedidos?numero=9` | 200 · `total` 1 · `items` [9] |
| `GET /api/pedidos?numero=PED-ANT-000009` | 200 · `total` 1 · `items` [9] |
| `GET /api/pedidos?numero=ped-ant-9` | 200 · `total` 1 · `items` [9] |
| `GET /api/pedidos?numero=PED-STGO-000009` | 200 · `total` 0 · `items` [] |
| `GET /api/pedidos?numero=ABC` | 400 · ProblemDetails, `errors.numero` |

## Errores esperados
Solo uno nuevo: `numero` con formato no válido (RN5) → 400 con ProblemDetails, igual que en HU-01. Mensaje:
«`numero` debe ser un número entre 1 y 999999 o un folio PED-{SUCURSAL}-{NUMERO}».
No son errores: un número o folio que no existe, o un folio con una sucursal que no corresponde.

## Fuera de alcance
- Buscar por parte del número o del folio (por ejemplo, todos los que empiezan con `PED-ANT`).
- Buscar varios números a la vez.
- Cambiar `GET /api/pedidos/{id}` o agregar otro endpoint.
- Búsqueda por otros datos (RUT, monto).

## Preguntas abiertas
Ninguna. Decisiones del PO del 9-nov-2026: un solo parámetro `numero` en el listado; el folio acepta
minúsculas, espacios y no tener ceros a la izquierda; una sucursal que no corresponde no encuentra nada; un formato
inválido responde 400; el número acepta ceros a la izquierda.
