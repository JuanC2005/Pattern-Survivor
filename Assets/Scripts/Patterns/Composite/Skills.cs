using System.Collections.Generic;
using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using UnityEngine;

namespace PatternSurvivor.Patterns.Composite
{
    public sealed class SkillContext
    {
        public PlayerController Player;
        public Vector2 Origin;
        public float Radius;
        public int HealAmount;
        public int DamageAmount;
    }

    public interface ISkill
    {
        string Name { get; }
        void Execute(SkillContext context);
    }

    public sealed class HealSkill : ISkill
    {
        public string Name => nameof(HealSkill);

        public void Execute(SkillContext context)
        {
            context.Player.Health.Heal(context.HealAmount);
            PatternEventLogger.Record("Composite", "HealSkill ejecutó curación real");
        }
    }

    public sealed class AreaBlastSkill : ISkill
    {
        public string Name => nameof(AreaBlastSkill);

        public void Execute(SkillContext context)
        {
            int affected = 0;
            Enemy[] enemies = Object.FindObjectsByType<Enemy>();
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && Vector2.Distance(context.Origin, enemy.transform.position) <= context.Radius)
                {
                    enemy.TakeDamage(context.DamageAmount, "AreaBlastSkill");
                    affected++;
                }
            }

            PatternEventLogger.Record("Composite", $"AreaBlastSkill dañó a {affected} enemigos cercanos");
        }
    }

    public sealed class CompositeSkill : ISkill
    {
        private readonly List<ISkill> children = new List<ISkill>();

        public string Name => nameof(CompositeSkill);
        public IReadOnlyList<ISkill> Children => children;

        public void Add(ISkill skill)
        {
            if (skill != null && !children.Contains(skill))
            {
                children.Add(skill);
            }
        }

        public void Remove(ISkill skill) => children.Remove(skill);

        public void Execute(SkillContext context)
        {
            foreach (ISkill child in children)
            {
                child.Execute(context);
            }

            PatternEventLogger.Record("Composite", $"CompositeSkill ejecutó {children.Count} habilidades hijas");
        }
    }
}
