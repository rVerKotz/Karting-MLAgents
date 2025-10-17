using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    public List<Checkpoint> checkPoints;
    
    private void Awake()
    {
        checkPoints = new List<Checkpoint>(GetComponentsInChildren<Checkpoint>());
        for (int i = 0; i < checkPoints.Count; i++)
        {
            checkPoints[i].checkpointIndex = i;
        }
    }
}
