# Pruebas

## Estado actual

Esta lista se actualiza con resultados observados en Unity Editor. No se marcan como verificadas pruebas que no se hayan ejecutado.

| Prueba | Resultado | Procedimiento |
|---|---|---|
| 01. Ejecutar escena | Verificado por smoke test | `PatternSurvivorSmokeTest` abrió `GameScene`, entró en Play y encontró menú inicial. |
| 02. Mover y disparar | Pendiente | Usar WASD, mouse y Espacio. |
| 03. Variantes de enemigo | Parcial | Smoke test verificó apariciones y Factory/Prototype; falta una sesión manual de duración suficiente para observar Fast y Tank en pantalla. |
| 04. Puntos y experiencia | Pendiente | Eliminar enemigos. |
| 05. Daño e invulnerabilidad | Verificado por smoke test | Primer daño dejó HP en 90 y el segundo inmediato fue ignorado por `InvulnerableState`. |
| 06. Mejoras de fuego/hielo | Pendiente | Subir de nivel y disparar. |
| 07. Habilidad Composite | Parcial | Smoke test ejecutó `HealSkill`, `AreaBlastSkill`, `CompositeSkill` y verificó la creación de `SkillPulse`; falta comprobar visualmente el pulso y el daño de área usando Q con enemigos cercanos. |
| 08. Pausa | Verificado parcialmente | Smoke test comprobó abrir/cerrar el panel de patrones con pausa y reanudar; falta probar Esc manualmente. |
| 09. Game Over | Pendiente | Dejar llegar la vida a cero. |
| 10. Reinicio repetido | Verificado parcialmente | Smoke test ejecutó `RestartGame` y comprobó fase Playing sin duplicar el manager; falta repetirlo dos veces desde UI. |
| 11. Panel de patrones | Verificado parcialmente | Smoke test abrió el panel y la escena no produjo excepciones; falta recorrer manualmente las 11 entradas y sus botones seguros. |

## Tests automatizados

El proyecto incluye Unity Test Framework en `Packages/manifest.json`. La ejecución EditMode del 16/11/2026 terminó con **5/5 tests aprobados** en `TestResults/editmode-results.xml`: Prototype, Builder, Flyweight, Composite y Decorator.

## Validaciones de calidad

- [x] Compilación C# sin errores.
- [x] Sin `Missing Script` en `GameScene` durante smoke test posterior a la regeneración.
- [x] Sin `NullReferenceException` en el recorrido cubierto por smoke test: menú, partida, pausa de patrones y reinicio.
- [x] No se duplicó `GameManager` durante el smoke test de reinicio.
- [x] Build Windows standalone generada correctamente en validación temporal; no se incluye el binario en el repositorio.

## Comandos ejecutados

```text
Unity.exe -batchmode -nographics -projectPath "D:\Proyectos Unity\Pattern Survivor" -executeMethod PatternSurvivor.Editor.PatternSurvivorProjectBuilder.GenerateDemo
Unity.exe -batchmode -nographics -projectPath "D:\Proyectos Unity\Pattern Survivor" -executeMethod PatternSurvivor.Editor.PatternSurvivorSmokeTest.Run
Unity.exe -batchmode -nographics -projectPath "D:\Proyectos Unity\Pattern Survivor" -runTests -testPlatform editmode -testResults "TestResults/editmode-results.xml"
Unity.exe -batchmode -nographics -quit -projectPath "D:\Proyectos Unity\Pattern Survivor" -buildTarget Win64 -buildWindows64Player "PatternSurvivor.exe"
```
