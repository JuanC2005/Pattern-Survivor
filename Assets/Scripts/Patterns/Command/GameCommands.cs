using PatternSurvivor.Core;
using PatternSurvivor.Gameplay;
using PatternSurvivor.Patterns.Facade;
using UnityEngine;

namespace PatternSurvivor.Patterns.Command
{
    public interface IGameCommand
    {
        string Name { get; }
        void Execute();
    }

    public sealed class MoveCommand : IGameCommand
    {
        private readonly PlayerController receiver;
        private Vector2 direction;

        public MoveCommand(PlayerController receiver) => this.receiver = receiver;
        public string Name => nameof(MoveCommand);
        public void SetDirection(Vector2 value) => direction = value;
        public void Execute() => receiver.Move(direction);
    }

    public sealed class ShootCommand : IGameCommand
    {
        private readonly PlayerController receiver;
        private Vector2 direction;

        public ShootCommand(PlayerController receiver) => this.receiver = receiver;
        public string Name => nameof(ShootCommand);
        public void SetDirection(Vector2 value) => direction = value;
        public void Execute()
        {
            if (receiver.Shoot(direction))
            {
                PatternEventLogger.Record("Command", "ShootCommand ejecutado");
            }
        }
    }

    public sealed class UseSkillCommand : IGameCommand
    {
        private readonly PlayerController receiver;

        public UseSkillCommand(PlayerController receiver) => this.receiver = receiver;
        public string Name => nameof(UseSkillCommand);
        public void Execute()
        {
            if (receiver.UseSkill())
            {
                PatternEventLogger.Record("Command", "UseSkillCommand ejecutado");
            }
        }
    }

    public sealed class PauseCommand : IGameCommand
    {
        private readonly GameFacade receiver;

        public PauseCommand(GameFacade receiver) => this.receiver = receiver;
        public string Name => nameof(PauseCommand);
        public void Execute()
        {
            if (GameManagerAccessor.Current == null)
            {
                return;
            }

            if (GameManagerAccessor.Current.Phase == GamePhase.Playing)
            {
                receiver.PauseGame();
            }
            else if (GameManagerAccessor.Current.Phase == GamePhase.Paused)
            {
                receiver.ResumeGame();
            }

            PatternEventLogger.Record("Command", "PauseCommand ejecutado");
        }
    }

    internal static class GameManagerAccessor
    {
        public static Patterns.Singleton.GameManager Current => Patterns.Singleton.GameManager.Instance;
    }
}
