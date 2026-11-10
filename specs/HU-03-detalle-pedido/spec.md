# Spec breve · HU-03 · Detalle de un pedido

> Estado: **aprobada**.

## HU-03 · Detalle de un pedido

**Historia:** Como vendedor, quiero ver el detalle de un pedido (cliente, líneas, total y estado), para responder las consultas del cliente sin abrir el sistema legado.

## Qué devuelve

La ruta `GET /api/pedidos/{id}/detalle` devuelve:

- Del pedido: `Id`, `Folio`, `Fecha`, `Estado` y `Sucursal`.
- Del cliente: `ClienteId` y `ClienteNombre`, tal como vienen en el pedido.
- De cada línea: `Numero`, `Producto`, `Cantidad`, `PrecioUnitario` y `Subtotal` (= `Cantidad × PrecioUnitario`), ordenadas por `Numero`.
- Total: la suma de los `Subtotal` de las líneas, en pesos chilenos, **sin IVA** (`decimal` sin decimales).

## Criterios de aceptación

Cada criterio se puede comprobar con un sí o un no.

- **CA-01** · Dado el pedido 8, cuando pido `GET /api/pedidos/8/detalle`, entonces responde `200` con folio `PED-STGO-000008`, cliente `C-001` «Ferretería El Clavo», estado `Entregado` y sus 3 líneas en orden 1, 2, 3.
- **CA-02** · Dado el pedido 8, cuando pido su detalle, entonces cada línea trae `Subtotal = Cantidad × PrecioUnitario` (64.500, 35.400 y 20.200) y el total es 120.100, igual a su `Pedido.Total`.
- **CA-03** · Dado un id que no existe (por ejemplo, 999), cuando pido su detalle, entonces responde `404 Not Found` con el cuerpo `{ "mensaje": "No encontramos el pedido 999. Revisa el número e intenta de nuevo." }`.
- **CA-04** · Dado un pedido que existe pero no tiene líneas, cuando pido su detalle, entonces responde `200` con la lista de líneas vacía y total 0.
- **CA-05** · Dado un pedido anulado (por ejemplo, el 2), cuando pido su detalle, entonces responde `200` con estado `Anulado`, igual que cualquier otro pedido.
- **CA-06** · Dado cualquier pedido, cuando pido `GET /api/pedidos/{id}`, entonces responde exactamente lo mismo que antes de esta historia.

## Si el pedido no existe

Responde `404 Not Found` con un mensaje claro para el vendedor, no con un error técnico:

```json
{ "mensaje": "No encontramos el pedido {id}. Revisa el número e intenta de nuevo." }
```

`{id}` es el número pedido. La pantalla que consume la API muestra ese texto. Esta regla aplica **solo** a la ruta nueva `/detalle`.

## Fuera de alcance

- Cálculo de IVA o total bruto.
- Editar el pedido o sus líneas.
- Paginar las líneas.
- Datos del cliente que no vienen en el pedido (RUT, zona, si está activo) y cualquier cambio en `ClientesService`.
- Cambios en la ruta `GET /api/pedidos/{id}`: sigue respondiendo `404` sin cuerpo cuando el pedido no existe.
