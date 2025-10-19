using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    // Fungsi untuk mulai game
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Fungsi untuk keluar dari game
    public void QuitGame()
    {
        Debug.Log("Keluar dari game...");
        Application.Quit();
    }
}