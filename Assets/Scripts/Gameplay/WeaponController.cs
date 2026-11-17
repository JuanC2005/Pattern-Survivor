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
        private Transform weaponVisual;
        private SpriteRenderer weaponVisualRenderer;

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
            CreateWeaponVisual();
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

        public void SetAimDirection(Vector2 direction)
        {
            if (weaponVisual == null || direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            direction.Normalize();
            weaponVisual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
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

            UpdateWeaponVisual();
        }

        private void CreateWeaponVisual()
        {
            if (weaponVisual != null)
            {
                return;
            }

            GameObject weaponObject = new GameObject("WeaponVisual");
            weaponObject.transform.SetParent(transform, false);
            weaponObject.transform.localPosition = new Vector3(0.42f, 0f, -0.05f);
            weaponObject.transform.localScale = new Vector3(0.78f, 0.18f, 1f);
            weaponVisual = weaponObject.transform;
            weaponVisualRenderer = weaponObject.AddComponent<SpriteRenderer>();
            weaponVisualRenderer.sprite = RuntimeSpriteLibrary.GetSquare("Weapon", Color.white, 16);
            RuntimeSpriteLibrary.ApplySpriteMaterial(weaponVisualRenderer);
            weaponVisualRenderer.sortingOrder = 9;
            SetAimDirection(Vector2.right);
        }

        private void UpdateWeaponVisual()
        {
            if (weaponVisualRenderer == null)
            {
                return;
            }

            weaponVisualRenderer.color = fireEnabled && iceEnabled
                ? new Color(0.8f, 0.35f, 1f)
                : fireEnabled
                    ? new Color(1f, 0.3f, 0.08f)
                    : iceEnabled
                        ? new Color(0.25f, 0.85f, 1f)
                        : new Color(1f, 0.82f, 0.25f);
        }

        private void SpawnProjectile(Vector2 origin, Vector2 direction, int damage, float speed, float lifetime, ProjectileEffects effects)
        {
            GameObject projectileObject = new GameObject("Projectile");
            projectileObject.transform.position = origin + direction * 0.45f;
            SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteLibrary.GetSquare("Projectile", effects == ProjectileEffects.None ? Color.white : new Color(1f, 0.85f, 0.25f));
            RuntimeSpriteLibrary.ApplySpriteMaterial(renderer);
            renderer.sortingOrder = 10;
            Projectile projectile = projectileObject.AddComponent<Projectile>();
            projectile.Configure(direction, damage, speed, lifetime, effects);
        }
    }
}
