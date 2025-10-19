using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour 
{
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}