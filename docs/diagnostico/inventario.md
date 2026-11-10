# Inventario de Pedidos Legado

<!--
Etapa 1 · Entender (plan mode).
Una fila por cada archivo de legado/src/ (.pas y .dfm). Puedes agregar legado/PedidosLegado.dpr al final.
- Tipo: Formulario, Data module, Unidad de lógica, Utilidades, Globales…
- Responsabilidad: qué hace, en una frase y con tus palabras.
- Líneas: pídele a Claude que las cuente con una herramienta; no las estimes a ojo.
Punto de control E1: están todos los archivos de legado/src/.
-->

> Responsabilidades: borrador escrito a partir del código, pendiente de revisión del equipo.
> Líneas: contadas con `wc -l` sobre `legado/`.

## Archivos

| Archivo | Tipo | Responsabilidad | Líneas |
|---|---|---|---|
| `UFrmPrincipal.pas` | Formulario | Menú principal. Pide el usuario (sin clave), le da nivel 9 a ADMIN y JEFEVENTAS, conecta la base, carga `TASA_IVA` e `IMPRESORA` y abre las demás pantallas. Informes exige nivel ≥ 2. | 104 |
| `UFrmPrincipal.dfm` | Diseño de formulario | Menú Archivo / Maestros / Ventas y barra de estado con usuario y versión. | 62 |
| `UFrmClientes.pas` | Formulario | Mantención de clientes: busca por razón social, crea con ID = MAX + 1, valida RUT, tipo (N/V) y región (1–16). Solo nivel ≥ 2 marca VIP. No elimina clientes con pedidos. | 215 |
| `UFrmClientes.dfm` | Diseño de formulario | Grilla y campos enlazados a su propia tabla `CLIENTES` (`tblCli`), sin pasar por el data module. | 213 |
| `UFrmProductos.pas` | Formulario | Mantención de productos: filtra por descripción o bajo stock mínimo; valida código, precio > 0 y categoría de 3 letras. Solo admin (nivel 9) puede borrar. | 105 |
| `UFrmProductos.dfm` | Diseño de formulario | Grilla y navegador sobre su propia tabla `PRODUCTOS` (`tblProd`). | 98 |
| `UFrmPedidos.pas` | Formulario | Toma de pedidos: busca el cliente por RUT (bloquea si tiene deuda vencida hace más de 60 días), agrega líneas, valida pedido mínimo y máximo 40 líneas, graba pedido y líneas, **recalcula el total con SQL propio** y descuenta stock. | 420 |
| `UFrmPedidos.dfm` | Diseño de formulario | Paneles de cliente, líneas (`TStringGrid`) y totales. | 226 |
| `UFrmInformes.pas` | Formulario | Informe de ventas por rango de fechas, cliente y estado; exporta a CSV en la ruta de datos. | 137 |
| `UFrmInformes.dfm` | Diseño de formulario | Filtros, grilla y su propia consulta (`qryInforme`). | 160 |
| `UDMPrincipal.pas` | Data module | Conexión a DBISAM (con la clave escrita en el código) y dos servicios compartidos: `LeerParametro` y `EjecutarSQL`. | 100 |
| `UDMPrincipal.dfm` | Diseño de data module | Base `PEDIDOS` en `C:\PEDIDOS\DATOS`, 5 tablas y 2 consultas compartidas (`qryGeneral`, `qryAux`). | 65 |
| `UCalculoTotales.pas` | Unidad de lógica | Subtotal, descuento (volumen + VIP, tope 10 %, excluye categoría OFE), flete por región, IVA y redondeo al peso. | 147 |
| `UStock.pas` | Unidad de lógica | Consulta y descuenta stock por pedido (un VIP puede dejar el stock en −20) y registra el movimiento. | 93 |
| `UUtilidades.pas` | Utilidades | Redondeo y formato de pesos, limpieza y validación de RUT, fechas (pedidos después de las 18:00 pasan al siguiente día hábil) y textos. | 189 |
| `UGlobales.pas` | Globales | Variables compartidas por todo el sistema (usuario, nivel, IVA, flete, ruta de datos…) y sus valores iniciales. | 41 |
| `PedidosLegado.dpr` | Programa | Punto de entrada: crea el data module y el menú principal. | 24 |

## Tablas de la base

Base DBISAM 4 (`legado/db/esquema-dbisam.sql`). «Lee» y «Escribe» indican qué archivo arma la consulta o usa el componente, aunque la ejecute con las consultas del data module.

| Tabla | Clave | Líneas en el esquema | Filas en CSV | Lee | Escribe |
|---|---|---|---|---|---|
| `CLIENTES` | `ID_CLIENTE` | 12–25 (índice: 27) | 30 | `UFrmPedidos.pas:99-102`, `UFrmClientes.pas:76` y `:160`, `UFrmInformes.pas:65-66` | `UFrmClientes` vía `tblCli` (`.pas:100` grabar, `:113` eliminar) |
| `PRODUCTOS` | `ID_PRODUCTO` | 29–39 (índice: 41) | 40 | `UFrmPedidos.pas:151-152` y `:284-287`, `UStock.pas:23` | `UStock.pas:51` (stock), `UFrmProductos` vía `tblProd` |
| `PEDIDOS` | `FOLIO` | 43–53 (índices: 55-56) | 60 | `UFrmPedidos.pas:249` y `:338`, `UFrmClientes.pas:207`, `UFrmInformes.pas:65` | `UFrmPedidos.pas:259` (alta) y `:320` (total) |
| `PEDIDO_LINEAS` | `FOLIO` + `LINEA` | 58–66 | 169 | `UFrmPedidos.pas:279-287`, `UStock.pas:64` | `UFrmPedidos.pas:268` (alta) |
| `PARAMETROS` | `CLAVE` | 68–74 | 6 | `UDMPrincipal.pas:85`, llamado desde `UFrmPrincipal.pas:62` y `:65` | — |
| `REGIONES` | `CODIGO` | 4–10 | 16 | `UFrmPedidos.pas:101` (solo en un JOIN; no tiene componente propio) | — |
| `MOV_STOCK` | — | **no existe** | — | — | `UStock.pas:85` (el error se ignora en `:89-90`) |

## Lo que me llamó la atención

<!-- 2 o 3 observaciones rápidas (por ejemplo, algo que no calza con legado/README.md). -->

- **El README no describe el sistema real.** Dice Delphi 5, Paradox vía BDE y Crystal Reports 8 (`legado/README.md:5-7`). El código usa DBISAM 4.24 (`UDMPrincipal.dfm:10`, `esquema-dbisam.sql:1`), la unidad `Variants` (que no existe en Delphi 5) y no hay ninguna referencia a Crystal: los informes se exportan a CSV (`UFrmInformes.pas:111`).
- **El total del pedido se calcula dos veces, con reglas distintas.** `UCalculoTotales.pas:77-79` aplica el descuento por volumen con `>` 1.000.000 / 500.000 y no tiene descuento de Navidad; `UFrmPedidos.pas:296-305` usa `>=` y sí lo tiene. En pantalla se ve el primero; se graba el segundo (`UFrmPedidos.pas:320`). Y el comentario de `UCalculoTotales.pas:4` habla de «5 % sobre 400.000».
- **`UStock` escribe en una tabla que no existe.** `UStock.pas:85` inserta en `MOV_STOCK`, que no está en el esquema, y el error se traga con un `except end` vacío (`:89-90`): el historial de stock nunca se registra y nadie se entera.
