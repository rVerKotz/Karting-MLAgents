using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [HideInInspector]
    public int checkpointIndex; // Pastikan ini diisi oleh Checkpoints.cs

    // Hapus referensi CheckpointManager dari sini, biarkan dicari saat trigger

    private void OnTriggerEnter(Collider other)
    {
        // Cari CheckpointManager pada kart yang menabrak
        CheckpointManager checkpointManager = other.GetComponentInParent<CheckpointManager>();

        if (checkpointManager == null)
        {
            // Debug.Log($"Checkpoint {name} (Index {checkpointIndex}): Ditolak trigger dari {other.name} karena tidak ada CheckpointManager."); // Bisa terlalu banyak log
            return; // Jika bukan kart atau tidak ada manager, abaikan
        }

        Debug.Log($"Checkpoint {name} (Index {checkpointIndex}): Dipicu oleh {other.name}. Memanggil CheckPointReached di manager {checkpointManager.gameObject.name}...");
        // Panggil CheckPointReached di manager kart tersebut
        checkpointManager.CheckPointReached(this, other);
    }
}