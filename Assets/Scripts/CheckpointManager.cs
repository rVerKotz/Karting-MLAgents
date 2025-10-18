using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public float MaxTimeToReachNextCheckpoint = 30f;
    public float TimeLeft = 30f;
    public float TimeToAddOnCorrectCheckpoint = 5f; // <- Variabel baru, atur di Inspector
    public Checkpoint nextCheckPointToReach;

    private int CurrentCheckpointIndex;
    private List<Checkpoint> Checkpoints;
    private Checkpoint lastCheckpoint;
    private KartController kartController;
    private KartAgent kartAgent;

    // Jaga signature event tetap sama agar AutomaticCameraSystem tidak error
    public event Action<Checkpoint> reachedCheckpoint;

    void Awake()
    {
        Transform rootParent = transform.parent;
        if (rootParent != null)
        {
            kartController = rootParent.GetComponentInChildren<KartController>(true);
            kartAgent = rootParent.GetComponentInChildren<KartAgent>(true);
        }

        if (kartController == null)
        {
            Debug.LogError($"[{gameObject.name}] CheckpointManager.Awake: Tidak menemukan KartController di parent atau child-nya!", transform.root.gameObject);
        }
        // else { Debug.Log($"[{kartController.name}] CheckpointManager.Awake: KartController ditemukan.", gameObject); } // Debug Log bisa dihapus jika sudah beres
    }

    void Start()
    {
        Checkpoints checkpointsComponent = FindObjectOfType<Checkpoints>();
        if (checkpointsComponent != null && checkpointsComponent.checkPoints != null)
        {
            Checkpoints = checkpointsComponent.checkPoints;
            // Debug.Log($"[{kartController?.name ?? gameObject.name}] CheckpointManager.Start: List Checkpoints didapatkan (Count: {Checkpoints.Count})."); // Debug Log bisa dihapus
        }
        else
        {
            Debug.LogError("Objek Checkpoints atau list checkPoints-nya tidak ditemukan/valid di scene!");
            enabled = false;
            return;
        }
        SetNextCheckpointInternal();
    }

    public void ResetCheckpoints()
    {
        TimeLeft = MaxTimeToReachNextCheckpoint; // Reset waktu saat episode baru dimulai
        // Debug.Log($"[{kartController?.name ?? gameObject.name}] CheckpointManager.ResetCheckpoints: Memanggil SetNextCheckpointInternal..."); // Debug Log bisa dihapus
        SetNextCheckpointInternal();
    }

    private void Update()
    {
        TimeLeft -= Time.deltaTime;

        if (TimeLeft < 0f)
        {
            if (kartAgent != null)
            {
                // Debug.LogWarning($"[{kartController?.name ?? gameObject.name}] CheckpointManager.Update: Waktu habis! Mengakhiri episode."); // Debug Log bisa dihapus
                kartAgent.AddReward(-1.0f); // Penalti waktu habis
                kartAgent.EndEpisode();
            }
        }
    }

    public void CheckPointReached(Checkpoint checkpoint, Collider other)
    {
        // Debug.Log($"[{kartController?.name ?? gameObject.name}] CheckpointManager.CheckPointReached Dipanggil. Target: {nextCheckPointToReach?.name}, Yang Dilewati: {checkpoint?.name}"); // Debug Log bisa dihapus

        // 1. Cek apakah checkpoint yang dilewati adalah target
        if (nextCheckPointToReach != checkpoint)
        {
            // Debug.LogWarning($"[{kartController?.name ?? gameObject.name}] CheckpointManager.CheckPointReached: Checkpoint yang dilewati ({checkpoint?.name}) BUKAN target ({nextCheckPointToReach?.name}). Mengabaikan."); // Debug Log bisa dihapus
            return;
        }

        // 2. Pastikan KartController valid
        if (kartController == null)
        {
            Debug.LogError($"[{gameObject.name}] CheckpointManager.CheckPointReached: kartController adalah null saat mencoba memproses checkpoint!");
            return;
        }

        // 3. Tambahkan Waktu
        TimeLeft += TimeToAddOnCorrectCheckpoint;
        // Opsional: Batasi waktu maksimal
        // TimeLeft = Mathf.Min(TimeLeft, MaxTimeToReachNextCheckpoint);
        Debug.Log($"[{kartController.name}] CheckpointManager.CheckPointReached: Waktu ditambah {TimeToAddOnCorrectCheckpoint}s. Sisa waktu: {TimeLeft}s.");

        // 4. Trigger Event & Panggil RaceManager
        // Debug.Log($"[{kartController.name}] CheckpointManager.CheckPointReached: Checkpoint BENAR ({checkpoint.name}). Memanggil RaceManager & event..."); // Debug Log bisa dihapus

        lastCheckpoint = checkpoint;
        reachedCheckpoint?.Invoke(checkpoint); // Kirim event

        if (RaceManager.Instance != null)
        {
            int passedCheckpointIndex = Checkpoints.IndexOf(checkpoint);
            if (passedCheckpointIndex != -1)
            {
                // Debug.Log($"[{kartController.name}] CheckpointManager.CheckPointReached: Memanggil RaceManager.OnCheckpointReached dengan index {passedCheckpointIndex}."); // Debug Log bisa dihapus
                RaceManager.Instance.OnCheckpointReached(kartController, passedCheckpointIndex);
            }
            else
            {
                Debug.LogError($"[{kartController.name}] CheckpointManager.CheckPointReached: Gagal mendapatkan index dari checkpoint {checkpoint.name}!");
            }
        }
        else
        {
            Debug.LogError($"[{kartController.name}] CheckpointManager.CheckPointReached: RaceManager.Instance adalah null!");
        }

        // 5. Reward dipindahkan ke KartAgent yang menerima event
    }

    public void SetNextCheckpointInternal()
    {
        CurrentCheckpointIndex = GetCurrentCheckpointIndex();
        // Debug.Log($"[{kartController?.name ?? gameObject.name}] SetNextCheckpointInternal: Dipanggil. Index dari RaceManager: {CurrentCheckpointIndex}"); // Debug Log bisa dihapus

        if (Checkpoints != null && Checkpoints.Count > 0)
        {
            int indexToSet = CurrentCheckpointIndex % Checkpoints.Count;
            if (CurrentCheckpointIndex >= Checkpoints.Count)
            {
                indexToSet = 0;
                // Debug.Log($"[{kartController?.name ?? gameObject.name}] SetNextCheckpointInternal: Index >= Count, target diset ke 0 (Garis Start)."); // Debug Log bisa dihapus
            }

            if (indexToSet >= 0 && indexToSet < Checkpoints.Count)
            {
                nextCheckPointToReach = Checkpoints[indexToSet];
                // TimeLeft TIDAK direset di sini, hanya saat ResetCheckpoints atau ditambah saat checkpoint benar
                // Debug.Log($"[{kartController?.name ?? gameObject.name}] SetNextCheckpointInternal: nextCheckPointToReach diatur ke Checkpoint {indexToSet} ({nextCheckPointToReach?.name})"); // Debug Log bisa dihapus
            }
            else
            {
                Debug.LogError($"[{kartController?.name ?? gameObject.name}] SetNextCheckpointInternal: Calculated index {indexToSet} is out of bounds for Checkpoints list (Count: {Checkpoints.Count}).");
                nextCheckPointToReach = null;
            }
        }
        else
        {
            // Debug.LogWarning($"[{kartController?.name ?? gameObject.name}] SetNextCheckpointInternal: List Checkpoints null or empty."); // Debug Log bisa dihapus
            nextCheckPointToReach = null;
        }
    }

    public Transform GetNextCheckpointTransform()
    {
        return nextCheckPointToReach?.transform;
    }

    public int GetCurrentCheckpointIndex()
    {
        if (RaceManager.Instance != null && kartController != null)
        {
            Racer racer = RaceManager.Instance.GetRacers()?.FirstOrDefault(r => r.KartController == kartController);
            if (racer != null)
            {
                return racer.CheckpointIndex;
            }
            // else { Debug.LogWarning($"[{kartController.name}] GetCurrentCheckpointIndex: Racer tidak ditemukan di RaceManager."); } // Debug Log bisa dihapus
        }
        // else { Debug.LogWarning($"[{kartController?.name ?? gameObject.name}] GetCurrentCheckpointIndex: RaceManager atau KartController null."); } // Debug Log bisa dihapus

        // Debug.LogWarning($"[{kartController?.name ?? gameObject.name}] GetCurrentCheckpointIndex: Fallback ke nilai lokal awal (0)."); // Debug Log bisa dihapus
        return 0;
    }
}