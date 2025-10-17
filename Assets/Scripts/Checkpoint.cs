using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [HideInInspector]
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        // Mulai pencarian dari objek yang memicu trigger
        Transform currentObject = other.transform;
        Transform kartRoot = null;

        // Terus naik ke atas dalam hirarki sampai menemukan Tag yang benar atau sampai ke puncak
        while (currentObject != null)
        {
            if (currentObject.CompareTag("Player") || currentObject.CompareTag("NPC"))
            {
                kartRoot = currentObject;
                break; // Ditemukan! Keluar dari loop.
            }
            currentObject = currentObject.parent; // Naik satu level
        }

        // Jika kartRoot berhasil ditemukan (tidak lagi null)...
        if (kartRoot != null)
        {
            // Panggil RaceManager dengan referensi yang benar
            RaceManager.Instance.OnCheckpointReached(kartRoot, checkpointIndex);
        }
    }
}