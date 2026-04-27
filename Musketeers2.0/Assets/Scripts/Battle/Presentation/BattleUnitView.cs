using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Musketeers.Battle
{
    public sealed class BattleUnitView : MonoBehaviour
    {
        [SerializeField] private Transform effectOrigin;
        [SerializeField] private Animator animator;
        [SerializeField] private Renderer highlightedRenderer;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private GameObject defeatedRoot;
        [SerializeField] private float actLungeDistance = 0.25f;
        [SerializeField] private float actLungeTime = 0.12f;

        private BattleUnit unit;
        private Vector3 startPosition;

        public Transform EffectOrigin => effectOrigin != null ? effectOrigin : transform;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            startPosition = transform.localPosition;
        }

        public void Bind(BattleUnit battleUnit)
        {
            unit = battleUnit;
            startPosition = transform.localPosition;
            Refresh();
        }

        public void Refresh()
        {
            if (unit == null)
            {
                return;
            }

            if (nameLabel != null)
            {
                nameLabel.text = unit.DisplayName;
            }

            if (hpSlider != null && unit.Definition != null)
            {
                hpSlider.maxValue = unit.Definition.maxHP;
                hpSlider.value = unit.CurrentHP;
            }

            if (defeatedRoot != null)
            {
                defeatedRoot.SetActive(!unit.IsAlive);
            }
        }

        public IEnumerator PlayAct(BattleSkillDefinition skill)
        {
            if (animator != null)
            {
                animator.SetTrigger(skill != null ? skill.kind.ToString() : "Act");
            }

            Vector3 direction = unit != null && unit.Faction == BattleFaction.Party ? Vector3.right : Vector3.left;
            Vector3 target = startPosition + direction * actLungeDistance;
            yield return MoveLocal(target, actLungeTime);
            yield return MoveLocal(startPosition, actLungeTime);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (highlightedRenderer != null)
            {
                highlightedRenderer.enabled = highlighted;
            }
        }

        private IEnumerator MoveLocal(Vector3 target, float duration)
        {
            Vector3 from = transform.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(from, target, duration > 0f ? elapsed / duration : 1f);
                yield return null;
            }

            transform.localPosition = target;
        }
    }
}