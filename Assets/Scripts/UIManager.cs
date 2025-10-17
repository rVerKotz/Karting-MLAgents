using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Elemen UI Balapan")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI positionText;

    [Header("Panel Kemenangan")]
    public GameObject winPanel;
    public TextMeshProUGUI winText;

    void Start()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void UpdateTime(float time)
    {
        if (timeText == null) return;

        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 100) % 100);
        timeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void UpdateLap(int currentLap, int totalLaps)
    {
        if (lapText == null) return;

        int lapToShow = Mathf.Min(currentLap, totalLaps);
        lapText.text = "Lap: " + lapToShow + "/" + totalLaps;
    }

    public void UpdatePosition(int currentPosition, int totalRacers)
    {
        if (positionText == null) return;
        positionText.text = "Pos: " + currentPosition + "/" + totalRacers;
    }

    public void ShowWinScreen(string winnerName)
    {
        if (winPanel == null) return;

        winPanel.SetActive(true);
        winText.text = winnerName + " Menang!";
    }
}