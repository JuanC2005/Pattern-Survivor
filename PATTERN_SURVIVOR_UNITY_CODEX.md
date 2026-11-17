# PATTERN SURVIVOR — Especificación de desarrollo para Codex

> **Rol:** Actúa como desarrollador senior de Unity 2D, arquitecto de software C# y QA técnico.
>
> **Objetivo:** Construye un **videojuego arcade 2D jugable** en Unity llamado **Pattern Survivor** que use **realmente y de manera demostrable** estos **11 patrones de diseño**: Singleton, Factory Method, Prototype, Builder, Flyweight, Composite, Decorator, Facade, Observer, State y Command.
>
> **Instrucción principal:** No entregues únicamente ejemplos teóricos ni scripts aislados. Crea el proyecto Unity funcional, escenas, scripts, prefabs o generadores, interfaces, HUD, controles, un modo de demostración de patrones y documentación. No dejes TODOs, código ficticio, referencias faltantes ni clases vacías. Prioriza que abra, compile y sea jugable antes de añadir adornos.

---

## 1. Alcance y decisiones obligatorias

- **Motor:** Unity 6, proyecto **2D Core** (renderizador integrado; evita dependencias externas innecesarias). Si el repositorio ya contiene un proyecto Unity compatible, **adáptate a su versión** en vez de recrearlo innecesariamente.
- **Lenguaje:** C#.
- **Plataforma objetivo:** PC Windows, ejecución desde el editor y, cuando sea viable, build standalone.
- **Juego:** supervivencia arcade top-down, una sola arena, enemigos por oleadas, disparos, mejoras de arma y puntuación.
- **Perspectiva:** 2D cenital, cámara ortográfica fija.
- **Gráficos:** pixel art minimalista o formas geométricas 2D creadas en el propio proyecto. **No dependas de assets pagos, cuentas, APIs ni descargas de terceros.**
- **Entrada:** teclado y mouse. Usa Input Manager clásico de Unity como solución simple; incluye instrucciones para configurar `Active Input Handling = Both` o `Input Manager (Old)`. Si el proyecto ya usa Input System, puedes adaptarlo manteniendo la capa Command.
- **Escena mínima:** `Assets/Scenes/GameScene.unity` completamente configurada y lista para Play.
- **Resultado esperado:** el usuario puede abrir el proyecto, pulsar Play, iniciar la partida, moverse, disparar, matar enemigos, recibir daño, comprar/seleccionar mejoras, morir y reiniciar. Los 11 patrones se activan en mecanismos reales del juego.
- **Prioridad:** prototipo académico verificable y estable, no videojuego comercial. Evita complejidad superflua.

### 1.1 Mecánicas mínimas

1. Menú inicial con botones **Jugar**, **Patrones** e instrucciones de control.
2. Jugador controlado por **WASD / flechas**; movimiento en 8 direcciones y límites de arena.
3. Apuntar con el mouse y disparar con **clic izquierdo** o **barra espaciadora** (barra puede disparar en dirección apuntada o última dirección válida).
4. Enemigos que aparecen periódicamente alrededor de la arena y persiguen al jugador.
5. Al menos **3 variantes**: normal, rápido y tanque.
6. Vida máxima inicial: **100**; al tocar un enemigo se recibe daño con invulnerabilidad temporal para evitar pérdidas instantáneas.
7. Enemigos derrotados entregan puntuación y progreso de experiencia; el jugador sube de nivel.
8. Al subir de nivel, aparece una selección sencilla de **3 mejoras** entre: +daño, +cadencia, efecto de fuego, efecto de hielo, proyectil doble (al menos **3 deben funcionar de verdad**, con preferencia por las cinco).
9. HUD: barra/valor de vida, puntos, tiempo, nivel, enemigos derrotados y oleada.
10. Pausa con **Esc**, pantalla de derrota y botón **Reiniciar**.
11. Dificultad aumenta suavemente con el tiempo; deben existir condiciones razonables para ganar puntos y perder la partida.
12. Panel o modo demostración **«11 patrones»** accesible desde el juego, que indica cuál se usó, qué clase lo implementa y qué evento real lo disparó.

### 1.2 Valores de juego configurables

Son valores iniciales sugeridos; expónlos por `[SerializeField]` o configuraciones editables, no los disperses como números mágicos:

| Parámetro | Valor inicial |
|---|---:|
| Vida jugador | 100 |
| Velocidad jugador | 5 unidades/s |
| Daño por contacto normal | 10 |
| Invulnerabilidad tras golpe | 0.8 s |
| Daño base de arma | 10 |
| Disparos por segundo | 3 |
| Velocidad proyectil | 11 unidades/s |
| Vida enemigo normal | 25 |
| Vida enemigo rápido | 15 |
| Vida enemigo tanque | 65 |
| Intervalo inicial de generación | 2 s |
| Puntos por enemigo normal / rápido / tanque | 10 / 15 / 25 |

---

## 2. Arquitectura general

Mantén separados: **dominio / reglas**, **comportamientos MonoBehaviour**, **infraestructura Unity** y **UI**. Emplea interfaces cuando hagan claro el patrón; evita un sistema de inyección de dependencias complejo.

Flujo principal:

