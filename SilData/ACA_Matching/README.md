# ACA.Matching — Motor de Matching

Class library (.NET 8) que clasifica pares (cupo, solicitud) como **Directo**,
**Parcial**, **Condicional** o **Incompatible**.

## Propósito

El motor es el componente central de la sección 5 del documento técnico
`SIL_Tecnico_GestionSolicitudesTurno.md`. Decide, **en el servidor**, cómo
de bueno es el match entre una solicitud de turno y un cupo disponible, para
que el cliente Blazor pueda mostrar el diálogo de confirmación de la sección
5.2 cuando el match es `Condicional` (la solicitud tiene observaciones).

## Orden de reglas (contrato)

```
GranoRule (mandatorio)
   ↓
VendedorRule (mandatorio)
   ↓
CompradorRule (opcional)
   ↓
DestinoRule (opcional, switch sobre TipoDestino)
   ↓
ClasificadorMatch (terminal: Condicional > Directo > Parcial)
```

El **primer** outcome terminal (`Incompatible` o `Clasificado`) gana. El orden
es **parte del contrato** y no se inyecta por DI: cambiarlo cambia la semántica.

## Tipos de match

| Tipo | Cuándo |
|---|---|
| `Directo` | Mandatorios OK y todos los opcionales coinciden. |
| `Parcial` | Mandatorios OK pero algún opcional (comprador o destino) no coincide. |
| `Condicional` | La solicitud tiene `Observacion` no vacía (gana sobre todo lo demás). |
| `Incompatible` | Algún criterio mandatorio (Grano o Vendedor) no coincide. |

## Distinción ZonaGeografica vs Destino

La entidad `SolicitudTurno.CuentaDestino` (long?) está **overloaded**:

- Cuando `TipoDestino = ZonaPortuaria` (caso habitual en producción),
  `CuentaDestino` contiene un `ZonaGeoId` y se compara vía `IContextoZona`.
- Cuando `TipoDestino = Destino`, `CuentaDestino` contiene la `Cuenta` del
  puerto físico y se compara directamente contra `Cupo.CodDestino`.

La pertenencia `CodDestino → ZonaGeoId` se resuelve **fuera** del motor (vía
`IZonaGeograficaResolver` en SILData) y se inyecta como `IContextoZona`.

## Cómo agregar una regla nueva

1. Implementar `IMatchRule` (ver `Engine/Reglas/IMatchRule.cs`).
2. Decidir si la regla emite `Incompatible` (cortocircuita) o actualiza
   `ResultadoParcial` y retorna `Continuar`.
3. Insertar la instancia en `MatchingEngine.ConstruirCadenaDefault()` en el
   orden correcto (después de los mandatorios, antes del Clasificador).
4. Agregar tests en `ACA_Matching.Tests/Rules/`.

## Integración

- **SILData** referencia este proyecto y registra `IMatchingEngine` como
  singleton en `Program.cs`.
- `SolicitudTurnoService.AcceptRequestsAsync` invoca el motor antes del
  Accept para clasificar el match y propagar el `TipoMatch` al cliente.
- `SolicitudTurnoService.BuscarMatchesAsync` orquesta el endpoint bulk
  `POST /api/ShiftRequest/Matches`.

## Tests

```
cd ACA_Matching.Tests
dotnet test
```

Cobertura: 45 tests xUnit en `Rules/`, `Engine/` y `Contexto/`.
