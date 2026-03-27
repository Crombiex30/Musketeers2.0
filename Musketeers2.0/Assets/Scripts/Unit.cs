using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Unit : MonoBehaviour
{
    
    public string unitName;
    public int unitLevel;
    public int damage;
    public int maxHP;
    public int currentHP;
    
    public Slider healthBar;


    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;
        
        if (currentHP <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }
    
}
