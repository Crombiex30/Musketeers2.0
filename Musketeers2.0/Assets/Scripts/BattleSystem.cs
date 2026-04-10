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
    public string randomEvent;
    public List<string> events = new List<string>{"Wet Floor", "Cracked Floor"};
    System.Random random = new System.Random();
    public GameObject tankPrefab;
    public GameObject enemyPrefab;
    public TMP_Text hudText; 
    public TMP_Text eventText;
    public TMP_Text turnText;

    Unit tankUnit;
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
        GameObject tankGO = tankPrefab;
        tankUnit = tankGO.GetComponent<Unit>();

        GameObject enemyGO = enemyPrefab;
        enemyUnit = enemyGO.GetComponent<Unit>();


        
        hudText.text = tankUnit.unitName + "'s turn";
        
        playerHud.SetHUD(tankUnit);
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

                    tankUnit.TakeDamage(2);
                    playerHud.SetHP(tankUnit.currentHP);
                    break;
                default:
                    break;
            }
        }

        state = BattleState.ENEMYTURN;
        enemyUnit.TakeDamage(tankUnit.damage);
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

                    tankUnit.TakeDamage(2);
                    playerHud.SetHP(tankUnit.currentHP);
                    break;
                default:
                    break;
            }
        }
        state = BattleState.ENEMYTURN;
        tankUnit.Heal(5);

        playerHud.SetHP(tankUnit.currentHP);
        hudText.text = "You healed";

        yield return new WaitForSeconds(time);

        
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

        tankUnit.TakeDamage(enemyUnit.damage);
        bool isDead = tankUnit.IsDead(tankUnit.currentHP);
        
        
        playerHud.SetHP(tankUnit.currentHP);

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