```text
Input del usuario
    |
    v
InputReader ---> ICommand (Move, Shoot, Pause)
                      |
                      v
                  PlayerController -> PlayerStateMachine
                      |                     |
                      |                State: Normal / Hurt / Invulnerable / Dead
                      v
                WeaponController ---> IWeapon -> WeaponDecorator(s)
                      |                         |
                      v                         v
                Projectile                  Efectos de ataque
                      |
                      v
                EnemyHealth ----> GameEvents ----> HUD / PatternDebugPanel
                      ^               |
                      |               v
               EnemySpawner       Score / XP / GameManager
                      |
                      v
             EnemyCreator (Factory Method)
                      |
          EnemyPrototype.Clone() + EnemyData Flyweight

GameFacade -> GameManager / Spawner / UI / LevelUp / Pause
SkillComposite -> habilidades individuales y combinadas
WeaponBuilder -> configura arma base antes de aplicar decorators
```

Evita dependencias circulares. La UI escucha los eventos; el modelo no conoce componentes de texto ni botones.

---

## 3. Los 11 patrones: requisitos de implementación verificables

**ATENCIÓN:** No basta con crear clases llamadas `*Factory`, `*Builder` o `*Observer`. Cada patrón debe tener estructura correcta, invocación real durante una partida y evidencia observable en el modo demo o consola.

### 01 — Singleton (creacional)

**Propósito:** una sola instancia coordinadora de la partida.

- Crea `GameManager : MonoBehaviour` con `public static GameManager Instance { get; private set; }`.
- En `Awake`, gestiona correctamente duplicados y ciclo de vida. **No es obligatorio `DontDestroyOnLoad`** porque basta una escena; usarlo solo si se justifican los reinicios/cambios de escena.
- Expone estado global de partida (`Menu`, `Playing`, `Paused`, `LevelUp`, `GameOver`), puntuación, enemigos derrotados y tiempo, preferentemente mediante propiedades de solo lectura y métodos públicos controlados.
- Un segundo intento de instanciar el manager **no debe generar dos managers activos**.
- No conviertas todas las clases en singletons. Mantén solo el coordinador principal como Singleton.
- **Demostración:** botón «Singleton» del panel muestra la instancia única y el estado de partida.

**Clases sugeridas:** `GameManager.cs`, `GamePhase.cs`.

### 02 — Factory Method (creacional)

**Propósito:** crear distintos enemigos sin que el spawner conozca los detalles de cada clase concreta.

- Implementa un **verdadero Factory Method**, no solo `switch` en una clase estática.
- Define, por ejemplo:

```csharp
public abstract class EnemyCreator
{
    public abstract Enemy Create(Vector2 position);
}

public sealed class NormalEnemyCreator : EnemyCreator { /* ... */ }
public sealed class FastEnemyCreator : EnemyCreator { /* ... */ }
public sealed class TankEnemyCreator : EnemyCreator { /* ... */ }
```

- `EnemySpawner` recibe o selecciona un `EnemyCreator` según la oleada y solicita `Create(...)`.
- En Unity, los creadores pueden ser clases C# puras que reciben referencias a prefabs/configuraciones en su constructor. No intentes instanciar `MonoBehaviour` con `new`.
- Los enemigos deben diferir efectivamente en estadísticas o comportamiento.
- **Demostración:** el registro muestra `Factory Method → FastEnemyCreator creó FastEnemy` al aparecer un enemigo rápido.

**Clases sugeridas:** `EnemyCreator.cs`, `NormalEnemyCreator.cs`, `FastEnemyCreator.cs`, `TankEnemyCreator.cs`, `EnemySpawner.cs`.

### 03 — Prototype (creacional)

**Propósito:** duplicar **configuraciones** de enemigos a partir de prototipos existentes para producir variantes durante la partida.

- Crea un `EnemyPrototype` o `EnemyRuntimeConfig` que implemente clonación explícita `Clone()` y contenga datos de configuración que puedan editarse por instancia.
- Distinción esencial: **`Object.Instantiate` de Unity por sí solo no demuestra adecuadamente el patrón Prototype**. Debe existir clonación propia de una configuración/prototipo y una variante derivada de ella.
- Flujo propuesto: una fábrica elige el prototipo según tipo; llama `Clone()`; modifica en el clon una característica (por ejemplo, HP o velocidad +10 % por oleada) y aplica esa instancia al enemigo creado.
- Asegura independencia de los datos variables del clon: cambiar el HP del clon no modifica el prototipo original ni otro clon. Para colecciones mutables, usa copia profunda o defensiva.
- **Demostración:** botón del panel o spawn de enemigo «élite» imprime valores del original y del clon modificado.

**Clases sugeridas:** `EnemyPrototype.cs`, `EnemyPrototypeCatalog.cs`.

### 04 — Builder (creacional)

**Propósito:** ensamblar armas con combinaciones de características.

- Crea `WeaponConfig` con daño, cadencia, velocidad de bala, cantidad de balas y alcance/vida útil del proyectil.
- Construcción explícita mediante `WeaponBuilder` con métodos `WithDamage(...)`, `WithFireRate(...)`, `WithProjectileSpeed(...)`, `WithProjectileCount(...)`, `Build()`.
- `Build()` debe devolver configuración **válida e independiente**, aplicar validaciones básicas y evitar compartir accidentalmente estado mutable del builder.
- Usa un `WeaponDirector` opcional para perfiles «Basic», «Rapid» y «Heavy». **Al menos dos** armas realmente construidas con Builder deben poder demostrarse.
- `WeaponController` aplica la configuración construida y la puede modificar mediante mejoras.
- **Demostración:** el panel muestra arma construida y sus valores; cambiar de arma o subir nivel debe poder usar Builder.

