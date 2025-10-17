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

    public Racer(string name, Transform kartTransform)
    {
        Name = name;
        Lap = 1;
        CheckpointIndex = 0;
        KartTransform = kartTransform;
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

        if (uiManager != null && racers.Any(r => r.Name == "Player"))
        {
            uiManager.UpdateLap(1, totalLaps);
            UpdateUIPosition();
        }
    }

    void Update()
    {
        if (isRaceOver) return;
        raceTime += Time.deltaTime;
        if (uiManager != null) uiManager.UpdateTime(raceTime);
        UpdateRacerProgress();
        SortRacers();
        UpdateUIPosition();
    }

    void RegisterRacers()
    {
        racers.Clear();
        GameObject playerKart = GameObject.FindWithTag("Player");
        if (playerKart != null)
        {
            racers.Add(new Racer("Player", playerKart.transform));
        }
        GameObject[] npcKarts = GameObject.FindGameObjectsWithTag("NPC");
        foreach (var npcKart in npcKarts)
        {
            racers.Add(new Racer("NPC " + (racers.Count), npcKart.transform));
        }
    }

    void UpdateRacerProgress()
    {
        foreach (var racer in racers)
        {
            if (racer.CheckpointIndex < checkpoints.checkPoints.Count)
            {
                Vector3 nextCpPos = checkpoints.checkPoints[racer.CheckpointIndex].transform.position;
                racer.DistanceToNextCp = Vector3.Distance(racer.KartTransform.position, nextCpPos);
            }
        }
    }

    void SortRacers()
    {
        racers = racers.OrderByDescending(r => r.Lap)
                       .ThenByDescending(r => r.CheckpointIndex)
                       .ThenBy(r => r.DistanceToNextCp)
                       .ToList();
    }

    void UpdateUIPosition()
    {
        if (uiManager == null) return;
        for (int i = 0; i < racers.Count; i++)
        {
            if (racers[i].Name == "Player")
            {
                uiManager.UpdatePosition(i + 1, racers.Count);
                return;
            }
        }
    }

    public void OnCheckpointReached(Transform kartTransform, int checkpointIndex)
    {
        Racer racer = racers.Find(r => r.KartTransform == kartTransform);
        if (racer == null) return;
        if (racer.CheckpointIndex == checkpointIndex)
        {
            if (racer.CheckpointIndex == checkpoints.checkPoints.Count - 1)
            {
                OnLapCompleted(racer);
                racer.CheckpointIndex = 0;
            }
            else
            {
                racer.CheckpointIndex++;
            }
        }
    }

    void OnLapCompleted(Racer racer)
    {
        if (isRaceOver) return;
        racer.Lap++;
        if (racer.Name == "Player")
        {
            uiManager.UpdateLap(racer.Lap, totalLaps);
            if (racer.Lap > totalLaps)
            {
                EndRace(racer);
            }
        }
    }

    void EndRace(Racer winner)
    {
        isRaceOver = true;
        Time.timeScale = 0;
        if (uiManager != null) uiManager.ShowWinScreen(winner.Name);
    }

    public Racer GetRacerData(Transform kartTransform)
    {
        return racers.Find(r => r.KartTransform == kartTransform);
    }
}