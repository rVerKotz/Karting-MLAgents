using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Tooltip("Seret GameObject CameraManager ke sini.")]
    public CameraManager cameraManager;

    public float MaxTimeToReachNextCheckpoint = 30f;
    public float TimeLeft = 30f;

    public KartAgent kartAgent;
    public Checkpoint nextCheckPointToReach;

    private int CurrentCheckpointIndex;
    private List<Checkpoint> Checkpoints;
    private Checkpoint lastCheckpoint;

    public event Action<Checkpoint> reachedCheckpoint;

    void Start()
    {
        Checkpoints = FindObjectOfType<Checkpoints>().checkPoints;
        ResetCheckpoints();
    }

    public void ResetCheckpoints()
    {
        CurrentCheckpointIndex = 0;
        TimeLeft = MaxTimeToReachNextCheckpoint;

        if (cameraManager != null)
        {
            cameraManager.SwitchToPlayerCamera();
        }

        SetNextCheckpoint();
    }

    private void Update()
    {
        TimeLeft -= Time.deltaTime;

        var agent = FindObjectOfType<KartAgent>();

        if (agent != null)
        {
            agent.AddReward(-1f);
            agent.EndEpisode();
        }
        else
        {
            ResetCheckpoints();
        }
    }

    public void CheckPointReached(Checkpoint checkpoint)
    {
        if (nextCheckPointToReach != checkpoint) return;

        lastCheckpoint = Checkpoints[CurrentCheckpointIndex];
        reachedCheckpoint?.Invoke(checkpoint);
        CurrentCheckpointIndex++;

        var kartAgent = FindObjectOfType<KartAgent>();

        if (CurrentCheckpointIndex >= Checkpoints.Count)
        {
            Debug.Log("Satu lap selesai!");

            if (kartAgent != null)
            {
                kartAgent.AddReward(0.5f);
                kartAgent.EndEpisode();
            }
            else
            {
                ResetCheckpoints();
            }

            if (cameraManager != null)
            {
                cameraManager.SwitchToHighlightCamera();
            }
        }
        else
        {
            if (kartAgent != null)
            {
                kartAgent.AddReward((0.5f) / Checkpoints.Count);
            }
            SetNextCheckpoint();
        }
    }

    private void SetNextCheckpoint()
    {
        if (Checkpoints.Count > 0 && CurrentCheckpointIndex < Checkpoints.Count)
        {
            TimeLeft = MaxTimeToReachNextCheckpoint;
            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];
        }
        else if (Checkpoints.Count > 0)
        {
            CurrentCheckpointIndex = 0;
            nextCheckPointToReach = Checkpoints[CurrentCheckpointIndex];
        }
    }
}