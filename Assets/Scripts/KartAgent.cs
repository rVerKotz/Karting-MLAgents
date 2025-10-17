using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class KartAgent : Agent
{
    public CheckpointManager _checkpointManager;
    private KartController _kartController;

    // Reward tambahan tiap kali checkpoint tercapai
    public float checkpointReward = 1.0f;

    public float speedMultiplier = 0.001f;
    public float directionMultiplier = 0.01f;

    public float timePenaltyPerStep = 0.001f;

    // Untuk menjaga supaya tidak menggantung ketika tidak ada Rigidbody langsung
    private Rigidbody SphereRigidbody
    {
        get
        {
            if (_kartController != null)
            {
                return _kartController.sphere;
            }
            return null;
        }
    }

    public override void Initialize()
    {
        _kartController = GetComponent<KartController>();

        // subscribe ke event checkpoint
        if (_checkpointManager != null)
        {
            _checkpointManager.reachedCheckpoint += OnReachedCheckpoint;
        }
    }

    public override void OnEpisodeBegin()
    {
        _checkpointManager.ResetCheckpoints();
        _kartController.Respawn();

        // Reset kecepatan di sphere Rigidbody
        Rigidbody rb = SphereRigidbody;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnReachedCheckpoint(Checkpoint checkpoint)
    {
        AddReward(checkpointReward);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_checkpointManager == null || _checkpointManager.nextCheckPointToReach == null)
        {
            // Tambahkan observasi default jika belum di-setup
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.forward);
            sensor.AddObservation(0f);
            return;
        }

        Vector3 diff = _checkpointManager.nextCheckPointToReach.transform.position - transform.position;
        Vector3 dirToNext = diff.normalized;

        Vector3 forward = transform.forward;

        Rigidbody rb = SphereRigidbody;
        Vector3 localVelocity = Vector3.zero;
        if (rb != null)
        {
            localVelocity = transform.InverseTransformDirection(rb.velocity);
        }

        float alignment = Vector3.Dot(forward, dirToNext);

        sensor.AddObservation(dirToNext);                         // 3 floats
        sensor.AddObservation(localVelocity / 20f);               // 3 floats
        sensor.AddObservation(forward);                           // 3 floats
        sensor.AddObservation(alignment);                         // 1 float

        AddReward(-timePenaltyPerStep);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var input = actions.ContinuousActions;
        float steer = Mathf.Clamp(input[0], -1f, 1f);
        float accel = Mathf.Clamp(input[1], 0f, 1f);

        _kartController.Steer(steer);
        _kartController.ApplyAcceleration(accel);

        if (_checkpointManager != null && _checkpointManager.nextCheckPointToReach != null)
        {
            Vector3 diff = _checkpointManager.nextCheckPointToReach.transform.position - transform.position;
            Vector3 dirToNext = diff.normalized;
            float alignment = Vector3.Dot(transform.forward, dirToNext);

            AddReward(alignment * directionMultiplier);

            Rigidbody rb = SphereRigidbody;
            if (rb != null)
            {
                float speed = Vector3.Dot(rb.velocity, transform.forward);
                AddReward(speed * speedMultiplier);
            }

            if (alignment < -0.2f)
            {
                AddReward(-0.05f);
            }
        }

        Rigidbody rbSlow = SphereRigidbody;
        if (rbSlow != null && rbSlow.velocity.magnitude < 1f)
        {
            AddReward(-0.002f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.ContinuousActions;
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetKey(KeyCode.W) ? 1f : 0f;
    }

    private void OnDestroy()
    {
        if (_checkpointManager != null)
        {
            _checkpointManager.reachedCheckpoint -= OnReachedCheckpoint;
        }
    }
}
