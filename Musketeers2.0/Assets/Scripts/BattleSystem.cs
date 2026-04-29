using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public enum BattleState{ START, PLAYERTURN, ENEMYTURN, WON, LOSS }
public class BattleSystem : MonoBehaviour
{   
    
    /// <summary>
    /// This part here is for the Dynamic Events System
    /// </summary>
    public int time;
    public int turns;
    public int turnsPassed;
    public int amountOfEvents = 0;
    public string randomEvent;
    public List<string> events = new List<string>{"Wet Floor", "Cracked Floor"};
    //////////////////////////////////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// This part is what is scene in Unity 
    /// </summary>
    System.Random random = new System.Random();
    public GameObject tankPrefab;
    public GameObject swordPrefab;
    public GameObject healerPrefab;
    public GameObject rangerPrefab;
    public GameObject enemyPrefab;
    public TMP_Text sitText; 
    public TMP_Text eventText;
    public TMP_Text turnText;
    public TMP_Text rollDisplay;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    

    /// <summary>
    /// This part is to define units and variables
    /// </summary>
    Unit tankUnit;
    Unit swordUnit;
    Unit healerUnit;
    Unit rangerUnit;
    Unit enemyUnit;
    Unit empty;
    public BattleState state;
    public List<Unit> members = new List<Unit>{};
    public int numRolled = 0;
    public bool hasDiceRolled;
    public float damageBoost = 1f;
    public float healBoost = 1f;
    public CombatManager hud;
    List<StatusEffect> activeEffects = new List<StatusEffect>();
    public int combatTurns;
    private float prev;
    private int prevDanger;

    
    /// <summary>
    /// These are the UI variables 
    /// </summary>
    public BattleHud tankHud;
    public BattleHud swordHud;
    public BattleHud healerHud;
    public BattleHud rangerHud;
    public BattleHud enemyHud;


    

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetUpBattle());
    }
    
    void SetUpParty()
    {
        GameObject tankGO = tankPrefab;
        tankUnit = tankGO.GetComponent<Unit>();
        tankHud.SetHUD(tankUnit);
        members.Add(tankUnit);

        GameObject swordGO = swordPrefab;
        swordUnit = swordGO.GetComponent<Unit>();
        swordHud.SetHUD(swordUnit);
        members.Add(swordUnit);

        GameObject healerGO = healerPrefab;
        healerUnit = healerGO.GetComponent<Unit>();
        healerHud.SetHUD(healerUnit);
        members.Add(healerUnit);

        GameObject rangerGO = rangerPrefab;
        rangerUnit = rangerGO.GetComponent<Unit>();
        rangerHud.SetHUD(rangerUnit);
        members.Add(rangerUnit);

    }
    void SetUpEnemy()
    {
        GameObject enemyGO = enemyPrefab;
        enemyUnit = enemyGO.GetComponent<Unit>();
        enemyHud.SetHUD(enemyUnit);
        prev = enemyUnit.damage;
    }
    IEnumerator SetUpBattle()
    {
        SetUpParty();
        SetUpEnemy();
        rollDisplay.text = "Roll: " + 0;
        sitText.text = "Your turn";
        eventText.text = "Random Event is occuring...";

        yield return new WaitForSeconds(time);

        SetRandomEvent();
        

        yield return new WaitForSeconds(time);

        state = BattleState.PLAYERTURN;
        PlayerTurn();

    }
    void PlayerTurn()
    {
        
        sitText.text = "Choose an action:";
        

        
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

    bool ActiveEvent()
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
    IEnumerator ActivateEvent()
    {
        switch (randomEvent)
            {
                case "Wet Floor":
                    sitText.text = "You slipped.";

                    yield return new WaitForSeconds(time);

                    state = BattleState.ENEMYTURN;
                    StartCoroutine(EnemyTurn());
                    yield break;
                case "Cracked Floor":
                    sitText.text = "You fell in a hole and took some damage.";
                    
                    yield return new WaitForSeconds(time);

                    tankUnit.TakeDamage(2);
                    tankHud.SetHP(tankUnit.currentHP);

                    swordUnit.TakeDamage(2);
                    swordHud.SetHP(swordUnit.currentHP);

                    healerUnit.TakeDamage(2);
                    healerHud.SetHP(healerUnit.currentHP);
                    
                    rangerUnit.TakeDamage(2);
                    rangerHud.SetHP(rangerUnit.currentHP);
                    break;
                default:
                    break;
            }
    }
/// <summary>
/// This code piece checks and updates effects based on when they were activated.
/// </summary>
    void updateEffect()
    {
        combatTurns++;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            int turnsPassed = combatTurns - effect.startTurn;

            if (turnsPassed > effect.duration)
            {
                switch (effect.name)
                {
                    case "UltDef":
                        enemyUnit.damage = prev;
                        sitText.text = "Protection has ended.";
                        break;
                    case "PullAgro":
                        tankUnit.dangerLevel = prevDanger;
                        sitText.text = "Enemies have lost interest in Tank.";
                        break;
                    default:
                        break;
                }

                activeEffects.RemoveAt(i);
            }
        }
    }

    Unit selectedCharacter()
    {
        if (hud.tank == true)
        {   
            Debug.Log("Tank");
            return tankUnit;

        }else if(hud.sword == true)
        {
            Debug.Log("Sword");
            return swordUnit;
        } else if (hud.healer == true)
        {
            Debug.Log("Healer");
            return healerUnit;
        }else if(hud.ranger == true)
        {
            Debug.Log("Ranger");
            return rangerUnit;
        }

        return empty;
    }
