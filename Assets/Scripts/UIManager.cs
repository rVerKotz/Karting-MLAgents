using UnityEngine;
using TMPro;
using System.Text;

public class UIManager : MonoBehaviour
{
    [Header("Elemen UI Balapan")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI positionText;
    public TextMeshProUGUI leaderboardText;

    [Header("Tutorial UI")]
    public TextMeshProUGUI tutorialText;

    [Header("Panel Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;

    private StringBuilder leaderboardBuilder = new StringBuilder();

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        if (leaderboardText != null)
        {
            leaderboardText.text = "";
        }
        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(true);
        }
    }

    public void HideTutorial()
    {
        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(false);
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

        int lapToShow = Mathf.Min(currentLap, totalLaps + 1);
        lapText.text = $"Lap: {lapToShow}/{totalLaps}";
    }

    public void UpdatePosition(int currentPosition, int totalRacers)
    {
        if (positionText == null) return;
        positionText.text = $"Pos: {currentPosition}/{totalRacers}";
        UpdateLeaderboard();
    }

    public void UpdateLeaderboard()
    {
        if (leaderboardText == null || RaceManager.Instance == null) return;

        leaderboardBuilder.Clear();
        var racers = RaceManager.Instance.GetRacers();

        leaderboardBuilder.AppendLine("Leaderboard");
        int displayCount = Mathf.Min(racers.Count, 5);

        for (int i = 0; i < displayCount; i++)
        {
            if (racers[i] != null)
            {
                leaderboardBuilder.AppendLine($"{i + 1}. {racers[i].Name} (Lap {racers[i].Lap})");
            }
        }
        leaderboardText.text = leaderboardBuilder.ToString();
    }


    public void ShowGameOver(string message, float finalTime)
    {
        if (gameOverPanel == null || gameOverText == null) return;

        gameOverPanel.SetActive(true);
        int minutes = (int)(finalTime / 60);
        int seconds = (int)(finalTime % 60);
        int milliseconds = (int)((finalTime * 100) % 100);
        gameOverText.text = $"{message}\nWaktu: {minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}