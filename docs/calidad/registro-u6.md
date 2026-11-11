# Registro de calidad · Unidad 6

<!--
Lo completas tú durante el taller. Frases cortas: lo que hiciste y por qué.
Borra estos comentarios al terminar.
-->

## Preparación

- **Tests en `Skip` (pendientes de unidades anteriores):** ninguno. `dotnet test` en local: 102 tests en verde (78 Core + 24 Api), 0 omitidos.

## Etapa 1 · Tests que sí prueban

- **Módulo elegido:** <CalculoTotales, ClientesService o FolioFormatter>
- **Tests que propuso Claude:** <cuántos>
- **Cambio que hice a mano para probarlos:** <qué línea cambié>
- **Tests que me quedé:** <cuántos y cuáles>
- **Tests que borré y por qué:** <el que pasaba con y sin el cambio>

## Etapa 2 · Pipeline seguro

- **`ci.yml`:** https://github.com/Manuelevh/pedidos-web/actions/runs/43079801437 (commit `c9ae45c`)
- **`claude.yml` en `main`:** sí (commit `fdefc4a`); máximo 5 turnos y sin `Edit`/`Write`: Claude solo diagnostica.
- **Revisé:** permisos mínimos ☑ · timeout ☑ · versiones fijas ☑ · token solo como secreto ☑
  - Permisos: `contents: read` en ambos workflows; en `claude.yml` solo `id-token: write` para el token de la App.
  - Timeout: `timeout-minutes: 10` en ambos jobs.
  - Versiones: `ubuntu-24.04`, .NET SDK `10.0.404` y acciones fijadas por SHA de commit.
  - Token: `CLAUDE_CODE_OAUTH_TOKEN` como secreto del repo; búsqueda de `sk-ant-` en archivos e historial sin resultados.

## Etapa 3 · La falla

- **PR:** <enlace>
- **Qué rompí (a mano):** <archivo y cambio>
- **Diagnóstico de `@claude` (en una línea):** <resumen>
- **Mi decisión:** Aceptar ☐ · Ajustar ☐ · Rechazar ☐
- **Por qué:** <tu razón>
- **Corrección y merge:** <commit de la corrección; CI en verde ☐; merge ☐>
