# AGENTS.md · Pedidos Web

## Proyecto
Portal web de pedidos de una distribuidora; moderniza Pedidos Legado (Delphi + DBISAM).
API REST en ASP.NET Core (.NET 10) con 45 pedidos de ejemplo en memoria, sin base de datos.

## Comandos
- Compilar: `dotnet build`
- Testear: `dotnet test`
- Ejecutar la API: `dotnet run --project src/PedidosWeb.Api` (http://localhost:5080, chequeo en `/salud`)

## Estilo de código
- Nombres de dominio en español: `Pedido`, `IPedidoRepositorio`, `ObtenerPorId`, `FormatearFolio`.
- Dinero en pesos chilenos: `decimal` sin decimales; nunca `double` ni `float`.
- Fechas sin hora: `DateOnly`.
- `src/PedidosWeb.Core`: modelo y lógica de dominio, sin dependencias de infraestructura.
- `src/PedidosWeb.Api`: Minimal API en `Program.cs`; expone endpoints y delega en Core.
- `Nullable` habilitado en todos los proyectos.

## Tests
- xUnit 2.9 en `tests/PedidosWeb.Core.Tests/`, con carpetas espejo de `src/PedidosWeb.Core/`.
- Tests de la API en `tests/PedidosWeb.Api.Tests/`, con `WebApplicationFactory<Program>`.
- Un archivo de tests por clase: `<Clase>Tests.cs`.
- Nombre de cada test: `Metodo_Escenario_ResultadoEsperado`.
- Se ejecutan con `dotnet test` y deben estar en verde antes de cada commit.

## Seguridad
- Nunca escribas secretos, claves ni cadenas de conexión en el código ni en archivos versionados.

## Reglas de trabajo
- Toda funcionalidad parte de una spec aprobada en `specs/<historia>/spec.md`.
- Un test xUnit por criterio de aceptación.
- No hacer commit sin tests en verde.
- Sin preguntar puedes: leer archivos y ejecutar `dotnet build` y `dotnet test`.
- Cualquier edición de archivos requiere aprobación previa.
- Pregunta siempre antes de: agregar paquetes NuGet; tocar `.sln` o `.csproj` o crear proyectos;
  cambiar los datos de ejemplo; hacer commit, push o merge.
