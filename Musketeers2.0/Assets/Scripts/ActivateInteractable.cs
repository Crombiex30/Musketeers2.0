using UnityEngine;

public class ActivateObjectInteractable : Interactable
{
    [SerializeField] private GameObject objectToActivate;

    public override void Interact()
    {
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }
    }
}