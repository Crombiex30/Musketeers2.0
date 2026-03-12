using Unity.VisualScripting;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    public GameObject container;
    public PlayerInteraction interact;


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
