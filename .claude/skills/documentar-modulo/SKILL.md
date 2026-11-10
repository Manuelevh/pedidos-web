---
name: documentar-modulo
description: "Documenta un módulo de Pedidos Web en docs/modulos/<Modulo>.md con propósito, API pública, ejemplo de uso y diagrama Mermaid. Úsala cuando pidan documentar, explicar o describir una clase, servicio o módulo del proyecto, aunque no nombren la Skill."
---

# Documentar un módulo

## Pasos
1. Lee la clase completa y su archivo de tests
   (`tests/PedidosWeb.Core.Tests/<Carpeta>/<Clase>Tests.cs`).
   Los tests son la fuente de verdad del comportamiento.
2. Lista los miembros públicos (constructores, métodos y propiedades)
   con su firma exacta.
3. Escribe `docs/modulos/<Modulo>.md` con las secciones de abajo.
4. Comprueba cada afirmación contra el código o un test. Si algo no está
   claro, anótalo en «Preguntas abiertas» en vez de suponerlo.
5. Al terminar, lista los métodos que documentaste y el test que
   respalda cada comportamiento descrito.

## Qué debe tener docs/modulos/<Modulo>.md
- Propósito
- API pública
- Ejemplo de uso
- Diagrama Mermaid
- Preguntas abiertas

## Reglas
- Español de Chile, frases cortas.
- Solo escribe en `docs/modulos/`. No modifica código ni tests.
- Usa solo lo que existe en el código. No inventes reglas de negocio
  (regla 8 de AGENTS.md).
- El ejemplo usa los datos de ejemplo del repo, y su salida coincide
  con la de un test.
- El diagrama es un `flowchart` del método principal del módulo.
- Montos en pesos chilenos, como enteros.