**Clases sugeridas:** `WeaponBuilder.cs`, `WeaponConfig.cs`, `WeaponDirector.cs`.

### 05 — Flyweight (estructural)

**Propósito:** reutilizar datos inmutables compartidos entre múltiples enemigos.

- Crea `EnemySharedData` (puede ser un `ScriptableObject` con configuración **solo lectura en ejecución** o una clase inmutable) con campos intrínsecos: nombre de tipo, color/sprite, velocidad base, vida base, puntos otorgados.
- Usa `EnemyDataCache` / `EnemyFlyweightFactory` para que varios enemigos del mismo tipo referencien **la misma instancia de datos compartidos**.
- El estado extrínseco por enemigo (**posición, HP actual, timers, objetivo**) debe vivir en cada instancia y nunca dentro del flyweight.
- `ReferenceEquals(a.SharedData, b.SharedData)` debe ser verdadero para dos enemigos del mismo tipo; sus HP/posiciones deben seguir siendo independientes.
- **Demostración:** mostrar cantidad de enemigos vivos frente a cantidad de objetos flyweight (por ejemplo, 25 enemigos, 3 datos compartidos), **sin inventar métricas de memoria**.

**Clases sugeridas:** `EnemySharedData.cs`, `EnemyDataCache.cs`, `Enemy.cs`.

### 06 — Composite (estructural)

**Propósito:** tratar una habilidad simple y una combinación de habilidades mediante la misma interfaz.

- Define:

```csharp
public interface ISkill
{
    void Execute(SkillContext context);
}
```

- Implementa al menos `HealSkill`, `AreaBlastSkill` y `CompositeSkill`.
- `CompositeSkill` administra una colección de `ISkill`, tiene `Add`, `Remove` y ejecuta cada componente usando el mismo contrato.
- Incluye una **habilidad combinada visible** (por ejemplo, tecla `Q`: pequeña curación y pulso de daño contra enemigos cercanos), que consume un cooldown.
- Evita que el Composite se limite a imprimir mensajes: **las dos habilidades hijas deben afectar el juego**.
- **Demostración:** registro de ejecución de las hijas y del composite.

**Clases sugeridas:** `ISkill.cs`, `SkillContext.cs`, `HealSkill.cs`, `AreaBlastSkill.cs`, `CompositeSkill.cs`, `SkillController.cs`.

### 07 — Decorator (estructural)

**Propósito:** extender dinámicamente efectos de los ataques sin heredar decenas de clases de armas.

- Define un contrato como `IWeapon` con un método `Fire(FireContext context)` y/o un contrato de efecto `IProjectileEffect` con método de aplicación.
- Implementa un decorador **con composición real y misma interfaz**:

```csharp
public abstract class WeaponDecorator : IWeapon
{
    protected readonly IWeapon Inner;
    protected WeaponDecorator(IWeapon inner) => Inner = inner;
    public virtual void Fire(FireContext context) => Inner.Fire(context);
}
```

- `FireWeaponDecorator`, `IceWeaponDecorator` y/o `DamageWeaponDecorator` deben agregar un comportamiento medible: quemadura periódica, ralentización temporal o daño adicional.
- Las capas pueden combinarse: `new IceWeaponDecorator(new FireWeaponDecorator(baseWeapon))`.
- **No reemplaces Decorator por un único `enum` de arma o una gran condición `switch`**.
- No uses el mismo mecanismo para Builder y Decorator: Builder **construye la configuración inicial**; Decorator **envuelve un comportamiento existente**.
- **Demostración:** registro al aplicar mejora, indicador de efectos y prueba de que el enemigo recibe los efectos en combate.

**Clases sugeridas:** `IWeapon.cs`, `BaseWeapon.cs`, `WeaponDecorator.cs`, `FireWeaponDecorator.cs`, `IceWeaponDecorator.cs`.

### 08 — Facade (estructural)

**Propósito:** ofrecer una interfaz simple para secuencias de subsistemas.

- Crea `GameFacade` con métodos de alto nivel `StartNewGame()`, `PauseGame()`, `ResumeGame()`, `RestartGame()`, `EndGame()`.
- Al iniciar/reiniciar, coordina de manera concreta el estado del manager, la preparación del jugador, el spawner, el HUD y la pantalla activa.
- Los botones del menú/UI deben invocar **Facade**, en vez de configurar manualmente cinco objetos.
- Evita que `GameFacade` se vuelva un «Dios objeto» con todas las reglas de gameplay; es coordinador, no reemplazo de subsistemas.
- **Demostración:** trazas de `Facade → StartNewGame` y los subsistemas coordinados.

**Clases sugeridas:** `GameFacade.cs`, `GameUIController.cs`.

### 09 — Observer (comportamiento)

**Propósito:** emitir cambios del juego para que HUD y otros observadores se actualicen sin acoplamiento directo.

