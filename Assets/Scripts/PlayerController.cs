using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private KartController _kartController;
    private RaceManager _raceManager;

    private void Awake()
    {
        _kartController = GetComponent<KartController>();
        _raceManager = RaceManager.Instance;
    }

    private void Update()
    {
        float steerInput = Input.GetAxis("Horizontal");

        float accelerateInput = Input.GetKey(KeyCode.W) ? 1f : 0f;

        if (steerInput != 0f || accelerateInput != 0f)
        {
            if (_raceManager != null)
            {
                _raceManager.NotifyPlayerInput(); 
            }
            else
            {
                _raceManager = RaceManager.Instance;
                _raceManager.NotifyPlayerInput();
            }
        }

        _kartController.Steer(steerInput);
        _kartController.ApplyAcceleration(accelerateInput);

        _kartController.AnimateKart(steerInput);
    }
}