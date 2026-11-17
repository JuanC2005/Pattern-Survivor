# Patrones de diseño

## Matriz de trazabilidad

| Patrón | Clase(s) | Acción disparadora | Evidencia en UI/log | Prueba |
|---|---|---|---|---|
| Singleton | `GameManager` | Iniciar escena | Instancia única | Abrir panel tras pulsar Jugar |
| Factory Method | `EnemyCreator` y creadores concretos | Aparición de enemigo | Creador y tipo creado | Esperar oleadas o `PROBAR` |
| Prototype | `EnemyPrototype` | Creación de enemigo | HP original y clon | `PROBAR` en Prototype |
| Builder | `WeaponBuilder`, `WeaponDirector` | Arma inicial, mejora o demo | Valores de configuración | `PROBAR` en Builder |
| Flyweight | `EnemySharedData`, `EnemyDataCache` | Aparición de enemigos | Vivos frente a flyweights | Observar una oleada |
| Composite | `ISkill`, `CompositeSkill` | Tecla `Q` | Hijas ejecutadas | Usar Q cerca de enemigos |
| Decorator | `WeaponDecorator`, decoradores de fuego/hielo | Mejora y disparo | Efectos aplicados | Elegir fuego/hielo y disparar |
| Facade | `GameFacade` | Botones de ciclo de vida | Operación coordinadora | Jugar, pausar y reiniciar |
| Observer | `GameEvents`, `GameUIController` | Daño, puntos, nivel | Eventos y HUD actualizado | Recibir daño |
| State | `PlayerStateMachine` y estados | Contacto o muerte | Transiciones | Recibir dos golpes o morir |
| Command | `IGameCommand` y comandos | Teclado, mouse, Q, Esc | Último comando | Mover, disparar, usar Q |

## 1. Singleton

**Clasificación:** creacional.

**Problema:** varios sistemas necesitan consultar el estado único de la sesión sin crear coordinadores duplicados.

**Participantes:** `GameManager` es el Singleton; gameplay, fachada y UI son clientes.

**Código:** `Assets/Scripts/Patterns/Singleton/GameManager.cs`.

```csharp
public static GameManager Instance { get; private set; }

private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;
}
```

**Comprobación:** pulsa `JUGAR`, abre `PATRONES` y selecciona Singleton. El evento informa si `GameManager.Instance` está activo.

**Ventaja:** acceso único al estado de sesión. **Limitación:** acopla los clientes al coordinador global y exige limpiar correctamente el estado al reiniciar.

## 2. Factory Method

**Clasificación:** creacional.

**Problema:** el spawner no debe conocer cómo se construye cada variante de enemigo.

**Participantes:** `EnemyCreator` define `Create`, y `NormalEnemyCreator`, `FastEnemyCreator` y `TankEnemyCreator` son creadores concretos. `EnemySpawner` es el cliente.

**Código:** `Assets/Scripts/Patterns/FactoryMethod/EnemyCreators.cs`.

```csharp
public abstract class EnemyCreator
{
    public abstract Enemy Create(Vector2 position);
}

public sealed class FastEnemyCreator : EnemyCreator
{
    public override Enemy Create(Vector2 position) => CreateFromPrototype("Fast", position);
}
```

**Comprobación:** espera una oleada o pulsa `PROBAR` con Factory Method. El panel registra el creador y el tipo generado.

**Ventaja:** añadir variantes no obliga a convertir el spawner en una fábrica estática gigante. **Limitación:** cada variante necesita un creador concreto.

## 3. Prototype

**Clasificación:** creacional.

**Problema:** se necesitan configuraciones de enemigos independientes y ajustables por oleada.

**Participantes:** `EnemyPrototype` es el prototipo; `EnemyPrototypeCatalog` mantiene originales; los creadores son clientes.

**Código:** `Assets/Scripts/Patterns/Prototype/EnemyPrototype.cs`.

```csharp
public EnemyPrototype Clone()
{
    return new EnemyPrototype
    {
        TypeName = TypeName,
        CurrentHealth = CurrentHealth,
        MoveSpeed = MoveSpeed,
        ContactDamage = ContactDamage,
        PointValue = PointValue,
        ExperienceValue = ExperienceValue
    };
}
```

**Comprobación:** selecciona Prototype y pulsa `PROBAR`; el evento muestra que el clon recibe `+7 HP` sin alterar el original.

**Ventaja:** las modificaciones por oleada quedan aisladas. **Limitación:** al incorporar colecciones mutables habría que ampliar la copia defensiva.

## 4. Builder

**Clasificación:** creacional.

**Problema:** construir armas válidas con varias propiedades sin constructores largos.

**Participantes:** `WeaponBuilder` construye `WeaponConfig`; `WeaponDirector` ofrece perfiles; `WeaponController` es cliente.

**Código:** `Assets/Scripts/Patterns/Builder/WeaponBuilder.cs`.

```csharp
return new WeaponBuilder()
    .WithDamage(7)
    .WithFireRate(6f)
    .WithProjectileSpeed(13f)
    .WithProjectileCount(1)
    .Build();
```

**Comprobación:** selecciona Builder y pulsa `PROBAR` para construir Basic, Rapid y Heavy. Las mejoras también reconstruyen una configuración independiente.

**Ventaja:** validación centralizada y perfiles legibles. **Limitación:** demasiadas opciones pueden hacer crecer la API del builder.

## 5. Flyweight

**Clasificación:** estructural.

**Problema:** muchos enemigos comparten nombre, color, velocidad base y puntos.

**Participantes:** `EnemySharedData` es el dato intrínseco inmutable; `EnemyDataCache` lo reutiliza; `Enemy` almacena HP y posición extrínsecos.

