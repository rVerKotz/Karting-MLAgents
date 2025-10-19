using UnityEngine;

public class TutorManager : MonoBehaviour
{
    public GameObject tutorialUI; // panel tutorial kamu
    private bool isTutorialActive = true;

    void Start()
    {
        // pastikan tutorial muncul di awal
        if (tutorialUI != null)
            tutorialUI.SetActive(true);

        // hentikan waktu saat tutorial aktif (opsional)
        Time.timeScale = 0f;
    }

    // fungsi untuk menutup tutorial dari tombol "Mulai"
    public void CloseTutorial()
    {
        if (tutorialUI != null)
            tutorialUI.SetActive(false);

        isTutorialActive = false;
        Time.timeScale = 1f; // lanjutkan waktu game
    }
}