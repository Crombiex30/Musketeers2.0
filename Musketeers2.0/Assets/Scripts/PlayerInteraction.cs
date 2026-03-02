using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float interactRange = 2f;
            Collider [] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                if(collider.TryGetComponent(out NPCInteraction npcInteraction))
                {
                    npcInteraction.Interact();
                }
            }
        }
    }



}
