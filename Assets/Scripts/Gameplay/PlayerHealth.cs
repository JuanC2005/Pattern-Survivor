using PatternSurvivor.Core;
using PatternSurvivor.Patterns.State;
using UnityEngine;

namespace PatternSurvivor.Gameplay
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maximumHealth = 100;

        private PlayerStateMachine stateMachine;

        public int CurrentHealth { get; private set; }
        public int MaximumHealth => maximumHealth;

        public void Configure(PlayerStateMachine machine)
        {
            stateMachine = machine;
            ResetHealth();
        }

        public void ResetHealth()
        {
            CurrentHealth = maximumHealth;
            GameEvents.RaiseHealthChanged(CurrentHealth, MaximumHealth);
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || stateMachine == null)
            {
                return;
            }

            stateMachine.TryReceiveDamage(damage);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || CurrentHealth <= 0)
            {
                return;
            }

            int previous = CurrentHealth;
            CurrentHealth = Mathf.Min(MaximumHealth, CurrentHealth + amount);
            if (CurrentHealth != previous)
            {
                GameEvents.RaiseHealthChanged(CurrentHealth, MaximumHealth);
                PatternEventLogger.Record("Observer", $"HealthChanged disparado, HUD actualizado a {CurrentHealth}/{MaximumHealth}");
            }
        }

        internal void ApplyDamage(int damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            GameEvents.RaiseHealthChanged(CurrentHealth, MaximumHealth);
            PatternEventLogger.Record("Observer", $"HealthChanged disparado, HUD actualizado a {CurrentHealth}/{MaximumHealth}");
        }
    }
}
