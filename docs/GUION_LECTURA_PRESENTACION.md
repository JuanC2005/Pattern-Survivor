# Guion breve de presentación

## Antes de empezar

Este guion está pensado para leerlo mientras se juega. Las instrucciones entre corchetes indican qué hacer en pantalla. No es necesario abrir archivos de código durante la explicación principal.

## 1. Presentación

[Abro la escena, pulso Play y espero a que aparezca el menú.]

Buenos días. Voy a presentar **Pattern Survivor**, un juego arcade 2D de supervivencia.

El jugador debe sobrevivir a oleadas de enemigos, disparar, conseguir puntos, subir de nivel y elegir mejoras. Durante la partida voy a demostrar once patrones de diseño integrados en acciones reales del juego.

La demostración seguirá el recorrido natural de una partida: comenzar, sobrevivir, combatir, mejorar el arma, recibir daño y utilizar la habilidad especial.

## 2. Inicio: Singleton y Facade

[Pulso `JUGAR`.]

Al iniciar la partida podemos ver dos patrones trabajando juntos.

El primero es **Singleton**. La partida necesita un único coordinador para controlar la fase actual, el tiempo, la puntuación, las eliminaciones y la oleada. Gracias a esto no aparecen dos partidas diferentes controlando el mismo juego.

El segundo es **Facade**. Desde este botón inicio toda la partida con una sola acción. El juego activa al jugador, prepara las oleadas, reinicia las estadísticas, pone el tiempo en marcha y muestra el HUD.

La idea es que la interfaz no tenga que coordinar cada sistema por separado. Pulso un botón y la fachada organiza todo el inicio.

## 3. Enemigos: Factory Method, Prototype y Flyweight

[Espero hasta que aparezcan enemigos.]

Ahora empiezan a aparecer enemigos alrededor de la arena.

Aquí podemos observar **Factory Method**. El juego puede crear enemigos normales, rápidos y tanques sin utilizar exactamente el mismo proceso para todos. Cada variante tiene su propia forma de creación y sus propias estadísticas.

También aparece **Prototype**. Los enemigos parten de una configuración original, pero cada aparición utiliza una copia que puede modificarse. Por eso la dificultad puede aumentar con las oleadas sin cambiar los datos de los enemigos que ya existían.

Finalmente tenemos **Flyweight**. Los enemigos del mismo tipo comparten la información que no cambia, como su color o sus valores base. Sin embargo, cada enemigo conserva su propia posición y su propia vida.

Así se pueden tener muchos enemigos en pantalla sin repetir innecesariamente los mismos datos.

## 4. Movimiento y disparo: Command y Builder

[Muevo con WASD o flechas y disparo con clic izquierdo.]

Al moverme y disparar aparece **Command**.

El teclado y el mouse no ejecutan directamente las acciones del jugador. Cada entrada se convierte en una orden: mover, disparar, usar la habilidad o pausar. Esto permite separar los controles de las acciones que realizan.

También podemos observar **Builder** en el arma.

El arma se construye con varias características: daño, cadencia, velocidad y cantidad de proyectiles. En lugar de crearla con una configuración difícil de leer, se construye paso a paso. Por eso es posible tener perfiles como un arma básica, una rápida o una pesada.

## 5. Mejoras: Decorator

[Sigo jugando hasta subir de nivel y aparece la selección de mejoras.]

Al subir de nivel el tiempo se detiene y puedo elegir una mejora.

[Selecciono fuego o hielo y vuelvo a disparar.]

Aquí aparece **Decorator**.

El arma original sigue existiendo, pero ahora se le añade una capa de comportamiento. Con fuego, los proyectiles pueden quemar y causar daño adicional. Con hielo, pueden ralentizar al enemigo.

La ventaja es que los efectos se pueden combinar sin crear una clase diferente para cada posible combinación de mejoras.

En pantalla puedo comprobar que la mejora no es solamente un texto: cambia el color del arma, cambia el color de los proyectiles y modifica el comportamiento del enemigo impactado.

## 6. Daño: Observer y State

[Dejo que un enemigo golpee al jugador.]

Cuando recibo daño aparecen **Observer** y **State**.

Observer permite que el cambio de vida se comunique automáticamente al HUD. La barra y el texto de vida se actualizan porque la partida avisa a los elementos que están observando ese cambio.

State controla el comportamiento del jugador después del golpe.

Primero el jugador está en estado normal. Después del impacto pasa a un estado temporal de invulnerabilidad. Si recibe otro golpe inmediatamente, ese golpe se ignora. Esto evita perder toda la vida en un solo instante.

[Recibo otro golpe rápidamente.]

Si la vida llega a cero, el jugador pasa al estado muerto. En ese momento ya no puede moverse ni disparar y aparece la pantalla de Game Over.

## 7. Habilidad especial: Composite

[Me acerco a varios enemigos y pulso `Q`.]

La habilidad especial demuestra **Composite**.

Para el jugador es una sola acción, pero internamente combina dos efectos: una pequeña curación y una explosión de área que daña a los enemigos cercanos.

[Señalo el pulso visual y el cooldown del HUD.]

El pulso visual muestra que la habilidad se activó realmente. El cooldown evita que pueda utilizarse continuamente.

Este patrón permite tratar una habilidad sencilla y una habilidad compuesta de la misma manera.

## 8. Panel final de evidencias

[Abro el panel `11 PATRONES`.]

Este panel resume los patrones que se han activado durante la partida. No muestra evidencias inventadas: cada mensaje aparece cuando ocurre una acción real.

Aquí puedo seleccionar cada patrón, leer qué problema resuelve y consultar el último evento registrado.

También puedo utilizar las demostraciones seguras del panel para comprobar algunos patrones sin interrumpir la partida.

## 9. Cierre

Durante el recorrido hemos visto que Singleton y Facade organizan el inicio; Factory Method, Prototype y Flyweight gestionan la creación de enemigos; Command y Builder controlan la entrada y la construcción del arma; Decorator añade mejoras; Observer y State gestionan el daño; y Composite ejecuta la habilidad especial.

La conclusión es que los patrones no están incluidos solamente para cumplir una lista. Cada uno participa en una mecánica que el jugador puede ejecutar y observar.

Pattern Survivor demuestra que una arquitectura basada en patrones puede mantenerse separada y, al mismo tiempo, formar parte de un juego funcional.

## Plan de emergencia

Si Q parece no hacer nada, hago clic dentro de la ventana Game, compruebo que la habilidad esté lista y me acerco a un enemigo.

Si la vida está completa, la curación no se nota en el número, pero el pulso y el cooldown deben aparecer.

Si un patrón todavía no tiene evidencia, realizo primero la acción correspondiente y vuelvo a abrir su entrada en el panel.
