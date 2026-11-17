using System.Collections.Generic;
using UnityEngine;

namespace PatternSurvivor.Patterns.Flyweight
{
    public sealed class EnemySharedData
    {
        public EnemySharedData(string typeName, Color color, float baseSpeed, int baseHealth, int points)
        {
            TypeName = typeName;
            Color = color;
            BaseSpeed = baseSpeed;
            BaseHealth = baseHealth;
            Points = points;
        }

        public string TypeName { get; }
        public Color Color { get; }
        public float BaseSpeed { get; }
        public int BaseHealth { get; }
        public int Points { get; }
    }

    public static class EnemyDataCache
    {
        private static readonly Dictionary<string, EnemySharedData> Cache = new Dictionary<string, EnemySharedData>
        {
            ["Normal"] = new EnemySharedData("Normal", new Color(0.95f, 0.25f, 0.3f), 1.25f, 25, 10),
            ["Fast"] = new EnemySharedData("Fast", new Color(1f, 0.65f, 0.15f), 2.2f, 15, 15),
            ["Tank"] = new EnemySharedData("Tank", new Color(0.65f, 0.25f, 0.95f), 0.7f, 65, 25)
        };

        public static EnemySharedData Get(string typeName) => Cache[typeName];

        public static int Count => Cache.Count;

        public static void ClearForTests() => Cache.Clear();
    }
}
