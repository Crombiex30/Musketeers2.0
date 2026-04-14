using UnityEngine;

public class CombatManager : MonoBehaviour
{
   
    public GameObject interactHub;
    public GameObject tankHud;
    public GameObject swordHud;



    
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

}
