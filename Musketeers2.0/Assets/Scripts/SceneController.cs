using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    public static SceneController instance;

    private void start()
    {
        
    }

    public static void EnterZone(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

 
}
