using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState{ START, PLAYERTURN, ENEMYTURN, WON, LOSS }
public class BattleSystem : MonoBehaviour
{

    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Text turn; 

    Unit playerUnit;
    Unit enemyUnit;

    public BattleState state;

    void Start()
    {
        state = BattleState.START;
        SetUpBattle();
    }

    void SetUpBattle()
    {
        GameObject playerGO = Instantiate(playerPrefab);
        playerUnit = playerGO.GetComponent<Unit>();

        GameObject enemyGO = Instantiate(enemyPrefab);
        enemyUnit = enemyGO.GetComponent<Unit>();
        
        turn.text = playerUnit.unitName + "'s turn";
        
    }
}
