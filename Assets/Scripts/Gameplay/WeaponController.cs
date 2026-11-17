using PatternSurvivor.Core;
using PatternSurvivor.Infrastructure;
using PatternSurvivor.Patterns.Builder;
using PatternSurvivor.Patterns.Decorator;
using UnityEngine;

namespace PatternSurvivor.Gameplay
{
    public sealed class WeaponController : MonoBehaviour
    {
        private PlayerController player;
        private IWeapon weapon;
        private WeaponConfig currentConfig;
        private bool fireEnabled;
        private bool iceEnabled;
        private float nextAllowedShot;

        public WeaponConfig CurrentConfig => currentConfig;
        public string ActiveEffectsDescription
        {
            get
            {
                if (!fireEnabled && !iceEnabled)
                {
                    return "sin efectos";
                }

                string value = string.Empty;
                if (fireEnabled) value += "FUEGO ";
                if (iceEnabled) value += "HIELO";
                return value.Trim();
            }
        }

        public void Configure(PlayerController controller)
        {
            player = controller;
            currentConfig = WeaponDirector.BuildBasic();
            RebuildWeapon();
            nextAllowedShot = 0f;
        }

        public void ResetWeapon()
        {
            fireEnabled = false;
            iceEnabled = false;
            currentConfig = WeaponDirector.BuildBasic();
            RebuildWeapon();
            nextAllowedShot = 0f;
        }

        public bool TryFire(Vector2 direction)
        {
            if (weapon == null || direction.sqrMagnitude <= 0.001f || Time.time < nextAllowedShot)
            {
                return false;
            }

            direction.Normalize();
            FireContext context = new FireContext
            {
                Origin = transform.position,
                Direction = direction,
                Config = currentConfig,
                SpawnProjectile = SpawnProjectile
            };
            weapon.Fire(context);
            nextAllowedShot = Time.time + 1f / currentConfig.FireRate;
            return true;
        }

        public void ApplyUpgrade(UpgradeOption option)
        {
            switch (option.Type)
            {
                case UpgradeType.Damage:
                    currentConfig = new WeaponBuilder(currentConfig)
                        .WithDamage(currentConfig.Damage + 5)
                        .Build();
                    break;
                case UpgradeType.FireRate:
                    currentConfig = new WeaponBuilder(currentConfig)
                        .WithFireRate(currentConfig.FireRate + 1.25f)
                        .Build();
                    break;
                case UpgradeType.Burn:
                    fireEnabled = true;
                    break;
                case UpgradeType.Freeze:
                    iceEnabled = true;
                    break;
                case UpgradeType.DoubleShot:
                    currentConfig = new WeaponBuilder(currentConfig)
                        .WithProjectileCount(currentConfig.ProjectileCount + 1)
                        .Build();
                    break;
                case UpgradeType.RapidProfile:
                    currentConfig = WeaponDirector.BuildRapid();
                    break;
            }

            RebuildWeapon();
            PatternEventLogger.Record("Decorator", $"mejora aplicada: {option.Title}; efectos {ActiveEffectsDescription}");
        }

        public void DemonstrateBuilderProfiles()
        {
            WeaponDirector.BuildBasic();
            WeaponDirector.BuildRapid();
            WeaponDirector.BuildHeavy();
            PatternEventLogger.Record("Builder", "Demo construyó perfiles Basic, Rapid y Heavy con la misma API");
        }

        private void RebuildWeapon()
        {
            weapon = new BaseWeapon(currentConfig);
            if (fireEnabled)
            {
                weapon = new FireWeaponDecorator(weapon);
            }

            if (iceEnabled)
            {
                weapon = new IceWeaponDecorator(weapon);
            }
        }

        private void SpawnProjectile(Vector2 origin, Vector2 direction, int damage, float speed, float lifetime, ProjectileEffects effects)
        {
            GameObject projectileObject = new GameObject("Projectile");
            projectileObject.transform.position = origin + direction * 0.45f;
            SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteLibrary.GetSquare("Projectile", effects == ProjectileEffects.None ? Color.white : new Color(1f, 0.85f, 0.25f));
            renderer.sortingOrder = 10;
            Projectile projectile = projectileObject.AddComponent<Projectile>();
            projectile.Configure(direction, damage, speed, lifetime, effects);
        }
    }
}
