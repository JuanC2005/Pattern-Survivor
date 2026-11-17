using System.Collections.Generic;
using PatternSurvivor.Core;
using PatternSurvivor.Patterns.Decorator;
using UnityEngine;

namespace PatternSurvivor.Gameplay
{
    public sealed class Projectile : MonoBehaviour
    {
        private static readonly List<Projectile> ActiveProjectiles = new List<Projectile>();

        private Vector2 direction;
        private int damage;
        private float speed;
        private float remainingLifetime;
        private ProjectileEffects effects;

        public void Configure(Vector2 movementDirection, int projectileDamage, float projectileSpeed, float lifetime, ProjectileEffects projectileEffects)
        {
            direction = movementDirection.normalized;
            damage = projectileDamage;
            speed = projectileSpeed;
            remainingLifetime = lifetime;
            effects = projectileEffects;
            ActiveProjectiles.Add(this);
            gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (GameManagerProxy.Phase != GamePhase.Playing)
            {
                return;
            }

            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f || !GameManagerProxy.Bounds.Contains(transform.position))
            {
                Destroy(gameObject);
                return;
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.13f);
            foreach (Collider2D hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null)
                {
                    continue;
                }

                enemy.TakeDamage(damage, "Projectile");
                if ((effects & ProjectileEffects.Burn) != 0)
                {
                    enemy.ApplyBurn(3f, 2);
                    PatternEventLogger.Record("Decorator", "Burn aplicado al objetivo por FireWeaponDecorator");
                }

                if ((effects & ProjectileEffects.Freeze) != 0)
                {
                    enemy.ApplyFreeze(2.5f, 0.45f);
                    PatternEventLogger.Record("Decorator", "Freeze aplicado al objetivo por IceWeaponDecorator");
                }

                Destroy(gameObject);
                return;
            }
        }

        private void OnDestroy() => ActiveProjectiles.Remove(this);

        public static void ClearAll()
        {
            Projectile[] snapshot = ActiveProjectiles.ToArray();
            foreach (Projectile projectile in snapshot)
            {
                if (projectile != null)
                {
                    Destroy(projectile.gameObject);
                }
            }
        }

        private static class GameManagerProxy
        {
            public static GamePhase Phase => Patterns.Singleton.GameManager.Instance == null
                ? GamePhase.Menu
                : Patterns.Singleton.GameManager.Instance.Phase;

            public static Rect Bounds => Patterns.Singleton.GameManager.Instance == null
                ? new Rect(-8f, -4.5f, 16f, 9f)
                : Patterns.Singleton.GameManager.Instance.ArenaBounds;
        }
    }
}
