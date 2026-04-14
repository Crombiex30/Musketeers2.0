using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class Unit : MonoBehaviour
{
    
    public string unitName;
    public int unitLevel;
    public int damage;
    public int maxHP;
    public int currentHP;
    public List<string> abilites = new List<string>{""};
    
    public Slider healthBar;


    public void initialize(string name, int level, int dmg, int hp, List<string> abilitiesList)
    {
        unitName = name;
        unitLevel = level;
        damage = dmg;
        maxHP = hp;
        currentHP = hp;
        abilites = abilitiesList;
    }

    public bool IsDead(int currentHP)
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

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        
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
