using Unity.VisualScripting;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private PlayerInteraction interact;

    private void Update()
    {
        if (interact.GetObject() != null)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        container.SetActive(true);
    }

    private void Hide()
    {
        container.SetActive(false);
    }
}
