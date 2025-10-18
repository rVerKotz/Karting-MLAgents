using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    private List<Transform> availableSpawnpoints = new List<Transform>();
    private List<Transform> allSpawnpoints = new List<Transform>();
    private readonly object syncLock = new object();

    private void Awake()
    {
        allSpawnpoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            allSpawnpoints.Add(child);
        }

        if (allSpawnpoints.Count == 0)
        {
            Debug.LogError("SpawnPointManager: Tidak ditemukan child Transform sebagai spawn points! Pastikan GameObject spawn point adalah anak LANGSUNG dari GameObject ini.");
        }
        else
        {
            Debug.Log($"SpawnPointManager: Ditemukan {allSpawnpoints.Count} spawn points dari child objects.");
            availableSpawnpoints = new List<Transform>(allSpawnpoints); 
        }
    }

    public Vector3 SelectRandomSpawnpoint()
    {
        lock (syncLock)
        {
            if (availableSpawnpoints == null)
            {
                Debug.LogError("SpawnPointManager: availableSpawnpoints belum diinisialisasi!");
                return transform.position;
            }

            if (availableSpawnpoints.Count == 0 && allSpawnpoints.Count > 0)
            {
                ResetAvailableSpawnpoints_Internal();
            }

            if (availableSpawnpoints.Count == 0)
            {
                Debug.LogWarning("SpawnPointManager: Tidak ada spawn point tersedia untuk dipilih!");
                return transform.position;
            }

            int randomIndex = Random.Range(0, availableSpawnpoints.Count);
            Transform selectedSpawnpoint = availableSpawnpoints[randomIndex];
            availableSpawnpoints.RemoveAt(randomIndex);

            if (selectedSpawnpoint == null)
            {
                Debug.LogError("SpawnPointManager: Spawn point yang terpilih ternyata null!");
                return transform.position;
            }

            return selectedSpawnpoint.position;
        }
    }

    private void ResetAvailableSpawnpoints_Internal()
    {
        availableSpawnpoints.Clear();
        if (allSpawnpoints.Count > 0)
        {
            availableSpawnpoints.AddRange(allSpawnpoints);
            ShuffleList(availableSpawnpoints);
        }
    }

    public void ResetAvailableSpawnpoints()
    {
        lock (syncLock)
        {
            ResetAvailableSpawnpoints_Internal();
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}