using UnityEngine;


//[CreateAssetMenu(fileName = "NewUnit", menuName = "Units/Unit")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int maxHP;
    public int maxMP;
    public int attack;
    public int damage;
}
