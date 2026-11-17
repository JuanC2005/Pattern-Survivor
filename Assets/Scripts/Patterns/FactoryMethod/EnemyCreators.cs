using System;
using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using PatternSurvivor.Patterns.Flyweight;
using PatternSurvivor.Patterns.Prototype;
using UnityEngine;

namespace PatternSurvivor.Patterns.FactoryMethod
{
    public abstract class EnemyCreator
    {
        protected EnemyCreator(Transform parent, PlayerController target, int wave, Action<Enemy> defeatedCallback)
        {
            Parent = parent;
            Target = target;
            Wave = wave;
            DefeatedCallback = defeatedCallback;
        }

        protected Transform Parent { get; }
        protected PlayerController Target { get; }
        protected int Wave { get; }
        protected Action<Enemy> DefeatedCallback { get; }

        public abstract Enemy Create(Vector2 position);

        protected Enemy CreateFromPrototype(string typeName, Vector2 position)
        {
            EnemyPrototype original = EnemyPrototypeCatalog.Get(typeName);
            EnemyPrototype clone = original.Clone();
            float multiplier = 1f + Mathf.Max(0, Wave - 1) * 0.05f;
            clone.CurrentHealth = Mathf.RoundToInt(clone.CurrentHealth * multiplier);
            clone.MoveSpeed *= 1f + Mathf.Max(0, Wave - 1) * 0.01f;

            GameObject enemyObject = new GameObject($"{clone.TypeName}Enemy");
            enemyObject.transform.SetParent(Parent);
            enemyObject.transform.position = position;
            Enemy enemy = enemyObject.AddComponent<Enemy>();
            EnemySharedData sharedData = EnemyDataCache.Get(clone.TypeName);
            enemy.Initialize(clone, sharedData, Target, DefeatedCallback);

            PatternEventLogger.Record(
                "Factory Method",
                $"{GetType().Name} creó {clone.TypeName}Enemy");
            PatternEventLogger.Record(
                "Prototype",
                $"copia de {original.TypeName}Prototype (HP ajustado {original.CurrentHealth} -> {clone.CurrentHealth})");
            return enemy;
        }
    }

    public sealed class NormalEnemyCreator : EnemyCreator
    {
        public NormalEnemyCreator(Transform parent, PlayerController target, int wave, Action<Enemy> defeatedCallback)
            : base(parent, target, wave, defeatedCallback) { }

        public override Enemy Create(Vector2 position) => CreateFromPrototype("Normal", position);
    }

    public sealed class FastEnemyCreator : EnemyCreator
    {
        public FastEnemyCreator(Transform parent, PlayerController target, int wave, Action<Enemy> defeatedCallback)
            : base(parent, target, wave, defeatedCallback) { }

        public override Enemy Create(Vector2 position) => CreateFromPrototype("Fast", position);
    }

    public sealed class TankEnemyCreator : EnemyCreator
    {
        public TankEnemyCreator(Transform parent, PlayerController target, int wave, Action<Enemy> defeatedCallback)
            : base(parent, target, wave, defeatedCallback) { }

        public override Enemy Create(Vector2 position) => CreateFromPrototype("Tank", position);
    }
}
