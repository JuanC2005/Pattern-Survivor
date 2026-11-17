using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using PatternSurvivor.Patterns.Builder;
using UnityEngine;

namespace PatternSurvivor.Patterns.Decorator
{
    [System.Flags]
    public enum ProjectileEffects
    {
        None = 0,
        Burn = 1,
        Freeze = 2
    }

    public sealed class FireContext
    {
        public Vector2 Origin;
        public Vector2 Direction;
        public WeaponConfig Config;
        public ProjectileEffects Effects;
        public int BonusDamage;
        public System.Action<Vector2, Vector2, int, float, float, ProjectileEffects> SpawnProjectile;
    }

    public interface IWeapon
    {
        WeaponConfig Config { get; }
        void Fire(FireContext context);
    }

    public sealed class BaseWeapon : IWeapon
    {
        public BaseWeapon(WeaponConfig config) => Config = config;
        public WeaponConfig Config { get; }

        public void Fire(FireContext context)
        {
            int count = Config.ProjectileCount;
            for (int i = 0; i < count; i++)
            {
                float spread = count == 1 ? 0f : Mathf.Lerp(-0.16f, 0.16f, i / (float)(count - 1));
                Vector2 direction = Quaternion.Euler(0f, 0f, spread * Mathf.Rad2Deg) * context.Direction;
                context.SpawnProjectile(
                    context.Origin,
                    direction,
                    Config.Damage + context.BonusDamage,
                    Config.ProjectileSpeed,
                    Config.ProjectileLifetime,
                    context.Effects);
            }
        }
    }

    public abstract class WeaponDecorator : IWeapon
    {
        protected WeaponDecorator(IWeapon inner) => Inner = inner;
        protected IWeapon Inner { get; }
        public WeaponConfig Config => Inner.Config;
        public virtual void Fire(FireContext context) => Inner.Fire(context);
    }

    public sealed class FireWeaponDecorator : WeaponDecorator
    {
        public FireWeaponDecorator(IWeapon inner) : base(inner) { }

        public override void Fire(FireContext context)
        {
            context.Effects |= ProjectileEffects.Burn;
            context.BonusDamage += 2;
            PatternEventLogger.Record("Decorator", "FireWeaponDecorator añadió quemadura y daño adicional");
            base.Fire(context);
        }
    }

    public sealed class IceWeaponDecorator : WeaponDecorator
    {
        public IceWeaponDecorator(IWeapon inner) : base(inner) { }

        public override void Fire(FireContext context)
        {
            context.Effects |= ProjectileEffects.Freeze;
            PatternEventLogger.Record("Decorator", "IceWeaponDecorator añadió ralentización temporal");
            base.Fire(context);
        }
    }
}
