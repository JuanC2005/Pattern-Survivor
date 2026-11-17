using PatternSurvivor.Core;
using UnityEngine;

namespace PatternSurvivor.Gameplay
{
    public sealed class ExperienceSystem : MonoBehaviour
    {
        [SerializeField] private int experienceForFirstLevel = 5;
        private int experience;

        public int Level { get; private set; } = 1;
        public int Experience => experience;
        public int ExperienceTarget => experienceForFirstLevel + (Level - 1) * 3;

        public void ResetSession()
        {
            Level = 1;
            experience = 0;
            GameEvents.RaiseLevelChanged(Level);
        }

        public void AddExperience(int amount)
        {
            experience += Mathf.Max(0, amount);
            while (experience >= ExperienceTarget)
            {
                experience -= ExperienceTarget;
                Level++;
                GameEvents.RaiseLevelChanged(Level);
                PatternEventLogger.Record("Observer", $"LevelChanged disparado, nivel {Level}");
            }
        }
    }
}
