using System.Collections.Generic;
using PatternSurvivor.Core;

namespace PatternSurvivor.Gameplay
{
    public enum UpgradeType
    {
        Damage,
        FireRate,
        Burn,
        Freeze,
        DoubleShot,
        RapidProfile
    }

    public sealed class UpgradeOption
    {
        public UpgradeOption(UpgradeType type, string title, string description)
        {
            Type = type;
            Title = title;
            Description = description;
        }

        public UpgradeType Type { get; }
        public string Title { get; }
        public string Description { get; }
    }

    public sealed class UpgradeSystem
    {
        private readonly WeaponController weapon;
        private readonly ExperienceSystem experience;

        public UpgradeSystem(WeaponController weaponController, ExperienceSystem experienceSystem)
        {
            weapon = weaponController;
            experience = experienceSystem;
        }

        public UpgradeOption[] CreateOptions()
        {
            UpgradeOption[] all =
            {
                new UpgradeOption(UpgradeType.Damage, "+ DAÑO", "Los proyectiles infligen 5 de daño adicional."),
                new UpgradeOption(UpgradeType.FireRate, "+ CADENCIA", "Dispara 1.25 veces por segundo más."),
                new UpgradeOption(UpgradeType.Burn, "EFECTO DE FUEGO", "Los impactos aplican quemadura periódica."),
                new UpgradeOption(UpgradeType.Freeze, "EFECTO DE HIELO", "Los impactos ralentizan al enemigo."),
                new UpgradeOption(UpgradeType.DoubleShot, "PROYECTIL DOBLE", "Dispara un proyectil adicional."),
                new UpgradeOption(UpgradeType.RapidProfile, "PERFIL RAPID", "Cambia al arma construida Rapid.")
            };

            int start = (experience.Level - 1) % all.Length;
            var options = new List<UpgradeOption>(3);
            for (int i = 0; i < 3; i++)
            {
                options.Add(all[(start + i) % all.Length]);
            }

            return options.ToArray();
        }

        public void ApplyUpgrade(UpgradeOption option)
        {
            weapon.ApplyUpgrade(option);
            PatternEventLogger.Record("Observer", $"mejora {option.Title} aplicada tras evento LevelChanged");
        }
    }
}
