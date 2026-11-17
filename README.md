# Pattern Survivor

Prototipo arcade 2D cenital para Unity 6. El jugador sobrevive a oleadas, dispara, consigue experiencia y elige mejoras. El proyecto integra de forma observable Singleton, Factory Method, Prototype, Builder, Flyweight, Composite, Decorator, Facade, Observer, State y Command.

## Versión

- Unity `6000.5.9f1`.
- Renderizado URP 2D existente del proyecto.
- Entrada mediante el nuevo Input System ya activo en `ProjectSettings/ProjectSettings.asset`.
- No utiliza assets externos, servicios online ni paquetes de pago.

## Preparación

1. Abre la carpeta desde Unity Hub con Unity `6000.5.9f1`.
2. Espera a que termine la importación de paquetes.
3. Si `Assets/Scenes/GameScene.unity` todavía no existe, abre el menú `Tools > Pattern Survivor > Generate Demo`.
4. Abre `Assets/Scenes/GameScene.unity` y pulsa Play.

El generador crea una escena única, cámara ortográfica, luz 2D, EventSystem, sistemas de juego y UI. Si la escena ya existe, solicita confirmación antes de regenerarla.

## Controles

- `WASD` o flechas: mover en ocho direcciones.
- Clic izquierdo o `Espacio`: disparar hacia el cursor.
- `Q`: curación más explosión de área, con cooldown.
- `Esc`: pausar o reanudar.
- Botón `PATRONES`: abrir el panel de evidencia.

## Flujo de juego

`JUGAR` inicia la partida. Los enemigos normal, rápido y tanque aparecen alrededor de la arena, persiguen al jugador y entregan puntos y experiencia. Al subir de nivel se detiene el tiempo y aparecen tres mejoras. Al llegar a cero de vida aparece Game Over; `REINTENTAR` limpia objetos y estadísticas.

## Panel de patrones

Desde el menú, el HUD o la pausa pulsa `VER PATRONES` / `PATRONES`. Selecciona cualquiera de los 11 patrones para ver su clasificación, clases, uso, acción de comprobación y el último evento real registrado. El botón `PROBAR` solo está habilitado para demostraciones seguras que ejecutan operaciones reales.

## Estructura relevante

- `Assets/Scripts/Core`: fases, eventos y registro académico.
- `Assets/Scripts/Gameplay`: jugador, enemigos, proyectiles, oleadas, experiencia y mejoras.
- `Assets/Scripts/Patterns`: implementaciones de los 11 patrones.
- `Assets/Scripts/UI`: HUD, pantallas y panel de evidencias.
- `Assets/Editor/PatternSurvivorProjectBuilder.cs`: generador de escena.
- `docs/PATRONES.md`: explicación y trazabilidad de patrones.
- `docs/ARQUITECTURA.md`: diagramas y decisiones.
- `docs/PRUEBAS.md`: estado de comprobaciones.

## Limitaciones conocidas

La escena y sus objetos de gameplay se construyen en runtime desde una escena generada por editor para evitar referencias manuales frágiles. La validación visual, los tests del Test Runner y la build Windows deben comprobarse en el Unity Editor de la máquina de desarrollo.
