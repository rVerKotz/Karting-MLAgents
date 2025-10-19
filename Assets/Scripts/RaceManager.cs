using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Racer
{
    public string Name { get; private set; }
    public int Lap { get; set; }
    public int CheckpointIndex { get; set; }
    public float DistanceToNextCp { get; set; }
    public Transform KartTransform { get; private set; }
    public KartController KartController { get; private set; }

    public Racer(string name, KartController kartController)
    {
        Name = name;
        Lap = 1;
        CheckpointIndex = 0;
        KartTransform = kartController.transform;
        KartController = kartController;
    }
}


public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    [Header("Pengaturan Balapan")]
    public int totalLaps = 5;

    [Header("Referensi")]
    public UIManager uiManager;
    public Checkpoints checkpoints;

    private List<Racer> racers = new List<Racer>();
    private float raceTime;
    private bool isRaceOver = false;
    private bool isTutorialHidden = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        Time.timeScale = 0f;
        isTutorialHidden = false;
        RegisterRacers();

        Racer playerRacer = racers.FirstOrDefault(r => r.Name == "Player");

        if (uiManager != null && playerRacer != null)
        {
            uiManager.UpdateLap(playerRacer.Lap, totalLaps);
        }
    }

    void RegisterRacers()
    {
        racers.Clear();
        KartController[] karts = FindObjectsOfType<KartController>();
        foreach (KartController kart in karts)
        {
            AddKart(kart);
        }
        UpdatePlayerPositionUI();
    }


    public void AddKart(KartController kart)
    {
        if (racers.All(r => r.KartController != kart))
        {
            string racerName = kart.GetComponent<PlayerController>() != null ? "Player" : $"AI Kart {racers.Count + 1}";
            racers.Add(new Racer(racerName, kart));
        }
    }

    public void RemoveKart(KartController kart)
    {
        Racer racerToRemove = racers.FirstOrDefault(r => r.KartController == kart);
        if (racerToRemove != null)
        {
            racers.Remove(racerToRemove);
        }
    }


    void Update()
    {
        if (isRaceOver || checkpoints == null || checkpoints.checkPoints == null) return;

        raceTime += Time.deltaTime;
        UpdateRacerDistances();
        SortRacers();
        UpdatePlayerPositionUI();

        if (uiManager != null)
        {
            uiManager.UpdateTime(raceTime);
        }
    }

    public void NotifyPlayerInput()
    {
        if (isTutorialHidden) return;
        Time.timeScale = 1f;
        if (uiManager != null)
        {
            uiManager.HideTutorial();
        }

        isTutorialHidden = true;
    }

    void UpdateRacerDistances()
    {
        if (checkpoints.checkPoints.Count == 0) return;

        foreach (Racer racer in racers)
        {
            // CheckpointIndex adalah target BERIKUTNYA
            int nextCpIndex = racer.CheckpointIndex % checkpoints.checkPoints.Count;
            // Handle jika index >= count (menuju garis start)
            if (racer.CheckpointIndex >= checkpoints.checkPoints.Count)
            {
                nextCpIndex = 0; // Targetnya adalah garis start
            }

            if (nextCpIndex >= 0 && nextCpIndex < checkpoints.checkPoints.Count) // Pastikan index valid
            {
                Transform nextCheckpoint = checkpoints.checkPoints[nextCpIndex]?.transform;

                if (nextCheckpoint != null && racer.KartTransform != null)
                {
                    racer.DistanceToNextCp = Vector3.Distance(racer.KartTransform.position, nextCheckpoint.position);
                }
                else
                {
                    racer.DistanceToNextCp = float.MaxValue;
                }
            }
            else
            {
                racer.DistanceToNextCp = float.MaxValue; // Index tidak valid
            }
        }
    }

    void SortRacers()
    {
        // Urutkan berdasarkan Lap (tertinggi), lalu CheckpointIndex (tertinggi), lalu jarak ke CP berikutnya (terendah)
        racers = racers.OrderByDescending(r => r.Lap)
                       .ThenByDescending(r => r.CheckpointIndex)
                       .ThenBy(r => r.DistanceToNextCp)
                       .ToList();
    }

    void UpdatePlayerPositionUI()
    {
        if (uiManager == null) return;
        for (int i = 0; i < racers.Count; i++)
        {
            if (racers[i] != null && racers[i].Name == "Player")
            {
                uiManager.UpdatePosition(i + 1, racers.Count);
                break;
            }
        }
    }

    public void OnCheckpointReached(KartController kartController, int passedCheckpointIndex)
    {
        if (checkpoints == null || checkpoints.checkPoints == null || checkpoints.checkPoints.Count == 0)
        {
            return;
        }

        Racer racer = racers.FirstOrDefault(r => r.KartController == kartController);
        if (racer == null)
        {
            return;
        }

        // Dapatkan index checkpoint yang SEHARUSNYA dilewati (target saat ini)
        int expectedCheckpointToPass = racer.CheckpointIndex % checkpoints.checkPoints.Count;
        // Handle jika targetnya adalah garis start (index >= count)
        if (racer.CheckpointIndex >= checkpoints.checkPoints.Count)
        {
            expectedCheckpointToPass = 0;
        }


        // Kondisi khusus: Melewati garis start (index 0) SETELAH menyelesaikan semua checkpoint (target >= count)
        if (passedCheckpointIndex == 0 && racer.CheckpointIndex >= checkpoints.checkPoints.Count)
        {
            OnLapCompleted(racer); // Proses penambahan lap
            racer.CheckpointIndex = 1; // Target berikutnya adalah checkpoint setelah start (index 1)
        }
        // Kondisi utama: Checkpoint yang dilewati adalah yang diharapkan
        else if (passedCheckpointIndex == expectedCheckpointToPass)
        {
            // Tentukan index checkpoint BERIKUTNYA yang harus dituju
            int nextTargetIndex = (passedCheckpointIndex + 1);

            // Jika checkpoint yang baru dilewati adalah yang TERAKHIR (index = count-1)
            if (passedCheckpointIndex == checkpoints.checkPoints.Count - 1)
            {
                // Target berikutnya adalah 'state' di luar batas index, menandakan siap melewati garis start (index = count)
                nextTargetIndex = checkpoints.checkPoints.Count;
            }

            // Update target racer
            racer.CheckpointIndex = nextTargetIndex;
        }
        else
        {
            // Jika checkpoint yang dilewati bukan yang diharapkan, abaikan saja
        }


        // Panggil SetNextCheckpointInternal di CheckpointManager yang bersangkutan untuk update targetnya di sana
        CheckpointManager cm = null;
        if (kartController != null && kartController.transform.parent != null)
        {
            cm = kartController.transform.parent.GetComponentInChildren<CheckpointManager>();
        }

        if (cm != null)
        {
            cm.SetNextCheckpointInternal();
        }
    }

    public void ResetRacerState(KartController kartController)
    {
        Racer racer = racers.FirstOrDefault(r => r.KartController == kartController);
        if (racer != null)
        {
            racer.Lap = 1;
            racer.CheckpointIndex = 0;
        }
    }


    void OnLapCompleted(Racer racer)
    {
        if (isRaceOver) return;
        racer.Lap++;

        if (racer.Name == "Player" && uiManager != null)
        {
            uiManager.UpdateLap(racer.Lap, totalLaps);

            if (racer.Lap > totalLaps)
            {
                int finalPosition = racers.IndexOf(racer) + 1;

                if (finalPosition <= 5)
                {
                    EndRace($"Selamat, Anda Menang!\nPosisi #{finalPosition}");
                }
                else
                {
                    EndRace($"Anda Kalah\nPosisi #{finalPosition}");
                }
            }
        }
        else
        {
            if (racer.Lap > totalLaps)
            {
                EndRace($"[{racer.Name}] AI finished the race!");
                if (racer.KartController?.GetComponent<KartAgent>() != null)
                {
                    EndRace($"[{racer.Name}] Ending episode for AI agent.");
                    racer.KartController.GetComponent<KartAgent>().EndEpisode();
                }
            }
        }
    }

    void EndRace(string message)
    {
        isRaceOver = true;
        Time.timeScale = 0;
        if (uiManager != null)
        {
            uiManager.ShowGameOver(message, raceTime);
        }
    }

    public List<Racer> GetRacers()
    {
        return racers;
    }
}