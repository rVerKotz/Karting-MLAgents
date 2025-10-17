using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private KartController _kartController;

    private void Awake()
    {
        _kartController = GetComponent<KartController>();
    }

    private void Update()
    {
        // Mengontrol setir berdasarkan input horizontal
        float steerInput = Input.GetAxis("Horizontal");
        _kartController.Steer(steerInput);

        // Mengontrol akselerasi berdasarkan input vertikal
        float accelerateInput = Input.GetKey(KeyCode.W) ? 1f : 0f;
        _kartController.ApplyAcceleration(accelerateInput);

        // Menerapkan animasi pada kart
        _kartController.AnimateKart(steerInput);
    }
}