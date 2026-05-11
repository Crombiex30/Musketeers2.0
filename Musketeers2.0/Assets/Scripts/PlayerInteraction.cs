using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 1f;
    [SerializeField] private string interactableTag = "Interactable";

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

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactDistance);

        float closestDistance = Mathf.Infinity;
        Interactable closestInteractable = null;

        foreach (Collider collider in nearbyColliders)
        {
            if (!collider.CompareTag(interactableTag))
            {
                continue;
            }

            Interactable interactable = collider.GetComponent<Interactable>();

            if (interactable == null)
            {
                interactable = collider.GetComponentInParent<Interactable>();
            }

            if (interactable == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, collider.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }

        currentInteractable = closestInteractable;
    }

    public Interactable GetInteractable()
    {
        return currentInteractable;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}