- Implementa eventos C# `event Action<...>` (o un EventBus tipado) para salud, puntos, nivel, derrota y/o spawn.
- El modelo emite eventos; HUD, sonido opcional y panel demo **se suscriben** sin que el jugador tenga referencias a sus textos/UI.
- Suscribe y desuscribe correctamente en `OnEnable` / `OnDisable` o métodos equivalentes para no dejar suscripciones duplicadas.
- Un evento real de daño debe actualizar salud en pantalla sin llamada directa a la UI.
- **Demostración:** `Observer → OnHealthChanged(90)` y HUD actualizado a 90/100.

**Clases sugeridas:** `GameEvents.cs`, `PlayerHealth.cs`, `HUDController.cs`.

### 10 — State (comportamiento)

**Propósito:** representar estados exclusivos del jugador con transición y conductas distintas.

- Define `IPlayerState` con `Enter`, `Tick`, `Exit`, o una interfaz equivalente.
- Implementa estados al menos **Normal**, **Invulnerable** y **Dead**; opcional `Hurt` transitorio.
- `PlayerStateMachine` coordina transiciones explícitas.
- En estado `Invulnerable`, recibir otro contacto no quita vida; el jugador puede mostrar parpadeo visual. En `Dead`, no puede moverse ni disparar.
- Las reglas de transición deben residir en el sistema de estados, **no solamente** en `if (isDead)` dispersos por otros scripts.
- El estado global del juego (`Playing`, `Paused`, etc.) puede quedar en `GameManager`; aclara que es diferente del **State del jugador**.
- **Demostración:** cada transición aparece en panel: `Normal → Invulnerable → Normal`, y `Normal → Dead`.

**Clases sugeridas:** `IPlayerState.cs`, `NormalState.cs`, `InvulnerableState.cs`, `DeadState.cs`, `PlayerStateMachine.cs`.

### 11 — Command (comportamiento)

**Propósito:** convertir las acciones de entrada en objetos o implementaciones de comandos desacoplados de la lectura de teclas.

- Define `IGameCommand` con `Execute()` o `Execute(CommandContext context)`.
- Implementa `MoveCommand`, `ShootCommand`, `UseSkillCommand`, `PauseCommand`.
- `PlayerInputReader` traduce las teclas en comandos; **no llama directamente** a `PlayerController.Move`, `WeaponController.Fire`, etc.
- Puedes reutilizar instancias de comandos actualizando contexto o parámetros para evitar asignaciones cada frame.
- No es necesario implementar Undo para movimiento/disparos. **Undo es opcional en Command**, no requisito del patrón.
- **Demostración:** el panel puede mostrar el último comando ejecutado (`ShootCommand`, `MoveCommand`, etc.), con limitación de frecuencia para movimiento.

**Clases sugeridas:** `IGameCommand.cs`, `MoveCommand.cs`, `ShootCommand.cs`, `UseSkillCommand.cs`, `PauseCommand.cs`, `PlayerInputReader.cs`.

---

## 4. Integración real entre patrones

Al desarrollar, implementa al menos estos caminos funcionales:

1. **Menú principal → Jugar:** `GameUIController` llama `GameFacade.StartNewGame()`, que coordina `GameManager` (Singleton), jugador, spawner y HUD.
2. **Aparece enemigo:** `EnemySpawner` elige un `EnemyCreator` (Factory Method), obtiene `EnemySharedData` reutilizable (Flyweight), duplica y ajusta un `EnemyPrototype` (Prototype) y aplica el resultado al prefab instanciado.
3. **Disparo:** `PlayerInputReader` crea/ejecuta `ShootCommand` (Command); `WeaponController` usa un arma configurada por Builder y envuelta en decoradores activos (Decorator).
4. **Golpe al jugador:** colisión → `PlayerHealth` → máquina de estados (State) que determina si acepta daño → se emite `HealthChanged` (Observer) → se actualiza HUD.
5. **Subida de nivel:** se emite evento `LevelChanged` (Observer) → `GameFacade` o un controlador específico abre selección de mejoras y pausa el juego → al seleccionar, modifica arma (Builder y/o Decorator) → reanuda.
6. **Habilidad especial:** `UseSkillCommand` ejecuta `CompositeSkill`, que ejecuta curación y explosión de área reales.
7. **Muerte:** `DeadState` bloquea controles, `GameFacade.EndGame()` detiene generación y muestra derrota; reinicio limpia enemigos/proyectiles y reinicia estadísticas sin duplicar suscripciones.

Cada caso debe poder ser ejecutado y reproducido en una sesión corta.

---

## 5. Interfaz y diseño visual

Estética arcade/pixel art, colores contrastantes, aspecto legible a 1080p. Puede montarse completamente con sprites básicos, `SpriteRenderer`, `Canvas`, imágenes y textos de Unity. Si TextMeshPro requiere importar recursos, documenta o automatiza esa preparación; alternativamente usa UI Text convencional si es compatible con la versión objetivo.

### 5.1 HUD durante la partida

- Arriba izquierda: `HP: 100/100` y barra de salud.
- Arriba centro: `OLEADA 1 — 00:45`.
- Arriba derecha: `PUNTOS: 150 | KILLS: 12 | NIVEL: 2`.
- Abajo: texto compacto de controles `WASD Mover | Click Disparar | Q Habilidad | Esc Pausa`.
- Indicador breve de arma/mejoras actuales.
- Señal visual cuando un enemigo recibe daño, es congelado o quemado.

