using System.Collections.Generic;

namespace PatternSurvivor.Patterns.Prototype
{
    public sealed class EnemyPrototype
    {
        public string TypeName;
        public int CurrentHealth;
        public float MoveSpeed;
        public int ContactDamage;
        public int PointValue;
        public int ExperienceValue;

        public EnemyPrototype Clone()
        {
            return new EnemyPrototype
            {
                TypeName = TypeName,
                CurrentHealth = CurrentHealth,
                MoveSpeed = MoveSpeed,
                ContactDamage = ContactDamage,
                PointValue = PointValue,
                ExperienceValue = ExperienceValue
            };
        }
    }

    public static class EnemyPrototypeCatalog
    {
        private static readonly Dictionary<string, EnemyPrototype> Prototypes = new Dictionary<string, EnemyPrototype>
        {
            ["Normal"] = new EnemyPrototype
            {
                TypeName = "Normal", CurrentHealth = 25, MoveSpeed = 1.25f,
                ContactDamage = 10, PointValue = 10, ExperienceValue = 1
            },
            ["Fast"] = new EnemyPrototype
            {
                TypeName = "Fast", CurrentHealth = 15, MoveSpeed = 2.2f,
                ContactDamage = 8, PointValue = 15, ExperienceValue = 2
            },
            ["Tank"] = new EnemyPrototype
            {
                TypeName = "Tank", CurrentHealth = 65, MoveSpeed = 0.7f,
                ContactDamage = 18, PointValue = 25, ExperienceValue = 3
            }
        };

        public static EnemyPrototype Get(string typeName)
        {
            return Prototypes[typeName];
        }
    }
}
