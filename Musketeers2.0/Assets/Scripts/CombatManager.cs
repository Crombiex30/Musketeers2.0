using UnityEngine;

public class CombatManager : MonoBehaviour
{
   
    public GameObject interactHub;
    public GameObject tankHud;
    public GameObject swordHud;
    public GameObject healerHud;
    public GameObject rangerHud;




    
    public void Back()
    {
        if (tankHud == true)
        {
            tankHud.SetActive(false);
        }
        if(swordHud == true)
        {
            swordHud.SetActive(false);
        }
        if(healerHud == true)
        {
            healerHud.SetActive(false);
        }
        if(rangerHud == true)
        {
            rangerHud.SetActive(false);
        }

        interactHub.SetActive(true);
    }

    public void Tank()
    {
        interactHub.SetActive(false);
        tankHud.SetActive(true);
    }

    public void Sword()
    {
        interactHub.SetActive(false);
        swordHud.SetActive(true);
    }
    public void Healer()
    {
        interactHub.SetActive(false);
        healerHud.SetActive(true);
    }
    public void Ranger()
    {
        interactHub.SetActive(false);
        rangerHud.SetActive(true);
    }

}