/// <summary>
/// This creates an effect so that it can be added to a list and be kept track of.
/// </summary>
    class StatusEffect
    {
        public string name;
        public int startTurn;
        public int duration;
    }

    IEnumerator PlayerAttack()
    {
        Unit selectedChar = selectedCharacter();
        if (ActiveEvent() )
        {
            StartCoroutine(ActivateEvent());
        }

        state = BattleState.ENEMYTURN;
        
        enemyUnit.TakeDamage(selectedChar.damage* damageBoost);
        
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
/// <summary>
/// All of these are Tank Abilities
/// </summary>

    IEnumerator PullAggro()
    {
        if (ActiveEvent())
        {
            StartCoroutine(ActivateEvent());
        }
        prevDanger = tankUnit.dangerLevel;
        tankUnit.dangerLevel = 1000;

        sitText.text = "Tank has attracted the attention of everyone";

        activeEffects.Add(new StatusEffect { name = "PullAgro", startTurn = combatTurns, duration = 2});

        yield return new WaitForSeconds(time);

        StartCoroutine(EnemyTurn());
    }

    IEnumerator Bash()
    {
        if (ActiveEvent())
        {
            StartCoroutine(ActivateEvent());
        }

        sitText.text = "Tank bashed into the enemy.";

        state = BattleState.ENEMYTURN;
        enemyUnit.TakeDamage((tankUnit.damage + 3) * damageBoost);
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

    IEnumerator UltDef()
    {
        if (ActiveEvent())
        {
            StartCoroutine(ActivateEvent());
        }
        state = BattleState.ENEMYTURN;

        sitText.text = "Tank calls upon the ultimate defence";

        activeEffects.Add(new StatusEffect { name = "UltDef", startTurn = combatTurns, duration = 2});
        
        
        if (activeEffects.Exists(effect => effect.name == "UltDef"))
        {
            enemyUnit.damage = 0;
        }

        yield return new WaitForSeconds(time);

        
        StartCoroutine(EnemyTurn());
        

    }
// End of Tanks Abilites
//////////////////////////////////////////////////////////
/// Start of Sword Abilities

    IEnumerator SwordSlash()
    {
        if (ActiveEvent())
        {
            StartCoroutine(ActivateEvent());
        }
        sitText.text = "Sword slash at the enemy.";

        state = BattleState.ENEMYTURN;
        enemyUnit.TakeDamage((swordUnit.damage + 3) * damageBoost);
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



///End of Sword Abilities
///////////////////////////////////////////////////////////
/// Start of Healer Abilities
/// End of Healer Abilities
////////////////////////////////////////////////////////////
/// Start of Ranger Abilities
/// End of Ranger Abilities
///////////////////////////////////////////////////////////
    IEnumerator PlayerHeal()
    {   
        if (ActiveEvent() )
        {
            StartCoroutine(ActivateEvent());
        }
        state = BattleState.ENEMYTURN;
        tankUnit.Heal(5);

        tankHud.SetHP(tankUnit.currentHP);
        sitText.text = "You healed";

        yield return new WaitForSeconds(time);

        
        StartCoroutine(EnemyTurn());

    }

    Unit Dangerous()
    {
        Unit dangerous = null;
        foreach (Unit member in members){
            if (dangerous == null || member.dangerLevel > dangerous.dangerLevel  )
            {
                if (member.dangerLevel > 0)
                {
                    dangerous = member;
                }
            }
            
            
        }
        return dangerous;
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
            sitText.text = "Enemy has been defeated";

        }else if (state == BattleState.LOSS)
        {
            sitText.text = "Your dead";

            
        }
    }

    IEnumerator EnemyTurn()
    {
        updateEffect();
        yield return new WaitForSeconds(time);
        numRolled = 0;
        damageBoost = 1;
        healBoost = 1;
        rollDisplay.text = "Roll: " + numRolled;
        hasDiceRolled = false;
        if (ActiveEvent())
        {
            switch (randomEvent)
            {
                case "Wet Floor":
                    sitText.text = "Enemy slipped.";
            
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
                    sitText.text = "Enemy fell into a hole and took damage.";

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
        sitText.text = enemyUnit.unitName + " attacks!";

        yield return new WaitForSeconds(time);

        Unit attackedUnit;
        attackedUnit = Dangerous();
        attackedUnit.TakeDamage(enemyUnit.damage);
        bool isDead = attackedUnit.IsDead(attackedUnit.currentHP);
        
        if (attackedUnit == tankUnit)
        {
            tankHud.SetHP(tankUnit.currentHP);
        }
        else if (attackedUnit == swordUnit)
        {
            swordHud.SetHP(swordUnit.currentHP);
        }
        
        

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

    

    /// <summary>
    /// The following are buttons pressed.
    /// </summary>
    public void OnAttackButton()
    {
        if(state != BattleState.PLAYERTURN)
        {
            return;
        }

        StartCoroutine(PlayerAttack());
    }
/////////////////////////////////////////////////////////////
/// This are Tank Buttons
    public void OnAggroButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        StartCoroutine(PullAggro());
    }

    public void OnBashButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        
        StartCoroutine(Bash());

    }

    public void OnUltDefButton()
    {
         if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        StartCoroutine(UltDef());
    }
//////////////////////////////////////////////////////////////
/// These are Sword Buttons
    public void OnSwordSlashButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        StartCoroutine(SwordSlash());
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
/// <summary>
/// The following are effects called during events.
/// </summary>
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