### 5.2 Pantallas y paneles

- **Menú:** nombre `PATTERN SURVIVOR`, botones `JUGAR`, `VER PATRONES` y `SALIR` (Salir solo relevante en standalone).
- **Pausa:** `REANUDAR`, `REINICIAR`, `MENÚ`.
- **Mejoras:** 3 opciones visibles y seleccionables por mouse, con descripción breve.
- **Game Over:** puntuación final, enemigos eliminados, tiempo y `REINTENTAR`.
- **Panel 11 patrones:** navegación entre Singleton, Factory Method, Prototype, Builder, Flyweight, Composite, Decorator, Facade, Observer, State y Command; para cada uno:
  - Nombre, tipo (creacional / estructural / comportamiento).
  - Definición de una línea.
  - Clases C# que lo implementan.
  - Uso dentro del juego.
  - **Evidencia de ejecución real:** último evento con marca de tiempo o contador y descripción corta.
  - Botón `PROBAR` solamente cuando exista una acción segura que pueda activarse con el juego abierto, o instrucciones para activarlo. No simules acciones inventadas.

### 5.3 Registro académico de patrones

Implementa `PatternEventLogger` con eventos o un pequeño historial por patrón:

```text
[02:14] Singleton: GameManager.Instance activo (1 instancia)
[02:15] Facade: StartNewGame coordinó reinicio, UI y spawner
[02:18] Factory Method: TankEnemyCreator creó TankEnemy
[02:18] Prototype: copia de TankPrototype (HP ajustado 65 -> 72)
[02:18] Flyweight: sharedData Tank reutilizado entre 4 enemigos
[02:21] Command: ShootCommand ejecutado
[02:21] Builder: arma Rapid configurada, cadencia 5/s
[02:22] Decorator: Burn aplicado al objetivo por FireWeaponDecorator
[02:25] Observer: HealthChanged disparado, HUD actualizado a 90/100
[02:25] State: Normal -> Invulnerable
[02:29] Composite: ejecutó HealSkill y AreaBlastSkill
```

Estos ejemplos describen **mensajes posibles**, no valores obligatorios ni evidencia falsa. Mostrar solo hechos que hayan ocurrido; no disparar «fake events» para rellenar la lista. La interfaz debe limitar frecuencia y cantidad de registros para no degradar rendimiento.

---

## 6. Estructura de carpetas sugerida

Puedes redistribuir clases si beneficia la claridad, manteniendo las responsabilidades.

```text
PatternSurvivor/
├── Assets/
│   ├── Scenes/
│   │   └── GameScene.unity
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GamePhase.cs
│   │   │   ├── GameEvents.cs
│   │   │   └── PatternEventLogger.cs
│   │   ├── Patterns/
│   │   │   ├── Singleton/
│   │   │   │   └── GameManager.cs
│   │   │   ├── FactoryMethod/
│   │   │   │   ├── EnemyCreator.cs
│   │   │   │   ├── NormalEnemyCreator.cs
│   │   │   │   ├── FastEnemyCreator.cs
│   │   │   │   └── TankEnemyCreator.cs
│   │   │   ├── Prototype/
│   │   │   │   ├── EnemyPrototype.cs
│   │   │   │   └── EnemyPrototypeCatalog.cs
│   │   │   ├── Builder/
│   │   │   │   ├── WeaponConfig.cs
│   │   │   │   ├── WeaponBuilder.cs
│   │   │   │   └── WeaponDirector.cs
│   │   │   ├── Flyweight/
│   │   │   │   ├── EnemySharedData.cs
│   │   │   │   └── EnemyDataCache.cs
│   │   │   ├── Composite/
│   │   │   │   ├── ISkill.cs
│   │   │   │   ├── HealSkill.cs
│   │   │   │   ├── AreaBlastSkill.cs
│   │   │   │   └── CompositeSkill.cs
│   │   │   ├── Decorator/
│   │   │   │   ├── IWeapon.cs
│   │   │   │   ├── BaseWeapon.cs
│   │   │   │   ├── WeaponDecorator.cs
│   │   │   │   ├── FireWeaponDecorator.cs
│   │   │   │   └── IceWeaponDecorator.cs
│   │   │   ├── Facade/
│   │   │   │   └── GameFacade.cs
│   │   │   ├── Observer/
│   │   │   │   └── HUDController.cs
│   │   │   ├── State/
│   │   │   │   ├── IPlayerState.cs
│   │   │   │   ├── PlayerStateMachine.cs
│   │   │   │   ├── NormalState.cs
│   │   │   │   ├── InvulnerableState.cs
│   │   │   │   └── DeadState.cs
│   │   │   └── Command/
│   │   │       ├── IGameCommand.cs
│   │   │       ├── MoveCommand.cs
│   │   │       ├── ShootCommand.cs
│   │   │       ├── UseSkillCommand.cs
│   │   │       └── PauseCommand.cs
│   │   ├── Gameplay/
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerInputReader.cs
│   │   │   ├── PlayerHealth.cs
│   │   │   ├── Enemy.cs
│   │   │   ├── EnemySpawner.cs
│   │   │   ├── Projectile.cs
│   │   │   ├── WeaponController.cs
│   │   │   ├── SkillController.cs
│   │   │   ├── ExperienceSystem.cs
│   │   │   └── UpgradeSystem.cs
│   │   └── UI/
│   │       ├── GameUIController.cs
│   │       ├── UpgradeSelectionUI.cs
│   │       └── PatternDebugPanel.cs
│   ├── Prefabs/
│   │   ├── Player.prefab
│   │   ├── Enemy_Normal.prefab
│   │   ├── Enemy_Fast.prefab
│   │   ├── Enemy_Tank.prefab
│   │   └── Projectile.prefab
│   ├── ScriptableObjects/
│   ├── Sprites/
│   └── Editor/
│       └── PatternSurvivorProjectBuilder.cs
├── Packages/
├── ProjectSettings/
├── README.md
└── docs/
    ├── PATRONES.md
    ├── PRUEBAS.md
    └── ARQUITECTURA.md
```

