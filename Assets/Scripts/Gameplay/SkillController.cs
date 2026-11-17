using System.Collections;
using PatternSurvivor.Core;
using PatternSurvivor.Infrastructure;
using PatternSurvivor.Patterns.Composite;
using UnityEngine;

namespace PatternSurvivor.Gameplay
{
    public sealed class SkillController : MonoBehaviour
    {
        [SerializeField] private float cooldown = 8f;
        private PlayerController player;
        private CompositeSkill compositeSkill;
        private float remainingCooldown;
        private Coroutine pulseRoutine;
        private GameObject activePulse;

        public float RemainingCooldown => remainingCooldown;

        public void Configure(PlayerController controller)
        {
            player = controller;
            compositeSkill = new CompositeSkill();
            compositeSkill.Add(new HealSkill());
            compositeSkill.Add(new AreaBlastSkill());
        }

        private void Update()
        {
            if (remainingCooldown > 0f)
            {
                remainingCooldown -= Time.deltaTime;
            }
        }

        private void OnDisable() => ResetSkill();

        public bool TryUse()
        {
            if (remainingCooldown > 0f || compositeSkill == null)
            {
                return false;
            }

            compositeSkill.Execute(new SkillContext
            {
                Player = player,
                Origin = player.transform.position,
                Radius = 2.75f,
                HealAmount = 12,
                DamageAmount = 30
            });
            remainingCooldown = cooldown;
            PlayPulseVisual();
            return true;
        }

        public void ResetSkill()
        {
            remainingCooldown = 0f;
            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }

            if (activePulse != null)
            {
                Destroy(activePulse);
                activePulse = null;
            }
        }

        private void PlayPulseVisual()
        {
            if (activePulse != null)
            {
                Destroy(activePulse);
            }

            activePulse = new GameObject("SkillPulse");
            activePulse.transform.SetParent(player.transform.parent);
            activePulse.transform.position = player.transform.position;
            SpriteRenderer renderer = activePulse.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteLibrary.GetCircle("SkillPulse", new Color(0.2f, 1f, 0.85f), 64);
            RuntimeSpriteLibrary.ApplySpriteMaterial(renderer);
            renderer.sortingOrder = 6;
            pulseRoutine = StartCoroutine(AnimatePulse(renderer));
        }

        private IEnumerator AnimatePulse(SpriteRenderer renderer)
        {
            const float duration = 0.3f;
            const float finalRadius = 2.75f;
            float elapsed = 0f;
            Vector3 origin = activePulse.transform.position;
            while (elapsed < duration && activePulse != null)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                activePulse.transform.position = origin;
                float diameter = Mathf.Lerp(0.2f, finalRadius * 2f, progress);
                activePulse.transform.localScale = Vector3.one * diameter;
                Color color = renderer.color;
                color.a = Mathf.Lerp(0.55f, 0f, progress);
                renderer.color = color;
                yield return null;
            }

            if (activePulse != null)
            {
                Destroy(activePulse);
                activePulse = null;
            }

            pulseRoutine = null;
        }
    }
}
