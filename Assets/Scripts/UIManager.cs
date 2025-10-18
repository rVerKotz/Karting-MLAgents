using UnityEngine;
using TMPro;
using System.Text; // Untuk StringBuilder

public class UIManager : MonoBehaviour
{
    [Header("Elemen UI Balapan")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI positionText;
    public TextMeshProUGUI leaderboardText; // Tambahkan ini untuk leaderboard

    [Header("Panel Game Over")] // Ganti nama dari winPanel
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText; // Ganti nama dari winText

    private StringBuilder leaderboardBuilder = new StringBuilder(); // Untuk efisiensi update leaderboard

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        if (leaderboardText != null)
        {
            leaderboardText.text = ""; // Kosongkan leaderboard di awal
        }
    }

    public void UpdateTime(float time)
    {
        if (timeText == null) return;

        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 100) % 100);
        timeText.text = string.Format("Time: {0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void UpdateLap(int currentLap, int totalLaps)
    {
        if (lapText == null) return;

        int lapToShow = Mathf.Min(currentLap, totalLaps + 1); // Tampilkan lap hingga totalLaps+1 (saat finish)
        lapText.text = $"Lap: {lapToShow}/{totalLaps}";
    }

    public void UpdatePosition(int currentPosition, int totalRacers)
    {
        if (positionText == null) return;
        positionText.text = $"Pos: {currentPosition}/{totalRacers}";
        UpdateLeaderboard(); // Panggil update leaderboard setiap posisi berubah
    }

    // Fungsi baru untuk mengupdate leaderboard
    public void UpdateLeaderboard()
    {
        if (leaderboardText == null || RaceManager.Instance == null) return;

        leaderboardBuilder.Clear();
        var racers = RaceManager.Instance.GetRacers(); // Dapatkan list racer yang sudah diurutkan

        leaderboardBuilder.AppendLine("--- Leaderboard ---");
        int displayCount = Mathf.Min(racers.Count, 5); // Tampilkan top 5

        for (int i = 0; i < displayCount; i++)
        {
            if (racers[i] != null)
            {
                leaderboardBuilder.AppendLine($"{i + 1}. {racers[i].Name} (Lap {racers[i].Lap})");
            }
        }
        leaderboardText.text = leaderboardBuilder.ToString();
    }


    public void ShowGameOver(string message, float finalTime) // Ubah nama fungsi dan parameter
    {
        if (gameOverPanel == null || gameOverText == null) return;

        gameOverPanel.SetActive(true);
        int minutes = (int)(finalTime / 60);
        int seconds = (int)(finalTime % 60);
        int milliseconds = (int)((finalTime * 100) % 100);
        gameOverText.text = $"{message}\nWaktu: {minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}