**Nota:** La lista representa componentes previstos; añade archivos indispensables (`SkillContext`, `FireContext`, eventos, configuración, etc.) si el código lo necesita. No fuerces un archivo por cada clase trivial si empeora el mantenimiento.

---

## 7. Configuración Unity, escena y automatización

**Requisito crítico:** Codex debe entregar una escena configurada, **no una colección de scripts que requiera conectar manualmente decenas de referencias**.

1. Usa una sola escena para menú, partida, pausa, mejoras, derrota y panel de patrones, activando y desactivando paneles según fase.
2. Crea `Main Camera` ortográfica centrada y arena delimitada visual y físicamente.
3. Jugador: `SpriteRenderer`, `Rigidbody2D` (sin gravedad, rotación bloqueada), collider compatible y componentes `PlayerController`, `PlayerHealth`, `PlayerStateMachine`, etc.
4. Enemigos: `SpriteRenderer`, collider 2D, scripts de movimiento/persecución, daño y configuraciones.
5. Proyectiles: collider con trigger, desplazamiento estable con `Time.deltaTime` o física coherente, daño contra enemigo y autodestrucción por tiempo.
6. Colisiones: configura capas o validaciones para que los proyectiles no dañen al jugador y los enemigos no hagan daño entre sí por accidente.
7. UI Canvas con escalado adecuado y botones conectados; no debe haber `Missing (Mono Script)`.
8. Si crear assets `.unity`/`.prefab` correctamente desde código externo no es fiable, escribe un **script de editor** con un menú como `Tools > Pattern Survivor > Generate Demo` que construya escena, prefabs, datos compartidos, HUD y conexiones mediante `UnityEditor`, guardando los assets. Documenta los pasos exactos de una sola ejecución.
9. Idealmente ejecuta el generador automáticamente en la primera apertura mediante un procedimiento seguro e idempotente, **sin sobrescribir cambios manuales**; como mínimo el menú editor debe funcionar de forma fiable.
10. Incluye escena en `Build Settings` / configuración de build equivalente para la versión de Unity elegida.
11. No incluyas bibliotecas o assets ajenos con licencias inciertas.

**Restricción de archivos:** Nunca edites `.meta` al azar ni reutilices GUIDs duplicados; respeta convenciones de Unity. Mantén serialización válida y referencias estables.

---

## 8. Orden recomendado de implementación

Realiza las tareas en este orden para controlar dependencias y compilar progresivamente:

**Etapa A — Base jugable**

1. Inspecciona el repositorio y detecta si existe proyecto Unity.
2. Prepara estructura, escena/cámara/arena y controles.
3. Implementa jugador, enemigos, proyectiles, colisiones, daño y puntuación.
4. Implementa menús, HUD, pausa, derrota y reinicio.
5. **Valida que el juego básico ya se puede jugar.**

**Etapa B — Patrones creacionales**

6. Singleton GameManager.
7. Factory Method con creadores polimórficos.
8. Prototype de configuraciones de enemigos.
9. Builder de armas.

**Etapa C — Patrones estructurales**

10. Flyweight de datos compartidos de enemigos.
11. Composite de habilidades (Q).
12. Decorator de mejoras de ataque.
13. Facade que coordine las operaciones de ciclo de vida.

**Etapa D — Patrones de comportamiento**

14. Observer con eventos y UI desacoplada.
15. State de jugador.
16. Command de entrada.

**Etapa E — Integración y evidencia**

17. Experiencia, nivel y mejoras con interfaz funcional.
18. Panel interactivo de los 11 patrones y registro de eventos reales.
19. Verifica los siete recorridos funcionales descritos en sección 4.
20. Documenta, ejecuta pruebas y corrige errores.

La secuencia es orientativa: puedes introducir patrones antes si así reduces retrabajo.

---

## 9. Criterios de aceptación (Definition of Done)

No declares la tarea terminada hasta satisfacer esta lista:

### Jugabilidad

