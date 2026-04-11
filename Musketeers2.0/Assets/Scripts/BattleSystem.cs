using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public enum BattleState{ START, PLAYERTURN, ENEMYTURN, WON, LOSS }
public class BattleSystem : MonoBehaviour
{   
    
    public int amountOfEvents = 0;
    public int time;
    public int turns;
    public int numRolled = 0;
    public string randomEvent;
    public bool hasDiceRolled;
    public float damageBoost = 1f;
    public float healBoost = 1f;
    System.Random random = new System.Random();
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public TMP_Text rollDisplay;
    public TMP_Text hudText; 
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
        rollDisplay.text = "Roll: " + 0;

        
        hudText.text = playerUnit.unitName + "'s turn";
        
        playerHud.SetHUD(playerUnit);
        enemyHud.SetHUD(enemyUnit);
        
        eventText.text = "Random Event is occuring...";

        yield return new WaitForSeconds(time);

        SetRandomEvent();
        

        yield return new WaitForSeconds(time);

        state = BattleState.PLAYERTURN;
        PlayerTurn();

    }
    
    void SetRandomEvent()
    {
        
        List<string> events = new List<string>{"Wet Floor", "Cracked Floor"};
        turns = random.Next(1,11);

        randomEvent = events[random.Next(0, events.Count)];
        
        eventText.text = randomEvent;
        turnText.text = randomEvent + " duration: " + turns + " turns left";

    }

    void UpdateEvent()
    {
        turnText.text = randomEvent + " duration: " + turns + " turns left";
    }

    bool ActivateEvent()
    {
        
        switch (randomEvent)
        {
            case "Wet Floor":
                return Slipped();
            case "Cracked Floor":
                return TickDamage();
            default:
                return false;
        }
        
    }


    IEnumerator PlayerAttack()
    {
        if (ActivateEvent() )
        {
            switch (randomEvent)
            {
                case "Wet Floor":
                    hudText.text = "You slipped.";

                    yield return new WaitForSeconds(time);

                    state = BattleState.ENEMYTURN;
                    StartCoroutine(EnemyTurn());
                    yield break;
                case "Cracked Floor":
                    hudText.text = "You fell in a hole and took some damage.";

                    yield return new WaitForSeconds(time);

                    playerUnit.TakeDamage(2);
                    playerHud.SetHP(playerUnit.currentHP);
                    break;
                default:
                    break;
            }
        }
        state = BattleState.ENEMYTURN;
        enemyUnit.TakeDamage(playerUnit.damage * damageBoost);
        bool isDead = enemyUnit.IsDead(enemyUnit.currentHP);

        enemyHud.SetHP(enemyUnit.currentHP);

        yield return new WaitForSeconds(time);
        if (isDead)
        {
            state = BattleState.WON;
            EndBattle();

            yield return new WaitForSeconds(time);

            SceneController.EnterZone("TestScene");
        }
        else
        {
            StartCoroutine(EnemyTurn());

        }
    }

    IEnumerator PlayerHeal()
    {   
        if (ActivateEvent() )
        {
            switch (randomEvent)
            {
                case "Wet Floor":
                    hudText.text = "You slipped.";

                    yield return new WaitForSeconds(time);

                    state = BattleState.ENEMYTURN;
                    StartCoroutine(EnemyTurn());
                    yield break;
                case "Cracked Floor":
                    hudText.text = "You fell in a hole and took some damage.";
                    
                    yield return new WaitForSeconds(time);

                    playerUnit.TakeDamage(2);
                    playerHud.SetHP(playerUnit.currentHP);
                    break;
                default:
                    break;
            }
        }
        
        state = BattleState.ENEMYTURN;
        playerUnit.Heal(5 * healBoost);

        playerHud.SetHP(playerUnit.currentHP);
        hudText.text = "You healed";

        yield return new WaitForSeconds(time);

        
        StartCoroutine(EnemyTurn());

    }

    void PlayerRoll()
    {
        numRolled = random.Next(1, 7);
        if(numRolled == 1)
        {
            damageBoost = .50f;
            healBoost = .25f;
        }
        else if(numRolled == 2)
        {
            damageBoost = .75f;
            healBoost = .50f;
        }
        else if (numRolled == 3)
        {
            damageBoost = 1.50f;
            healBoost = .75f;
        }
        else if (numRolled == 4)
        {
            damageBoost = 1.75f;
            healBoost = 1.25f;
        }
        else if(numRolled == 5)
        {
            damageBoost = 2.0f;
            healBoost = 1.25f;
        }
        else
        {
            damageBoost = 2.5f;
            healBoost = 2.0f;
        }
        hasDiceRolled = true;
        rollDisplay.text = "Roll: " + numRolled;
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
        numRolled = 0;
        damageBoost = 1;
        healBoost = 1;
        rollDisplay.text = "Roll: " + numRolled;
        hasDiceRolled = false;
        if (ActivateEvent())
        {
            switch (randomEvent)
            {
                case "Wet Floor":
                    hudText.text = "Enemy slipped.";
            
                    yield return new WaitForSeconds(time);
                    if (turns > 0)
                    {
                        turns --;
                        UpdateEvent();
                        
                        if (turns <= 0)
                        {
                            turnText.text = "The ground has dried up.";
                            eventText.text = "Random Event is occuring...";
                            amountOfEvents ++;
                            yield return new WaitForSeconds(time);
                            if (amountOfEvents != 3)
                            {
                                SetRandomEvent();
                            }
                            else
                            {
                                eventText.text = "Events Finished.";
                                turnText.text = "";
                                randomEvent = "None";
                            }
                                
                        }
                    }
                    state = BattleState.PLAYERTURN;
                    PlayerTurn();
                    yield break;
                case "Cracked Floor":
                    hudText.text = "Enemy fell into a hole and took damage.";

                    yield return new WaitForSeconds(time);
                    
                    enemyUnit.TakeDamage(2);
                    enemyHud.SetHP(enemyUnit.currentHP);
                    if (enemyUnit.IsDead(enemyUnit.currentHP))
                    {
                        state = BattleState.WON;
                        EndBattle();

                        yield return new WaitForSeconds(time);

                        SceneController.EnterZone("TestScene");

                        yield return new WaitForSeconds(time);
                    }
                    break;
                default:
                    break;
            }
            
        }
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

            SceneController.EnterZone("TestScene");

        }
        else
        {
            if (turns > 0)
            {
                turns --;
                UpdateEvent();
                
                if (turns <= 0)
                {
                    switch (randomEvent)
                    {
                        case "Wet Floor":
                            turnText.text = "The ground has dried up.";
                            break;
                        case "Cracked Floor":
                            turnText.text = "The ground has been fixed.";
                            break;
                        default:
                            break;
                    }
                    eventText.text = "Random Event is occuring...";
                    amountOfEvents ++;
                    yield return new WaitForSeconds(time);
                    if (amountOfEvents != 3)
                    {
                        SetRandomEvent();
                    }
                    else
                    {
                        eventText.text = "Events Finished.";
                        turnText.text = "";
                        randomEvent = "None";
                    }
                        
                }
            }
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    void PlayerTurn()
    {
        
        hudText.text = "Choose an action:";
        

        
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
    public void OnDiceButton()
    {
        if (state != BattleState.PLAYERTURN || hasDiceRolled == true)
        {
            return;
        }
        PlayerRoll();
    }

    bool Slipped()
    {
        
        int chance = random.Next(1,5);
       
        if (chance == 1)  
        {
            if (state == BattleState.PLAYERTURN)
            {
                return true;
            }
            else if(state == BattleState.ENEMYTURN)
            {   
                
                return true;
            }
            
        }
        return false;
        
    }
    bool TickDamage()
    {   
        
        if (state == BattleState.PLAYERTURN)
        {
            return true;
        }
        else if(state == BattleState.ENEMYTURN)
        {           
            return true;
        }
        return false;
    }

}
