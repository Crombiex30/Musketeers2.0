using System;
using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public GameObject container;
    private InteractUI action;
    public void Interact()
    {
        if (container.activeSelf == false)
        {
            container.SetActive(true);
        }
        else
        {
            container.SetActive(false);
        }
        
    }



}