- [ ] La escena principal abre sin referencias faltantes.
- [ ] Al iniciar Play aparece un menú funcional.
- [ ] Jugar inicia una partida con jugador controlable.
- [ ] WASD/flechas mueve correctamente en 8 direcciones.
- [ ] Mouse orienta y clic dispara.
- [ ] Los proyectiles impactan y matan enemigos.
- [ ] Aparecen enemigos normal, rápido y tanque.
- [ ] Los enemigos persiguen al jugador.
- [ ] Recibir daño reduce HP y activa invulnerabilidad temporal.
- [ ] La UI cambia al recibir daño, ganar puntos y subir de nivel.
- [ ] Se puede elegir una mejora con efecto real.
- [ ] `Q` ejecuta habilidad combinada.
- [ ] Esc pausa y reanuda correctamente.
- [ ] Al llegar a 0 HP aparece Game Over.
- [ ] Reiniciar devuelve HP/puntos/nivel/oleada a valores iniciales, limpia objetos y no duplica managers ni listeners.

### Verificación de los patrones

- [ ] **Singleton:** solo hay un `GameManager` activo.
- [ ] **Factory Method:** creadores concretos polimórficos construyen enemigos distintos.
- [ ] **Prototype:** el clon puede modificarse sin alterar su original.
- [ ] **Builder:** se configuran al menos dos armas mediante la misma API de construcción.
- [ ] **Flyweight:** varios enemigos comparten la misma referencia de datos intrínsecos sin compartir HP individual.
- [ ] **Composite:** una habilidad compuesta ejecuta al menos dos hijos útiles.
- [ ] **Decorator:** una o varias envolturas agregan efectos reales a un arma base.
- [ ] **Facade:** botones operan a través de operaciones de alto nivel sobre subsistemas.
- [ ] **Observer:** cambios de salud/puntos actualizan HUD mediante eventos, no llamadas directas del modelo.
- [ ] **State:** la invulnerabilidad y muerte se rigen por transiciones reales.
- [ ] **Command:** input ejecuta objetos comando, no llamadas directas a las acciones del jugador.
- [ ] **Panel:** se pueden consultar los 11 patrones y hay evidencia real asociada a cada uno cuando se activa.

### Calidad

- [ ] No hay errores de compilación C#.
- [ ] No se producen `NullReferenceException` o errores persistentes al entrar, jugar, pausar, morir y reiniciar.
- [ ] No quedan scripts/clases sin uso creados solo para «cumplir la lista».
- [ ] Las dependencias y referencias de escena están resueltas.
- [ ] Se incluyen instrucciones reproducibles para ejecutar en Unity.
- [ ] Se incluyen comprobaciones técnicas o tests de las reglas más delicadas.

---

## 10. Pruebas recomendadas

### 10.1 Pruebas automáticas cuando sea posible

