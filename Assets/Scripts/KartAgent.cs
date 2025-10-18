using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class KartAgent : Agent
{
    // Pastikan referensi ini di-assign (bisa via Inspector atau GetComponent di Initialize)
    public CheckpointManager _checkpointManager;
    public KartController _kartController;

    // Reward tambahan tiap kali checkpoint tercapai (atur di Inspector)
    public float checkpointRewardValue = 0.2f;

    public float speedMultiplier = 0.25f;
    public float directionMultiplier = 0.5f; // Pastikan ini sudah dinaikkan
    public float timePenaltyPerStep = 0.001f;

    // public RayPerceptionSensorComponent3D frontRaySensor; // Untuk penalti tabrakan

    private Rigidbody SphereRigidbody
    {
        get
        {
            if (_kartController != null) return _kartController.sphere;
            // Coba cari jika null saat runtime
            if (_kartController == null) _kartController = GetComponentInParent<KartController>() ?? transform.root.GetComponentInChildren<KartController>();
            return _kartController?.sphere;
        }
    }

    public override void Initialize()
    {
        // Pastikan _kartController didapatkan
        if (_kartController == null) _kartController = GetComponentInParent<KartController>() ?? transform.root.GetComponentInChildren<KartController>();
        if (_kartController == null) Debug.LogError("KartAgent.Initialize: KartController tidak ditemukan!", gameObject);

        // Pastikan _checkpointManager didapatkan
        if (_checkpointManager == null) _checkpointManager = GetComponentInChildren<CheckpointManager>(); // Asumsi ada di child "Collider"
        if (_checkpointManager == null) Debug.LogError("KartAgent.Initialize: CheckpointManager tidak ditemukan!", gameObject);

        // Subscribe ke event checkpoint
        if (_checkpointManager != null)
        {
            _checkpointManager.reachedCheckpoint += OnReachedCheckpoint; // Gunakan handler yang sesuai signature
        }
        else
        {
            Debug.LogError($"[{_kartController?.name ?? gameObject.name}] KartAgent.Initialize: Gagal subscribe ke event reachedCheckpoint karena _checkpointManager null.");
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset state di RaceManager dulu
        if (RaceManager.Instance != null && _kartController != null)
        {
            RaceManager.Instance.ResetRacerState(_kartController);
        }
        // else { Debug.LogWarning($"[{_kartController?.name ?? gameObject.name}] KartAgent.OnEpisodeBegin: Gagal reset state di RaceManager."); } // Debug Log bisa dihapus

        // Baru reset CheckpointManager
        if (_checkpointManager != null)
        {
            _checkpointManager.ResetCheckpoints();
        }
        // else { Debug.LogError($"[{_kartController?.name ?? gameObject.name}] KartAgent.OnEpisodeBegin: _checkpointManager null, tidak bisa reset checkpoints."); } // Debug Log bisa dihapus

        if (_kartController != null)
        {
            _kartController.Respawn();
        }
        // else { Debug.LogError($"[{gameObject.name}] KartAgent.OnEpisodeBegin: _kartController null, tidak bisa respawn."); } // Debug Log bisa dihapus

        // Reset kecepatan
        Rigidbody rb = SphereRigidbody;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        // else { Debug.LogWarning($"[{_kartController?.name ?? gameObject.name}] KartAgent.OnEpisodeBegin: SphereRigidbody null saat reset kecepatan."); } // Debug Log bisa dihapus
    }

    // Handler untuk event reachedCheckpoint (signature Action<Checkpoint>)
    private void OnReachedCheckpoint(Checkpoint checkpoint)
    {
        AddReward(checkpointRewardValue);
        // Debug.Log($"[{_kartController?.name ?? gameObject.name}] KartAgent.OnReachedCheckpoint: Reward {checkpointRewardValue} diberikan untuk {checkpoint.name}."); // Debug Log bisa dihapus
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Transform nextCpTransform = _checkpointManager?.GetNextCheckpointTransform();

        if (nextCpTransform == null)
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(0f);
            AddReward(-timePenaltyPerStep);
            return;
        }

        Vector3 diff = nextCpTransform.position - transform.position;
        Vector3 dirToNext = diff.normalized;
        Vector3 forward = transform.forward;
        Rigidbody rb = SphereRigidbody;
        Vector3 localVelocity = Vector3.zero;
        if (rb != null)
        {
            localVelocity = transform.InverseTransformDirection(rb.velocity);
        }
        float alignment = Vector3.Dot(forward, dirToNext);

        sensor.AddObservation(dirToNext);
        sensor.AddObservation(localVelocity / 20f);
        sensor.AddObservation(forward);
        sensor.AddObservation(alignment);

        AddReward(-timePenaltyPerStep);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (_kartController == null)
        {
            // Debug.LogError($"[{gameObject.name}] OnActionReceived: _kartController null. Mengabaikan aksi."); // Debug Log bisa dihapus
            return;
        }

        var input = actions.ContinuousActions;
        float steer = Mathf.Clamp(input[0], -1f, 1f);
        float accel = Mathf.Clamp(input[1], 0f, 1f);

        _kartController.Steer(steer);
        _kartController.ApplyAcceleration(accel);

        Transform nextCpTransform = _checkpointManager?.GetNextCheckpointTransform();
        if (nextCpTransform != null)
        {
            Vector3 diff = nextCpTransform.position - transform.position;
            Vector3 dirToNext = diff.normalized;
            float alignment = Vector3.Dot(transform.forward, dirToNext);

            AddReward(alignment * directionMultiplier); // Reward arah

            Rigidbody rb = SphereRigidbody;
            if (rb != null)
            {
                float speed = Vector3.Dot(rb.velocity, transform.forward);
                AddReward(speed * speedMultiplier); // Reward kecepatan
            }

            if (alignment < -0.2f)
            {
                AddReward(-0.2f); // Penalti arah salah
            }
        }

        Rigidbody rbSlow = SphereRigidbody;
        if (rbSlow != null && rbSlow.velocity.magnitude < 1f)
        {
            AddReward(-0.05f); // Penalti macet
        }

        // --- Logika Penalti Hampir Tabrakan ---
        // ... (kode penalti raycast jika Anda gunakan) ...
        // ---
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Tetap nonaktifkan untuk training
        /* var action = actionsOut.ContinuousActions;
         action[0] = Input.GetAxis("Horizontal");
         action[1] = Input.GetKey(KeyCode.W) ? 1f : 0f;*/
    }

    private void OnDestroy()
    {
        // Unsubscribe dari event
        if (_checkpointManager != null)
        {
            _checkpointManager.reachedCheckpoint -= OnReachedCheckpoint;
        }
    }
}