using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Tooltip("GameObject yang memiliki komponen ThirdPersonCamera.")]
    public GameObject playerCamera;

    [Tooltip("GameObject yang memiliki komponen AutomaticCameraSystem.")]
    public GameObject highlightCamera;

    void Start()
    {
        // Pastikan saat balapan dimulai, kamera pemain yang aktif.
        SwitchToPlayerCamera();
    }

    public void SwitchToPlayerCamera()
    {
        playerCamera.SetActive(true);
        highlightCamera.SetActive(false);
    }

    public void SwitchToHighlightCamera()
    {
        playerCamera.SetActive(false);
        highlightCamera.SetActive(true);
    }
}