using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Musketeers.Battle
{
    public sealed class BattleHudRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text mpText;
        [SerializeField] private Image portrait;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider mpSlider;

        private BattleUnit unit;

        public void Bind(BattleUnit battleUnit)
        {
            unit = battleUnit;
            Refresh();
        }

        public void Refresh()
        {
            if (unit == null)
            {
                return;
            }

            if (nameText != null)
            {
                nameText.text = unit.DisplayName;
            }

            if (portrait != null && unit.Definition != null)
            {
                portrait.sprite = unit.Definition.portrait;
                portrait.enabled = unit.Definition.portrait != null;
            }

            if (hpSlider != null && unit.Definition != null)
            {
                hpSlider.maxValue = unit.Definition.maxHP;
                hpSlider.value = unit.CurrentHP;
            }

            if (mpSlider != null && unit.Definition != null)
            {
                mpSlider.maxValue = unit.Definition.maxMP;
                mpSlider.value = unit.CurrentMP;
            }

            if (hpText != null && unit.Definition != null)
            {
                hpText.text = $"{unit.CurrentHP}/{unit.Definition.maxHP}";
            }

            if (mpText != null && unit.Definition != null)
            {
                mpText.text = $"{unit.CurrentMP}/{unit.Definition.maxMP}";
            }
        }
    }
}
