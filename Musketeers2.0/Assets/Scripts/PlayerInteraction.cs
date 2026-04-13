using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float intteractDistance = 3f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask InteractableLayer;

    private Interactable currentInteractable;

    private void Update()
    {
        CheckForInteractable();
        
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    private void CheckForInteractable()
    {
        currentInteractable = null;
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, intteractDistance, InteractableLayer))
        {
            currentInteractable = hit.collider.GetComponent<Interactable>();

            if (currentInteractable == null)
            {
                currentInteractable = hit.collider.GetComponentInParent<Interactable>();
            }
        }
    }

    public Interactable GetInteractable()
    {
        return currentInteractable;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * intteractDistance);
        }
    }
}
