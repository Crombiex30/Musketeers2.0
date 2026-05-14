using UnityEngine;

public class CombatManager : MonoBehaviour
{
   
    public GameObject interactHub;
    public GameObject tankHud;
    public GameObject swordHud;
    public GameObject healerHud;
    public GameObject rangerHud;
    public bool tank = false;
    public bool sword = false;
    public bool healer = false;
    public bool ranger = false;




    
    public void Back()
    {
        if (tankHud == true)
        {
            tankHud.SetActive(false);
            tank = false;
        }
        if(swordHud == true)
        {
            swordHud.SetActive(false);
            sword = false;
        }
        if(healerHud == true)
        {
            healerHud.SetActive(false);
            healer = false;
        }
        if(rangerHud == true)
        {
            rangerHud.SetActive(false);
            ranger = false; 
        }

        interactHub.SetActive(true);
    }

    public void Tank()
    {
        interactHub.SetActive(false);
        tankHud.SetActive(true);
        tank = true;
        Debug.Log("You made it here");
        
    }

    public void Sword()
    {
        interactHub.SetActive(false);
        swordHud.SetActive(true);
        sword = true;
    }
    public void Healer()
    {
        interactHub.SetActive(false);
        healerHud.SetActive(true);
        healer = true;
    }
    public void Ranger()
    {
        interactHub.SetActive(false);
        rangerHud.SetActive(true);
        ranger = true;
    }

}
