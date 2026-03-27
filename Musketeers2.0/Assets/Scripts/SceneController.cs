using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    public static SceneController instance;

    

    public static void EnterZone(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

 
}
