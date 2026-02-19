using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    public static SceneController instance;

    private void start()
    {
        /*if (instance == null)                       //This line makes sure that the scene doesn't get destroyed.
        {
            instance = this;
            DontDestroyOnLoad(Cube); 
        }
        else
        {
            Destroy(Cube);                    //This line destroy this object if the other scene already holds it.
        }
        */
    }

    public static void EnterDungeon()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex+1);
    }

    public void LoadDungeon(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
 
}
