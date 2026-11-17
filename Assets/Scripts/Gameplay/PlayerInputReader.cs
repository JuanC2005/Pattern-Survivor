using PatternSurvivor.Core;
using PatternSurvivor.Patterns.Command;
using PatternSurvivor.Patterns.Facade;
using PatternSurvivor.Patterns.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PatternSurvivor.Gameplay
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private PlayerController player;
        private GameFacade facade;
        private Camera mainCamera;
        private MoveCommand moveCommand;
        private ShootCommand shootCommand;
        private UseSkillCommand skillCommand;
        private PauseCommand pauseCommand;
        private Vector2 lastAim = Vector2.right;
        private float nextMoveEvidenceTime;

        public void Configure(PlayerController controller, GameFacade gameFacade)
        {
            player = controller;
            facade = gameFacade;
            mainCamera = Camera.main;
            moveCommand = new MoveCommand(player);
            shootCommand = new ShootCommand(player);
            skillCommand = new UseSkillCommand(player);
            pauseCommand = new PauseCommand(facade);
        }

        private void Update()
        {
            if (Keyboard.current == null || GameManager.Instance == null)
            {
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                pauseCommand.Execute();
                return;
            }

            if (GameManager.Instance.Phase != GamePhase.Playing)
            {
                return;
            }

            Vector2 movement = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) movement.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) movement.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) movement.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) movement.x += 1f;
            if (movement.sqrMagnitude > 0.001f)
            {
                moveCommand.SetDirection(movement.normalized);
                moveCommand.Execute();
                if (Time.unscaledTime >= nextMoveEvidenceTime)
                {
                    nextMoveEvidenceTime = Time.unscaledTime + 0.35f;
                    PatternEventLogger.Record("Command", "MoveCommand ejecutado");
                }
            }

            Vector2 aim = GetAimDirection();
            player.Weapon.SetAimDirection(aim);
            bool shooting = Mouse.current != null && Mouse.current.leftButton.isPressed;
            shooting |= Keyboard.current.spaceKey.isPressed;
            if (shooting)
            {
                shootCommand.SetDirection(aim);
                shootCommand.Execute();
            }

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                skillCommand.Execute();
            }
        }

        private Vector2 GetAimDirection()
        {
            if (Mouse.current == null || mainCamera == null)
            {
                return lastAim;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -mainCamera.transform.position.z));
            Vector2 direction = (Vector2)worldPosition - (Vector2)player.transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                lastAim = direction.normalized;
            }

            return lastAim;
        }
    }
}
