using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using UnityEngine;

namespace PatternSurvivor.Patterns.State
{
    public interface IPlayerState
    {
        string Name { get; }
        bool CanMove { get; }
        bool CanAct { get; }
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }

    public sealed class NormalState : IPlayerState
    {
        private readonly PlayerStateMachine machine;
        public NormalState(PlayerStateMachine machine) => this.machine = machine;
        public string Name => nameof(NormalState);
        public bool CanMove => true;
        public bool CanAct => true;
        public void Enter() => machine.SetVisualInvulnerable(false);
        public void Tick(float deltaTime) { }
        public void Exit() { }
    }

    public sealed class InvulnerableState : IPlayerState
    {
        private readonly PlayerStateMachine machine;
        private readonly float duration;
        private float remaining;

        public InvulnerableState(PlayerStateMachine machine, float duration)
        {
            this.machine = machine;
            this.duration = duration;
        }

        public string Name => nameof(InvulnerableState);
        public bool CanMove => true;
        public bool CanAct => true;

        public void Enter()
        {
            remaining = duration;
            machine.SetVisualInvulnerable(true);
        }

        public void Tick(float deltaTime)
        {
            remaining -= deltaTime;
            machine.SetBlinkVisible(Mathf.FloorToInt(remaining * 14f) % 2 == 0);
            if (remaining <= 0f)
            {
                machine.TransitionToNormal();
            }
        }

        public void Exit() => machine.SetVisualInvulnerable(false);
    }

    public sealed class DeadState : IPlayerState
    {
        private readonly PlayerStateMachine machine;
        public DeadState(PlayerStateMachine machine) => this.machine = machine;
        public string Name => nameof(DeadState);
        public bool CanMove => false;
        public bool CanAct => false;
        public void Enter()
        {
            machine.SetVisualInvulnerable(false);
            PatternEventLogger.Record("State", "DeadState bloqueó movimiento y disparo");
        }
        public void Tick(float deltaTime) { }
        public void Exit() { }
    }
}
