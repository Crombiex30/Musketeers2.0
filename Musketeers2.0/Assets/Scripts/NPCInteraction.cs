using System;
using UnityEngine;

public class NPCInteraction : Interactable
{
    public GameObject container;
    public override void Interact()
    {
        container.SetActive(!container.activeSelf);
    }



}
