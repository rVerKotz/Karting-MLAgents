using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ... (Kelas Racer tetap sama) ...
public class Racer
{
    public string Name { get; private set; }
    public int Lap { get; set; }
    public int CheckpointIndex { get; set; } // Ini adalah INDEX DARI CHECKPOINT BERIKUTNYA YANG HARUS DICAPAI
    public float DistanceToNextCp { get; set; }
    public Transform KartTransform { get; private set; }
    public KartController KartController { get; private set; }

    public Racer(string name, KartController kartController)
    {
        Name = name;
        Lap = 1;
        CheckpointIndex = 0; // Awalnya, target adalah checkpoint index 0
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

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        Time.timeScale = 1;
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
        Debug.Log($"RaceManager.RegisterRacers: Menemukan {karts.Length} KartControllers.");
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
            Debug.Log($"RaceManager.AddKart: Kart {kart.name} ({racerName}) ditambahkan. Total: {racers.Count}");
        }
        else
        {
            Debug.LogWarning($"RaceManager.AddKart: Kart {kart.name} sudah terdaftar.");
        }
    }

    public void RemoveKart(KartController kart)
    {
        Racer racerToRemove = racers.FirstOrDefault(r => r.KartController == kart);
        if (racerToRemove != null)
        {
            racers.Remove(racerToRemove);
            Debug.Log($"RaceManager.RemoveKart: Kart {kart.name} dihapus. Total: {racers.Count}");
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
                    // Debug.LogWarning($"[{racer.Name}] UpdateRacerDistances: nextCheckpoint atau KartTransform null (Index target: {nextCpIndex})");
                }
            }
            else
            {
                racer.DistanceToNextCp = float.MaxValue; // Index tidak valid
                                                         // Debug.LogError($"[{racer.Name}] UpdateRacerDistances: Invalid nextCpIndex calculated: {nextCpIndex}");
            }
        }
    }

    void SortRacers()
    {
        racers = racers.OrderByDescending(r => r.Lap)
                       .ThenByDescending(r => r.CheckpointIndex) // Index TERTINGGI yang dituju/dicapai lebih dulu
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

    public void OnCheckpointReached(KartController kartController, int passedCheckpointIndex) // Ubah nama parameter
    {
        if (checkpoints == null || checkpoints.checkPoints == null || checkpoints.checkPoints.Count == 0)
        {
            Debug.LogError("RaceManager.OnCheckpointReached: Checkpoints tidak valid!");
            return;
        }


        Racer racer = racers.FirstOrDefault(r => r.KartController == kartController);
        if (racer == null)
        {
            Debug.LogWarning($"RaceManager.OnCheckpointReached: Racer untuk {kartController.name} tidak ditemukan.");
            return;
        }

        // --- AWAL DEBUGGING LOG ---
        string passedCpName = (passedCheckpointIndex >= 0 && passedCheckpointIndex < checkpoints.checkPoints.Count) ? checkpoints.checkPoints[passedCheckpointIndex].name : "INVALID INDEX";
        Debug.Log($"[{racer.Name}] RaceManager.OnCheckpointReached: Melewati CP Index={passedCheckpointIndex} ({passedCpName}). Target saat ini adalah Index={racer.CheckpointIndex}.");
        // --- AKHIR DEBUGGING LOG ---


        // Dapatkan index checkpoint yang SEHARUSNYA dilewati (target saat ini)
        int expectedCheckpointToPass = racer.CheckpointIndex % checkpoints.checkPoints.Count;
        // Handle jika targetnya adalah garis start (index >= count)
        if (racer.CheckpointIndex >= checkpoints.checkPoints.Count)
        {
            expectedCheckpointToPass = 0;
        }


        // Kondisi utama: Checkpoint yang dilewati adalah yang diharapkan
        if (passedCheckpointIndex == expectedCheckpointToPass)
        {
            Debug.Log($"[{racer.Name}] Passed CORRECT checkpoint ({passedCpName}). Calculating next target...");

            // Tentukan index checkpoint BERIKUTNYA yang harus dituju
            int nextTargetIndex = (passedCheckpointIndex + 1);

            if (passedCheckpointIndex == 0 && racer.CheckpointIndex >= checkpoints.checkPoints.Count)
            {
                Debug.Log($"[{racer.Name}] Crossed FINISH LINE (Index 0) after completing lap. Processing lap completion...");
                OnLapCompleted(racer); // Proses penambahan lap
                racer.CheckpointIndex = 1; // Target berikutnya adalah checkpoint setelah start (index 1)
                Debug.Log($"[{racer.Name}] Lap completed. NEW TARGET INDEX SET TO: {racer.CheckpointIndex}");
            }
            // Kondisi utama: Checkpoint yang dilewati adalah yang diharapkan
            else if (passedCheckpointIndex == expectedCheckpointToPass)
            {
                Debug.Log($"[{racer.Name}] Passed CORRECT checkpoint ({passedCpName}). Calculating next target...");

                // Tentukan index checkpoint BERIKUTNYA yang harus dituju
                nextTargetIndex = (passedCheckpointIndex + 1);

                // Jika checkpoint yang baru dilewati adalah yang TERAKHIR (index = count-1)
                if (passedCheckpointIndex == checkpoints.checkPoints.Count - 1)
                {
                    // Target berikutnya adalah 'state' di luar batas index, menandakan siap melewati garis start
                    nextTargetIndex = checkpoints.checkPoints.Count;
                    Debug.Log($"[{racer.Name}] Passed LAST checkpoint. Next target state set to {nextTargetIndex} (ready for finish line).");
                }

                // Update target racer
                racer.CheckpointIndex = nextTargetIndex;
                Debug.Log($"[{racer.Name}] TARGET INDEX UPDATED TO: {racer.CheckpointIndex}");
            }

            // Update target racer
            racer.CheckpointIndex = nextTargetIndex;
            Debug.Log($"[{racer.Name}] TARGET INDEX UPDATED TO: {racer.CheckpointIndex}");

        }
        // Kondisi khusus: Melewati garis start (index 0) SETELAH menyelesaikan semua checkpoint (target >= count)
        else if (passedCheckpointIndex == 0 && racer.CheckpointIndex >= checkpoints.checkPoints.Count)
        {
            Debug.Log($"[{racer.Name}] Crossed FINISH LINE (Index 0) after completing lap. Processing lap completion...");
            OnLapCompleted(racer); // Proses penambahan lap
            racer.CheckpointIndex = 1; // Target berikutnya adalah checkpoint setelah start (index 1)
            Debug.Log($"[{racer.Name}] Lap completed. NEW TARGET INDEX SET TO: {racer.CheckpointIndex}");
        }
        else
        {
            Debug.LogWarning($"[{racer.Name}] Passed WRONG checkpoint ({passedCpName}). Expected: Index {expectedCheckpointToPass}. Ignoring.");
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
            Debug.Log($"[{racer.Name}] Calling SetNextCheckpointInternal on {cm.gameObject.name}...");
            cm.SetNextCheckpointInternal();
        }
        else
        {
            Debug.LogError($"[{racer.Name}] RaceManager.OnCheckpointReached: Gagal menemukan CheckpointManager untuk memanggil SetNextCheckpointInternal!");
        }
    }

    public void ResetRacerState(KartController kartController)
    {
        Racer racer = racers.FirstOrDefault(r => r.KartController == kartController);
        if (racer != null)
        {
            racer.Lap = 1;
            racer.CheckpointIndex = 0; 
            Debug.Log($"[{racer.Name}] RaceManager.ResetRacerState: State direset (Lap: {racer.Lap}, Target CP Index: {racer.CheckpointIndex})");
        }
        else
        {
            Debug.LogWarning($"RaceManager.ResetRacerState: Gagal menemukan racer untuk {kartController?.name}");
        }
    }


    void OnLapCompleted(Racer racer)
    {
        if (isRaceOver) return;
        racer.Lap++;
        Debug.Log($"[{racer.Name}] OnLapCompleted: Lap incremented to {racer.Lap}");

        if (racer.Name == "Player" && uiManager != null)
        {
            uiManager.UpdateLap(racer.Lap, totalLaps);
            if (racer.Lap > totalLaps)
            {
                Debug.Log($"[{racer.Name}] Player finished the race!");
                EndRace(racer);
            }
        }
        else // Untuk AI
        {
            if (racer.Lap > totalLaps)
            {
                Debug.Log($"[{racer.Name}] AI finished the race!");
                if (racer.KartController?.GetComponent<KartAgent>() != null)
                {
                    Debug.Log($"[{racer.Name}] Ending episode for AI agent.");
                    // racer.KartController.GetComponent<KartAgent>().EndEpisode(); // Bisa diaktifkan jika ingin AI berhenti setelah finish
                }
            }
        }
    }

    void EndRace(Racer winner)
    {
        isRaceOver = true;
        Time.timeScale = 0;
        if (uiManager != null)
        {
            uiManager.ShowGameOver($"Pemenang: {winner.Name}!", raceTime);
        }
        Debug.Log($"Race Over! Winner: {winner.Name}, Time: {raceTime}");
    }

    public List<Racer> GetRacers()
    {
        return racers;
    }
}