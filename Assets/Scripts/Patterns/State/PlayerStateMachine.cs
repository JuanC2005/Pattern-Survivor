using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using UnityEngine;

namespace PatternSurvivor.Patterns.State
{
    public sealed class PlayerStateMachine : MonoBehaviour
    {
        [SerializeField] private float invulnerabilityDuration = 0.8f;

        private PlayerController player;
        private PlayerHealth health;
        private SpriteRenderer playerRenderer;
        private IPlayerState currentState;
        private Color normalColor;
        private bool deathEventSent;

        public IPlayerState CurrentState => currentState;
        public bool CanMove => currentState != null && currentState.CanMove;
        public bool CanAct => currentState != null && currentState.CanAct;

        public void Configure(PlayerController controller, PlayerHealth playerHealth, SpriteRenderer renderer)
        {
            player = controller;
            health = playerHealth;
            playerRenderer = renderer;
            normalColor = renderer.color;
            ResetState();
        }

        private void Update()
        {
            currentState?.Tick(Time.deltaTime);
        }

        public void ResetState()
        {
            currentState?.Exit();
            deathEventSent = false;
            currentState = new NormalState(this);
            currentState.Enter();
        }

        public bool TryReceiveDamage(int damage)
        {
            if (currentState == null || currentState is InvulnerableState || currentState is DeadState)
            {
                return false;
            }

            health.ApplyDamage(damage);
            if (health.CurrentHealth <= 0)
            {
                TransitionTo(new DeadState(this));
                if (!deathEventSent)
                {
                    deathEventSent = true;
                    GameEvents.RaisePlayerDied(player);
                }
            }
            else
            {
                TransitionTo(new InvulnerableState(this, invulnerabilityDuration));
            }

            return true;
        }

        public void TransitionToNormal()
        {
            if (!(currentState is DeadState))
            {
                TransitionTo(new NormalState(this));
            }
        }

        private void TransitionTo(IPlayerState nextState)
        {
            string previousName = currentState?.Name ?? "None";
            currentState?.Exit();
            currentState = nextState;
            currentState.Enter();
            PatternEventLogger.Record("State", $"{previousName} -> {currentState.Name}");
        }

        public void SetVisualInvulnerable(bool value)
        {
            if (playerRenderer != null)
            {
                playerRenderer.color = value ? new Color(normalColor.r, normalColor.g, normalColor.b, 0.55f) : normalColor;
            }
        }

        public void SetBlinkVisible(bool visible)
        {
            if (playerRenderer != null)
            {
                Color color = playerRenderer.color;
                color.a = visible ? 0.55f : 0.1f;
                playerRenderer.color = color;
            }
        }
    }
}
