using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState{ START, PLAYERTURN, ENEMYTURN, WON, LOSS }
public class BattleSystem : MonoBehaviour
{
    public int time;
    public int turns;
    public string randomEvent;
    System.Random random = new System.Random();
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Text hudText; 
    public TMP_Text eventText;
    public TMP_Text turnText;

    Unit playerUnit;
    Unit enemyUnit;

    public BattleHud playerHud;
    public BattleHud enemyHud;


    public BattleState state;

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetUpBattle());
    }

    IEnumerator SetUpBattle()
    {
        GameObject playerGO = Instantiate(playerPrefab);
        playerUnit = playerGO.GetComponent<Unit>();

        GameObject enemyGO = Instantiate(enemyPrefab);
        enemyUnit = enemyGO.GetComponent<Unit>();


        
        hudText.text = playerUnit.unitName + "'s turn";
        
        playerHud.SetHUD(playerUnit);
        enemyHud.SetHUD(enemyUnit);
        
        eventText.text = "Random Event is occuring...";

        yield return new WaitForSeconds(time);

        SetRandomEvent();
        eventText.text = randomEvent;
        turnText.text = "Turns: " + Convert.ToString(turns);

        yield return new WaitForSeconds(time);

        state = BattleState.PLAYERTURN;
        PlayerTurn();

    }
    
    void SetRandomEvent()
    {
        
        List<string> events = new List<string>{"WetFloor", "CrackedFloor"};
        turns = random.Next(1,11);

        randomEvent = events[random.Next(0, events.Count)];

    }


    IEnumerator PlayerAttack()
    {
        enemyUnit.TakeDamage(playerUnit.damage);
        bool isDead = enemyUnit.IsDead(enemyUnit.currentHP);

        enemyHud.SetHP(enemyUnit.currentHP);

        yield return new WaitForSeconds(time);

        if (isDead)
        {
            state = BattleState.WON;
            EndBattle();

            yield return new WaitForSeconds(time);

            SceneController.EnterZone("Zone 1");
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());

        }
    }

    IEnumerator PlayerHeal()
    {
        playerUnit.Heal(5);

        playerHud.SetHP(playerUnit.currentHP);
        hudText.text = "You healed";

        yield return new WaitForSeconds(time);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());

    }

    void EndBattle()
    {
        if (state == BattleState.WON)
        {
            hudText.text = "Enemy has been defeated";

        }else if (state == BattleState.LOSS)
        {
            hudText.text = "Your dead";

            
        }
    }

    IEnumerator EnemyTurn()
    {
        hudText.text = enemyUnit.unitName + " attacks!";

        yield return new WaitForSeconds(time);

        playerUnit.TakeDamage(enemyUnit.damage);
        bool isDead = playerUnit.IsDead(playerUnit.currentHP);
        
        
        playerHud.SetHP(playerUnit.currentHP);

        yield return new WaitForSeconds(time);

        if (isDead)
        {
            state = BattleState.LOSS;
            EndBattle();

            yield return new WaitForSeconds(time);

            SceneController.EnterZone("Homebase");

        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    void PlayerTurn()
    {
        //bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
        hudText.text = "Choose an action:";
        //RandomEvent();

        //if ()
    }

    public void OnAttackButton()
    {
        if(state != BattleState.PLAYERTURN)
        {
            return;
        }

        StartCoroutine(PlayerAttack());
    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        StartCoroutine(PlayerHeal());
    }

    public bool slipped()
    {
        
        int chance = random.Next(1,5);
       
        if (chance == 1)
        {
           return true;
        }
        else
        {
            return false;
        }
    }

}
