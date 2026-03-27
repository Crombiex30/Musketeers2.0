using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHud : MonoBehaviour
{
   public TMP_Text nameText;
   //public Text levelText;
   public Slider hpSlider;

   public void SetHUD(Unit unit)
    {
        nameText.text = unit.unitName;
        //levelText.text = unit.unitLevel;
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;

    }

    public void SetHP(int hp)
    {
        hpSlider.value = hp;
    }

}
