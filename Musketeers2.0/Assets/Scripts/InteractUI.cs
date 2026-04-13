using UnityEngine;
using TMPro;

public class InteractUI : MonoBehaviour
{
    public GameObject container;
    public PlayerInteraction interact;
    public TextMeshProUGUI promptText;

    private void Update()
    {
        Interactable interactable = interact.GetInteractable();

        if (interactable != null)
        {
            Show(interactable.promptMessage);
        }
        else
        {
            Hide();
        }
    }

    private void Show(string message)
    {
        container.SetActive(true);

        if (promptText != null)
        {
            promptText.text = message;
        }
    }

    private void Hide()
    {
        container.SetActive(false);
    }
}