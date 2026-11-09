# Mapa de uso de IA en el SDLC · Pedidos Web

> Paso 4 del taller de la Unidad 1. Para cada etapa: qué tarea concreta se delega a Claude,
> qué artefacto queda versionado, quién decide y qué revisa, qué puede salir mal y en qué evento de Scrum ocurre.
> Modelo: el caso de Valentina y TurnoVet (`caso-scrum-y-sdd.pdf`).

**Autor:** 
**Fecha:** 

| Etapa | Se delega a Claude | Artefacto versionado | Revisa una persona | Riesgo | Evento Scrum |
|---|---|---|---|---|---|
| Requisitos | Extraer de las pantallas o el código Delphi de Pedidos Legado las reglas del folio y listar las ambigüedades como preguntas abiertas | `docs/specs/folio.md` (tabla R1–R5 + ejemplos) | _(por completar)_ | Inventar una regla que el legado no tiene (por ejemplo, aceptar `Ñ` o un largo distinto de 2–4 letras) | Refinamiento del backlog |
| Diseño | Proponer dos alternativas de firma y ubicación (`FolioFormatter` estático vs. value object `Folio`) con sus pros y contras | `docs/adr/0001-formato-folio.md` | _(por completar)_ | Sobrediseñar o meter dependencias de infraestructura en `PedidosWeb.Core` | Sprint Planning |
| Código | Implementar `FormatearFolio()` a partir de la spec y de los tests ya escritos | `src/PedidosWeb.Core/Folios/FolioFormatter.cs` | _(por completar)_ | Un folio distinto al del legado hace que los clientes no encuentren sus pedidos antiguos; validar R3 con `char.IsLetter` deja pasar `Ñ` y tildes | Durante el sprint (Daily) |
| Pruebas | Proponer los casos borde que faltan (R3 con `Ñ`, dígitos y `null`; orden de validación de R5) a partir de la tabla de ejemplos | `tests/PedidosWeb.Core.Tests/Folios/FolioFormatterTests.cs` | _(por completar)_ | Tests que confirman el código en vez de la regla: si Claude escribe código y tests a la vez, ambos pueden compartir el mismo error | Durante el sprint, antes de cerrar el ítem |
| Revisión y despliegue | Resumir el diff y revisarlo contra `AGENTS.md` y la Definition of Done (sin `double`, un test por regla, sin NuGet nuevos) | Descripción del PR y salida de `dotnet test` | _(por completar)_ | Aprobar un diff que nadie entendió, confiando en el resumen de la IA | Antes de la Sprint Review |
| Mejora continua | Comparar las métricas de la ronda A y la ronda B y proponer cambios a `AGENTS.md` según los errores que se repitieron | `docs/retro-u1.md` (el cambio a `AGENTS.md` lo hace una persona, regla 6) | _(por completar)_ | Sacar conclusiones con una sola muestra, o agregar reglas nuevas que nadie valida | Sprint Retrospective |

## Evidencia del paso 3

| | Sin IA | Con Claude Code |
|---|---|---|
| Tiempo (min) | | |
| Errores encontrados | | |
| Qué tuve que corregir | | |
