using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class Unit : MonoBehaviour
{
    
    public string unitName;
    public int unitLevel;
    public float damage;
    public int maxHP;
    public float currentHP;
    public int dangerLevel;
    public List<string> abilites = new List<string>{""};
    
    public Slider healthBar;



     public bool IsDead(float currentHP)
    {
        if (currentHP <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;
        
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }
    
}
