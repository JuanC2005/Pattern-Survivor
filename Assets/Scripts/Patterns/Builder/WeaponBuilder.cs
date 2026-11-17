using PatternSurvivor.Core;

namespace PatternSurvivor.Patterns.Builder
{
    public sealed class WeaponConfig
    {
        public WeaponConfig(int damage, float fireRate, float projectileSpeed, int projectileCount, float projectileLifetime)
        {
            Damage = damage;
            FireRate = fireRate;
            ProjectileSpeed = projectileSpeed;
            ProjectileCount = projectileCount;
            ProjectileLifetime = projectileLifetime;
        }

        public int Damage { get; }
        public float FireRate { get; }
        public float ProjectileSpeed { get; }
        public int ProjectileCount { get; }
        public float ProjectileLifetime { get; }
    }

    public sealed class WeaponBuilder
    {
        private int damage = 10;
        private float fireRate = 3f;
        private float projectileSpeed = 11f;
        private int projectileCount = 1;
        private float projectileLifetime = 2.5f;

        public WeaponBuilder() { }

        public WeaponBuilder(WeaponConfig source)
        {
            damage = source.Damage;
            fireRate = source.FireRate;
            projectileSpeed = source.ProjectileSpeed;
            projectileCount = source.ProjectileCount;
            projectileLifetime = source.ProjectileLifetime;
        }

        public WeaponBuilder WithDamage(int value)
        {
            damage = value;
            return this;
        }

        public WeaponBuilder WithFireRate(float value)
        {
            fireRate = value;
            return this;
        }

        public WeaponBuilder WithProjectileSpeed(float value)
        {
            projectileSpeed = value;
            return this;
        }

        public WeaponBuilder WithProjectileCount(int value)
        {
            projectileCount = value;
            return this;
        }

        public WeaponBuilder WithProjectileLifetime(float value)
        {
            projectileLifetime = value;
            return this;
        }

        public WeaponConfig Build()
        {
            WeaponConfig config = new WeaponConfig(
                damage < 1 ? 1 : damage,
                fireRate < 0.25f ? 0.25f : fireRate,
                projectileSpeed < 1f ? 1f : projectileSpeed,
                projectileCount < 1 ? 1 : projectileCount,
                projectileLifetime < 0.2f ? 0.2f : projectileLifetime);

            PatternEventLogger.Record(
                "Builder",
                $"arma configurada, daño {config.Damage}, cadencia {config.FireRate:0.0}/s, proyectiles {config.ProjectileCount}");
            return config;
        }
    }

    public static class WeaponDirector
    {
        public static WeaponConfig BuildBasic()
        {
            return new WeaponBuilder()
                .WithDamage(10)
                .WithFireRate(3f)
                .WithProjectileSpeed(11f)
                .WithProjectileCount(1)
                .WithProjectileLifetime(2.5f)
                .Build();
        }

        public static WeaponConfig BuildRapid()
        {
            return new WeaponBuilder()
                .WithDamage(7)
                .WithFireRate(6f)
                .WithProjectileSpeed(13f)
                .WithProjectileCount(1)
                .WithProjectileLifetime(2f)
                .Build();
        }

        public static WeaponConfig BuildHeavy()
        {
            return new WeaponBuilder()
                .WithDamage(24)
                .WithFireRate(1.5f)
                .WithProjectileSpeed(9f)
                .WithProjectileCount(1)
                .WithProjectileLifetime(3f)
                .Build();
        }
    }
}
