using UnityEngine;
using UnityEngine.SceneManagement;
public class Teleport : Interactable
{

    public string scenename;

    public override void Interact()
    {
        SceneController.EnterZone(scenename);
    }
}