**Código:** `Assets/Scripts/Patterns/Flyweight/EnemySharedData.cs`.

```csharp
public static EnemySharedData Get(string typeName) => Cache[typeName];
```

**Comprobación:** observa una oleada y selecciona Flyweight. El evento muestra enemigos vivos frente a los tres objetos compartidos.

**Ventaja:** la identidad compartida no se duplica por enemigo. **Limitación:** no se puede guardar estado mutable específico dentro del flyweight.

## 6. Composite

**Clasificación:** estructural.

**Problema:** ejecutar habilidades simples y combinadas con el mismo contrato.

**Participantes:** `ISkill`, `HealSkill`, `AreaBlastSkill` y `CompositeSkill`; `SkillController` compone y ejecuta.

**Código:** `Assets/Scripts/Patterns/Composite/Skills.cs`.

```csharp
public void Execute(SkillContext context)
{
    foreach (ISkill child in children)
    {
        child.Execute(context);
    }
}
```

**Comprobación:** pulsa `Q` durante la partida. Se cura el jugador y se dañan enemigos cercanos; el panel registra las dos hijas.

**Ventaja:** una habilidad compuesta se usa como una habilidad individual. **Limitación:** el contexto debe contener los datos comunes que necesiten todas las hijas.

## 7. Decorator

**Clasificación:** estructural.

**Problema:** añadir efectos de ataque sin crear una subclase por combinación.

**Participantes:** `IWeapon`, `BaseWeapon`, `WeaponDecorator`, `FireWeaponDecorator` e `IceWeaponDecorator`; `WeaponController` compone las capas.

**Código:** `Assets/Scripts/Patterns/Decorator/WeaponDecorators.cs`.

```csharp
public override void Fire(FireContext context)
{
    context.Effects |= ProjectileEffects.Burn;
    context.BonusDamage += 2;
    base.Fire(context);
}
```

**Comprobación:** elige fuego o hielo al subir de nivel y dispara. El proyectil aplica quemadura o ralentización al impacto.

**Ventaja:** los efectos se combinan por composición. **Limitación:** el orden de las capas puede afectar el resultado si se añaden efectos con interacción.

## 8. Facade

**Clasificación:** estructural.

**Problema:** los botones no deben coordinar por separado manager, gameplay, tiempo y UI.

**Participantes:** `GameFacade` coordina; `GameManager`, `GameplayRuntime` y `GameUIController` son subsistemas.

**Código:** `Assets/Scripts/Patterns/Facade/GameFacade.cs`.

```csharp
public void StartNewGame()
{
    Time.timeScale = 1f;
    runtime.SetGameplayActive(true);
    runtime.ResetSession();
    gameManager.ResetSession();
    gameManager.SetPhase(GamePhase.Playing);
    ui.ShowGameplay();
}
```

**Comprobación:** usa `JUGAR`, `REINICIAR`, `REANUDAR` o `MENÚ` y revisa el registro.

**Ventaja:** las operaciones de alto nivel son simples para UI. **Limitación:** la fachada debe seguir siendo coordinadora y no absorber reglas de gameplay.

## 9. Observer

**Clasificación:** comportamiento.

**Problema:** actualizar HUD sin que la salud o el manager conozcan componentes visuales.

**Participantes:** `GameEvents` publica, `PlayerHealth` emite, `GameUIController` observa.

**Código:** `Assets/Scripts/Core/GameEvents.cs` y `Assets/Scripts/UI/GameUIController.cs`.

```csharp
GameEvents.HealthChanged += HandleHealthChanged;
// ...
GameEvents.HealthChanged -= HandleHealthChanged;
```

**Comprobación:** deja que un enemigo golpee al jugador. El HP cambia mediante `HealthChanged` y el panel registra el evento.

**Ventaja:** modelo y UI quedan desacoplados. **Limitación:** las suscripciones deben retirarse siempre en `OnDisable`.

## 10. State

**Clasificación:** comportamiento.

**Problema:** las reglas de movimiento, daño e invulnerabilidad cambian según el estado del jugador.

**Participantes:** `IPlayerState`, `NormalState`, `InvulnerableState`, `DeadState` y `PlayerStateMachine`.

**Código:** `Assets/Scripts/Patterns/State/PlayerStateMachine.cs`.

```csharp
if (health.CurrentHealth <= 0)
{
    TransitionTo(new DeadState(this));
}
else
{
    TransitionTo(new InvulnerableState(this, invulnerabilityDuration));
}
```

**Comprobación:** recibe daño y vuelve a recibirlo dentro de `0.8 s`; el segundo impacto se ignora. Al morir, el jugador no puede moverse ni disparar.

**Ventaja:** las transiciones y capacidades están centralizadas. **Limitación:** cada nuevo estado debe definir entrada, actualización y salida.

## 11. Command

**Clasificación:** comportamiento.

**Problema:** desacoplar lectura de teclas y mouse de las acciones del jugador.

**Participantes:** `IGameCommand`, `MoveCommand`, `ShootCommand`, `UseSkillCommand`, `PauseCommand`; `PlayerInputReader` es invocador.

**Código:** `Assets/Scripts/Gameplay/PlayerInputReader.cs`.

```csharp
moveCommand.SetDirection(movement.normalized);
moveCommand.Execute();
```

**Comprobación:** mueve, dispara, pulsa Q o Esc y revisa el último comando en el panel.

**Ventaja:** cambiar el backend de entrada no obliga a reescribir receptores. **Limitación:** hay que administrar el contexto de cada comando y evitar crear objetos por frame.
