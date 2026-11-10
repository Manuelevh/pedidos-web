# specs/

Una carpeta por historia de usuario, con su `spec.md` y su `plan.md`.

| Carpeta | Unidad | Historia | Estado |
|---|---|---|---|
| `HU-01-listar-pedidos/` | U2 | Como vendedor, quiero listar pedidos filtrando por cliente, estado y rango de fechas. | Implementada: `spec.md` y `plan.md` aprobados; 25/25 criterios con test en verde |
| `HU-02-buscar-por-numero/` | U2 | Como vendedor, quiero buscar un pedido por su número o por su folio completo. | Implementada: `spec.md` y `plan.md` aprobados; 11/11 criterios con test en verde |
| `HU-03-detalle-pedido/` | U3 | Como vendedor, quiero ver el detalle de un pedido (cliente, líneas, total y estado). | Implementada: `spec.md` breve aprobada (sin `plan.md`); 6/6 criterios con test en verde |

No se escribe código sin una spec aprobada (ver `AGENTS.md`).

## Flujo
1. `spec.md` en borrador: se cierran los TODO y las preguntas abiertas con el Product Owner.
2. Spec aprobada: se cambia su estado a APROBADA.
3. `plan.md`: plan técnico en plan mode, a partir de la spec aprobada.
4. Implementación: un test xUnit por criterio de aceptación; `dotnet test` en verde antes del commit.