Agrega pruebas de Unity Test Framework (EditMode para C# puro y PlayMode cuando dependan de escena); si el paquete no está disponible o el entorno no ejecuta Unity, **no finjas haberlas corrido**. Por lo menos prepara pruebas para:

1. Prototype: `Clone()` crea un objeto distinto, y modificar el clon no cambia el original.
2. Builder: `Build()` produce configuración válida y dos builds independientes.
3. Flyweight: mismo tipo retorna la misma instancia; tipos distintos no comparten referencia.
4. Composite: al ejecutar conjunto, se llama exactamente una vez a cada hijo.
5. Decorator: ejecutar un arma decorada invoca arma interna y aplica efecto extra.
6. State: daño inicial activa invulnerabilidad; daño inmediato adicional se ignora; muerte bloquea controles.
7. Observer: el evento se emite una vez al cambiar un valor y no deja suscripciones al desactivar.
8. Facade: iniciar/reiniciar restablece datos sin producir duplicados.
9. Factory Method: cada creador genera tipo correcto usando dependencias de prueba o prefabs de test.
10. Command: ejecutar un comando invoca el receptor esperado, y el input no depende de una implementación concreta.

### 10.2 Pruebas manuales guiadas

| Prueba | Acción | Resultado esperado |
|---|---|---|
| 01 | Ejecutar escena | Menú visible, sin errores |
| 02 | Jugar, mover y disparar | Controles fluidos, disparos visibles |
| 03 | Esperar varias apariciones | Enemigos de tipos diferentes |
| 04 | Matar enemigos | Suman puntos y experiencia |
| 05 | Dejar que golpeen al jugador | HP desciende una vez por ventana de invulnerabilidad |
| 06 | Elegir mejora de fuego/hielo | El efecto modifica el combate |
| 07 | Pulsar Q cerca de enemigos | Se activa curación + daño de área según reglas |
| 08 | Pulsar Esc | Pausa y reanudación correctas |
| 09 | Dejar morir al jugador | Pantalla de derrota y controles deshabilitados |
| 10 | Reiniciar dos veces | Valores iniciales; sin duplicaciones ni excepciones |
| 11 | Abrir el panel de patrones | Se muestran nombres, clases y evidencias válidas |

Guarda resultados de pruebas en `docs/PRUEBAS.md`, indicando **verificado** o **no ejecutado** según corresponda.

---

## 11. Documentación académica obligatoria

### `README.md`

Incluye:

- Descripción breve del juego y captura solo si existe una captura real.
- Versión de Unity utilizada o probada.
- Cómo abrir el proyecto en Unity Hub.
- Cómo generar escena/prefabs si se usa menú editor.
- Cómo iniciar, jugar y reiniciar.
- Controles completos.
- Cómo abrir el panel «11 patrones».
- Limitaciones conocidas y cualquier paso manual inevitable.

### `docs/PATRONES.md`

Para **cada uno** de los 11 patrones, escribe:

1. Nombre y clasificación.
2. Problema que resuelve.
3. Participantes (interfaz/clase principal, clases concretas, cliente).
4. Archivo/ruta real del código.
5. Explicación específica de cómo se usa dentro de la partida.
6. Fragmento de código real y breve (copiado del proyecto, no pseudocódigo inventado).
7. Acción exacta en el videojuego para comprobar su funcionamiento.
8. Ventajas y limitaciones de haberlo usado en este proyecto.

Incluye también una **matriz de trazabilidad**:

| Patrón | Clase(s) | Acción disparadora | Evidencia en UI/log | Prueba |
|---|---|---|---|---|
| Singleton | ... | ... | ... | ... |
| Factory Method | ... | ... | ... | ... |
| Prototype | ... | ... | ... | ... |
| Builder | ... | ... | ... | ... |
| Flyweight | ... | ... | ... | ... |
| Composite | ... | ... | ... | ... |
| Decorator | ... | ... | ... | ... |
| Facade | ... | ... | ... | ... |
| Observer | ... | ... | ... | ... |
| State | ... | ... | ... | ... |
| Command | ... | ... | ... | ... |

Rellena con **rutas y pruebas reales**, sin `...` en la entrega final.

### `docs/ARQUITECTURA.md`

- Diagrama Mermaid de dependencias principales.
- Diagrama Mermaid de clases o secuencia para generación de enemigos, disparo y daño al jugador.
- Distingue `EnemyPrototype` vs `EnemySharedData` claramente.
- Distingue Builder vs Decorator claramente.
- Justifica el uso del patrón Command y el State de jugador.
- Explica por qué cada patrón está involucrado en una característica jugable, no solo en un ejemplo.

---

## 12. Requisitos de calidad y precauciones Unity/C#

- Mantén namespaces coherentes, por ejemplo `PatternSurvivor.Core`, `.Gameplay`, `.Patterns.*`, `.UI`.
- C# compatible con la versión de Unity del proyecto, sin APIs obsoletas si existe alternativa estable.
- Prefiere `SerializeField` y referencias claras frente a cadenas mágicas o búsquedas repetitivas con `FindObjectOfType`.
- Respeta `Time.deltaTime`, `Time.timeScale` y cuándo usar `unscaledDeltaTime` (menús/pausa).
- Evita llamar `GetComponent` continuamente en `Update` si puede hacerse en `Awake`.
- Si se utiliza `Instantiate`/`Destroy`, limita objetos en escena y limpia al reiniciar.
- Los callbacks de eventos deben limpiarse correctamente en `OnDisable` o al desechar objetos.
- No mezcles referencias a `UnityEditor` en scripts que se compilan para runtime; los generadores deben vivir dentro de `Assets/Editor`.
- Evita usar `new` para `MonoBehaviour`.
- La generación de escena/prefabs debe ser segura al reejecutarse (idempotente o advertir antes de sobrescribir).
- Si un sistema usa `Time.timeScale = 0`, restaura a `1` al empezar nueva partida y al salir de pausa; garantiza que la UI siga interactiva.
- Define orden de inicialización robusto, sin depender del orden arbitrario de `Awake` cuando haya dependencias.
- No suplantes tests con logs. Los logs son **observabilidad**, no prueba de funcionamiento por sí solos.
- Nada de servicios online, autenticación, telemetry, anuncios, paquetes pagos ni audio con derechos no autorizados.

---

## 13. Entregables finales

Al terminar, deja en el repositorio:

1. **Proyecto Unity completo** o modificaciones sobre el proyecto existente: `Assets/`, `Packages/`, `ProjectSettings/`, escenas y componentes necesarios.
2. Una escena jugable y recursos autocontenidos; si la escena se genera por menú de editor, generador estable e instrucciones exactas.
3. Los **11 patrones operativos**, utilizados en el flujo de juego real.
4. Panel de evidencias de patrones integrado.
5. `README.md` con instalación, controles, ejecución y estado real de pruebas.
6. `docs/PATRONES.md`, `docs/ARQUITECTURA.md`, `docs/PRUEBAS.md`.
7. Pruebas automatizadas cuando se puedan ejecutar y pruebas manuales descritas sin afirmar resultados no comprobados.

### Forma en que Codex debe responder al finalizar

Escribe un resumen con:

- **Archivos creados o modificados.**
- **Pasos exactos para abrir y ejecutar el juego.**
- **Tabla de los 11 patrones con ruta de la implementación y cómo comprobar cada uno.**
- **Qué compilaste o probaste realmente y con qué resultado.** Si no tienes Unity Editor disponible, dilo explícitamente y no asegures que el proyecto compila: indica lo que queda por validar en la máquina del usuario.
- **Limitaciones o errores pendientes**, si los hay, sin ocultarlos.

## 14. Orden final para Codex

**Empieza a implementar ahora.** Inspecciona el repositorio, reutiliza lo existente cuando convenga y desarrolla el prototipo. No te detengas después de generar la estructura de carpetas ni entregues solo pseudocódigo. Prioriza el núcleo jugable, integra los 11 patrones correctamente, incluye la demo verificable y documenta. Cuando no dispongas del editor de Unity en tu entorno, crea todos los archivos, herramientas de generación y pruebas posibles, y explica con precisión las validaciones pendientes en el editor sin afirmar que ya se ejecutaron.
