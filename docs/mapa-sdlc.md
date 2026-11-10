# Mapa de uso de IA en el SDLC · Pedidos Web

> Paso 4 del taller de la Unidad 1. Para cada etapa: qué tarea concreta se delega a Claude,
> qué artefacto queda versionado, quién decide y qué revisa, qué puede salir mal y en qué evento de Scrum ocurre.
> Modelo: el caso de Valentina y TurnoVet (`caso-scrum-y-sdd.pdf`).

**Autor:** Manuel Vergara
**Fecha:** 2026-11-09

| Etapa | Se delega a Claude | Artefacto versionado | Revisa una persona | Riesgo | Evento Scrum |
|---|---|---|---|---|---|
| Requisitos | Extraer de las pantallas o el código Delphi de Pedidos Legado las reglas del folio y listar las ambigüedades como preguntas abiertas | `docs/specs/folio.md` (tabla R1–R5 + ejemplos) | **Product Owner**, con un usuario clave del legado. Decide qué reglas entran y responde las preguntas abiertas. Revisa cada regla contra folios reales de Pedidos Legado (por ejemplo, `PED-STGO-000123`) | Inventar una regla que el legado no tiene (por ejemplo, aceptar `Ñ` o un largo distinto de 2–4 letras) | Refinamiento del backlog |
| Diseño | Proponer dos alternativas de firma y ubicación (`FolioFormatter` estático vs. value object `Folio`) con sus pros y contras | `docs/adr/0001-formato-folio.md` | **Developers**, que eligen la alternativa y firman el ADR. Revisan que `PedidosWeb.Core` siga sin dependencias de infraestructura y que la firma calce con la del README | Sobrediseñar o meter dependencias de infraestructura en `PedidosWeb.Core` | Sprint Planning |
| Código | Implementar `FormatearFolio()` a partir de la spec y de los tests ya escritos | `src/PedidosWeb.Core/Folios/FolioFormatter.cs` | **Developer dueño del ítem**. Lee el diff línea por línea. Revisa el orden de validación (R1 antes que R3), el rango `'A'..'Z'` en vez de `char.IsLetter` y que no haya `double`, `float` ni NuGet nuevos | Un folio distinto al del legado hace que los clientes no encuentren sus pedidos antiguos; validar R3 con `char.IsLetter` deja pasar `Ñ` y tildes | Durante el sprint (Daily) |
| Pruebas | Proponer los casos borde que faltan (R3 con `Ñ`, dígitos y `null`; orden de validación de R5) a partir de la tabla de ejemplos | `tests/PedidosWeb.Core.Tests/Folios/FolioFormatterTests.cs` | **Developer distinto del que escribió el código**. Revisa que haya al menos un test por regla R1–R5 y por cada fila de la tabla de ejemplos del README. Rompe a propósito una regla para ver que algún test falle | Tests que confirman el código en vez de la regla: si Claude escribe código y tests a la vez, ambos pueden compartir el mismo error | Durante el sprint, antes de cerrar el ítem |
| Revisión y despliegue | Resumir el diff y revisarlo contra `AGENTS.md` y la Definition of Done (sin `double`, un test por regla, sin NuGet nuevos) | Descripción del PR y salida de `dotnet test` | **Revisor del PR (otro Developer)**. Aprueba y hace el merge (regla 3: commit, push y merge los hace una persona). Lee el diff completo, no solo el resumen, y ejecuta `dotnet build` y `dotnet test` en su máquina | Aprobar un diff que nadie entendió, confiando en el resumen de la IA | Antes de la Sprint Review |
| Mejora continua | Comparar las métricas de la ronda A y la ronda B y proponer cambios a `AGENTS.md` según los errores que se repitieron | `docs/retro-u1.md` (el cambio a `AGENTS.md` lo hace una persona, regla 6) | **Todo el Scrum Team, facilitado por el Scrum Master**. El equipo decide qué propuestas pasan a `AGENTS.md`. Revisa que cada regla nueva responda a un error observado y no a una suposición | Sacar conclusiones con una sola muestra, o agregar reglas nuevas que nadie valida | Sprint Retrospective |

## Lo que no se delega a Claude

Sale de `AGENTS.md` y se aplica en todas las etapas:

- **Commit, push y merge.** Los hace siempre una persona, después de leer el diff (regla 3).
- **Agregar paquetes NuGet.** Claude tiene que preguntar antes (regla 5).
- **Editar `AGENTS.md`, `.idea/` o archivos fuera del repo.** Claude solo trabaja en `src/`, `tests/` y `docs/` (regla 6).
- **Decidir reglas del negocio que no están escritas.** Claude pregunta y no supone (regla 8).
- **Dar por terminado un ítem.** Solo cuando una persona leyó y entendió el diff (Definition of Done).

## Evidencia del paso 3

> Ronda A: implementas `FormatearFolio()` y los tests sin IA (la completa el autor con sus mediciones).
> Ronda B: Claude Code escribió en el mismo paso los tests de todas las filas de la tabla de ejemplos del README y la función (no vio los tests nuevos en rojo antes de implementar).
> Resultado de la ronda B: 24 tests, 0 warnings, `dotnet test` en verde.

| | Sin IA | Con Claude Code |
|---|---|---|
| Tiempo (min) | _(desde que abres el archivo hasta `dotnet test` en verde)_ | ~1 (14:41:54 a 14:42:35), sin contar la lectura del README ni la revisión humana del diff, que todavía falta |
| Errores encontrados | _(tests que fallaron o casos del README que olvidaste: por ejemplo, la `Ñ` o el orden de R5)_ | Ninguno al ejecutar: 24 de 24 en verde al primer intento. Los riesgos conocidos se evitaron: R3 usa el rango `'A'..'Z'` y no `char.IsLetter`, y R1 se valida antes que la sucursal (R5) |
| Qué tuve que corregir | _(qué cambiaste para pasar a verde)_ | Nada en el código. Ojo: Claude escribió el código y los tests en la misma sesión (riesgo de la fila "Pruebas"), así que falta que una persona los revise contra el README |
