using UnityEngine;
using UnityEngine.SceneManagement; 

public class StartMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Homebase");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game"); 
        Application.Quit();
    }
}