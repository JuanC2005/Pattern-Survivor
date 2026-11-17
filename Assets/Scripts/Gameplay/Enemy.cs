using System;
using System.Collections.Generic;
using PatternSurvivor.Core;
using PatternSurvivor.Infrastructure;
using PatternSurvivor.Patterns.Flyweight;
using PatternSurvivor.Patterns.Prototype;
using UnityEngine;

namespace PatternSurvivor.Gameplay
{
    public sealed class Enemy : MonoBehaviour
    {
        private static readonly List<Enemy> ActiveEnemies = new List<Enemy>();

        private EnemyPrototype configuration;
        private EnemySharedData sharedData;
        private PlayerController target;
        private Action<Enemy> defeatedCallback;
        private SpriteRenderer spriteRenderer;
        private float contactCooldown;
        private float burnRemaining;
        private float burnTickTimer;
        private int burnDamage;
        private float freezeRemaining;
        private float freezeMultiplier = 1f;
        private bool defeated;

        public EnemySharedData SharedData => sharedData;
        public int CurrentHealth { get; private set; }
        public int PointValue => configuration == null ? 0 : configuration.PointValue;
        public int ExperienceValue => configuration == null ? 0 : configuration.ExperienceValue;
        public static int ActiveCount
        {
            get
            {
                ActiveEnemies.RemoveAll(enemy => enemy == null);
                return ActiveEnemies.Count;
            }
        }

        public static IReadOnlyList<Enemy> Active => ActiveEnemies;

        public void Initialize(EnemyPrototype prototype, EnemySharedData data, PlayerController player, Action<Enemy> onDefeated)
        {
            configuration = prototype;
            sharedData = data;
            target = player;
            defeatedCallback = onDefeated;
            CurrentHealth = prototype.CurrentHealth;
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = RuntimeSpriteLibrary.GetCircle(data.TypeName, data.Color, data.TypeName == "Tank" ? 42 : 32);
            RuntimeSpriteLibrary.ApplySpriteMaterial(spriteRenderer);
            spriteRenderer.sortingOrder = 5;
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = data.TypeName == "Tank" ? 0.65f : 0.45f;
            ActiveEnemies.Add(this);
            GameEvents.RaiseEnemySpawned(this);
        }

        private void Update()
        {
            if (defeated || target == null || GameManagerProxy.Phase != GamePhase.Playing)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            if (freezeRemaining > 0f)
            {
                freezeRemaining -= deltaTime;
                if (freezeRemaining <= 0f)
                {
                    freezeMultiplier = 1f;
                }
            }

            if (burnRemaining > 0f)
            {
                burnRemaining -= deltaTime;
                burnTickTimer -= deltaTime;
                if (burnTickTimer <= 0f)
                {
                    burnTickTimer = 0.75f;
                    TakeDamage(burnDamage, "Burn");
                }
            }

            if (defeated)
            {
                return;
            }

            Vector2 direction = (target.transform.position - transform.position).normalized;
            transform.position += (Vector3)(direction * configuration.MoveSpeed * freezeMultiplier * deltaTime);

            contactCooldown -= deltaTime;
            if (contactCooldown <= 0f && Vector2.Distance(transform.position, target.transform.position) <= 0.75f)
            {
                target.Health.TakeDamage(configuration.ContactDamage);
                contactCooldown = 0.8f;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = freezeRemaining > 0f
                    ? new Color(0.45f, 0.85f, 1f)
                    : burnRemaining > 0f ? new Color(1f, 0.4f, 0.15f) : sharedData.Color;
            }
        }

        public void TakeDamage(int damage, string source)
        {
            if (defeated || damage <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            if (CurrentHealth > 0)
            {
                return;
            }

            defeated = true;
            PatternEventLogger.Record("Observer", $"EnemyHealth de {sharedData.TypeName} llegó a 0 por {source}");
            defeatedCallback?.Invoke(this);
            Destroy(gameObject);
        }

        public void ApplyBurn(float duration, int damagePerTick)
        {
            burnRemaining = Mathf.Max(burnRemaining, duration);
            burnDamage = Mathf.Max(burnDamage, damagePerTick);
        }

        public void ApplyFreeze(float duration, float speedMultiplier)
        {
            freezeRemaining = Mathf.Max(freezeRemaining, duration);
            freezeMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 1f);
        }

        private void OnDestroy() => ActiveEnemies.Remove(this);

        private static class GameManagerProxy
        {
            public static GamePhase Phase => Patterns.Singleton.GameManager.Instance == null
                ? GamePhase.Menu
                : Patterns.Singleton.GameManager.Instance.Phase;
        }
    }
}
