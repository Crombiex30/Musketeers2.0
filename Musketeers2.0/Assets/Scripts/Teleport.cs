using UnityEngine;
using UnityEngine.SceneManagement;
public class Teleport : MonoBehaviour
{

    public string scenename;

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneController.EnterZone(scenename);
        }
    }
}
