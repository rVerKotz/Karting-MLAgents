using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour // <-- DIUBAH: Nama class harus 'GameOver'
{
    // Saya ganti nama fungsinya agar lebih jelas
    public void BackToMainMenu()
    {
        // Pastikan Anda punya scene bernama "MainMenu" di Build Settings
        SceneManager.LoadScene("MainMenu");
    }
}