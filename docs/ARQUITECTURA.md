# Arquitectura

## Dependencias principales

```mermaid
flowchart TD
    Input[PlayerInputReader] --> Commands[IGameCommand]
    Commands --> Player[PlayerController]
    Player --> State[PlayerStateMachine]
    Player --> Weapon[WeaponController]
    Weapon --> Builder[WeaponBuilder]
    Weapon --> Decorator[IWeapon + Decorators]
    Decorator --> Projectile[Projectile]
    Projectile --> Enemy[Enemy]
    Spawner[EnemySpawner] --> Factory[EnemyCreator]
    Factory --> Prototype[EnemyPrototype.Clone]
    Factory --> Flyweight[EnemyDataCache]
    Enemy --> Events[GameEvents]
    PlayerHealth[PlayerHealth] --> Events
    Events --> HUD[GameUIController]
    Facade[GameFacade] --> Manager[GameManager]
    Facade --> Runtime[GameplayRuntime]
    Facade --> HUD
    Skill[SkillController] --> Composite[CompositeSkill]
    Composite --> Enemy
```

## Generación de enemigos

```mermaid
sequenceDiagram
    participant S as EnemySpawner
    participant C as EnemyCreator
    participant P as EnemyPrototype
    participant F as EnemyDataCache
    participant E as Enemy

    S->>C: Create(position)
    C->>P: Clone()
    C->>C: Ajusta HP/velocidad por oleada
    C->>F: Get(tipo)
    F-->>C: EnemySharedData compartido
    C->>E: Initialize(clon, sharedData, target)
    E-->>S: Enemy activo
```

`EnemyPrototype` es una configuración mutable por instancia: sus valores de HP, velocidad y contacto se ajustan para esa aparición. `EnemySharedData` contiene únicamente datos intrínsecos reutilizables y no guarda HP actual, posición ni timers.

## Disparo y daño

```mermaid
sequenceDiagram
    participant I as PlayerInputReader
    participant C as ShootCommand
    participant W as WeaponController
    participant D as WeaponDecorator
    participant P as Projectile
    participant E as Enemy

    I->>C: Execute()
    C->>W: TryFire(direction)
    W->>D: Fire(FireContext)
    D->>P: Añade efectos y crea proyectil
    P->>E: TakeDamage()
    P->>E: ApplyBurn/ApplyFreeze
```

Builder configura la configuración inicial de `WeaponConfig`. Decorator envuelve un comportamiento `IWeapon` ya existente y añade efectos durante `Fire`; no son el mismo mecanismo.

## Daño al jugador

```mermaid
flowchart LR
    Contact[Contacto enemigo] --> Health[PlayerHealth.TakeDamage]
    Health --> Machine[PlayerStateMachine.TryReceiveDamage]
    Machine --> Normal[NormalState]
    Machine --> Inv[InvulnerableState]
    Machine --> Dead[DeadState]
    Health --> Event[GameEvents.HealthChanged]
    Event --> HUD[GameUIController]
    Dead --> Facade[GameFacade.EndGame]
```

## Decisiones

- Se conserva la plantilla URP 2D existente porque el repositorio ya es un proyecto Unity compatible.
- Se mantiene el nuevo Input System activo. `PlayerInputReader` solo traduce dispositivos a comandos; los receptores siguen desacoplados.
- `GameManager` es el único Singleton. La caché de Flyweight no coordina la partida ni expone estado global de gameplay.
- `GameFacade` coordina ciclo de vida, pero las reglas de disparo, daño, experiencia y oleadas permanecen en sus subsistemas.
- El generador de editor crea una escena mínima estable. Gameplay, sprites geométricos y UI se construyen desde `GameplayRuntime` y `GameUIController`, evitando referencias serializadas frágiles y assets externos.
