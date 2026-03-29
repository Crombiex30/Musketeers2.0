using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HealthBar : MonoBehaviour
{
    
    public Slider slider;
    Unit Unit;
    
    public void SetMaxHealth (int health)
    {
        slider.maxValue = Unit.maxHP;;
        slider.value = Unit.currentHP;
    }
    public void SetHealth (int health)
    {
        slider.value = Unit.currentHP;
    }

    
}
