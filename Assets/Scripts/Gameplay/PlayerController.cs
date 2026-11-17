using PatternSurvivor.Core;
using PatternSurvivor.Patterns.Composite;
using PatternSurvivor.Patterns.Singleton;
using PatternSurvivor.Patterns.State;
using UnityEngine;

namespace PatternSurvivor.Gameplay
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        public PlayerHealth Health { get; private set; }
        public PlayerStateMachine StateMachine { get; private set; }
        public WeaponController Weapon { get; private set; }
        public SkillController Skill { get; private set; }

        public void Configure(PlayerHealth health, PlayerStateMachine stateMachine, WeaponController weapon, SkillController skill)
        {
            Health = health;
            StateMachine = stateMachine;
            Weapon = weapon;
            Skill = skill;
        }

        public void Move(Vector2 direction)
        {
            if (!CanControl() || direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Vector2 clampedDirection = Vector2.ClampMagnitude(direction, 1f);
            Vector3 position = transform.position + (Vector3)(clampedDirection * moveSpeed * Time.deltaTime);
            Rect bounds = GameManager.Instance.ArenaBounds;
            position.x = Mathf.Clamp(position.x, bounds.xMin + 0.45f, bounds.xMax - 0.45f);
            position.y = Mathf.Clamp(position.y, bounds.yMin + 0.45f, bounds.yMax - 0.45f);
            transform.position = position;
        }

        public bool Shoot(Vector2 direction)
        {
            return CanControl() && Weapon != null && Weapon.TryFire(direction);
        }

        public bool UseSkill()
        {
            return CanControl() && Skill != null && Skill.TryUse();
        }

        public void ResetSession()
        {
            transform.position = Vector3.zero;
            StateMachine.ResetState();
            Health.ResetHealth();
            Weapon.ResetWeapon();
            Skill.ResetSkill();
        }

        private bool CanControl()
        {
            return GameManager.Instance != null &&
                   GameManager.Instance.Phase == GamePhase.Playing &&
                   StateMachine != null && StateMachine.CanAct;
        }
    }
}
