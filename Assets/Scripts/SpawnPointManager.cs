using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnPointManager : MonoBehaviour
{
    private List<Transform> _spawnPoints;

    void Awake()
    {
        _spawnPoints = GetComponentsInChildren<Transform>().ToList();
        _spawnPoints.Remove(this.transform);
    }

    public Vector3 SelectRandomSpawnpoint()
    {
        if (_spawnPoints.Count == 0)
        {
            Debug.LogError("Tidak ditemukan anak object di dalam SpawnPointManager! Harap tambahkan beberapa GameObject kosong sebagai anak untuk dijadikan spawn point. Menggunakan posisi default untuk mencegah crash.");

            return this.transform.position;
        }
        return _spawnPoints[Random.Range(0, _spawnPoints.Count)].position;
    }
}