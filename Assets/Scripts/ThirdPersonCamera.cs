using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Tooltip("Target yang akan diikuti oleh kamera, yaitu PlayerKart.")]
    public Transform target;

    [Tooltip("Seberapa jauh kamera dari target.")]
    public float distance = 8.0f;

    [Tooltip("Seberapa tinggi kamera dari target.")]
    public float height = 3.0f;

    [Tooltip("Seberapa mulus kamera akan mengikuti rotasi target.")]
    public float rotationDamping = 3.0f;

    [Tooltip("Seberapa mulus kamera akan mengikuti ketinggian target.")]
    public float heightDamping = 2.0f;

    // LateUpdate dipanggil setelah semua pembaruan Update lainnya,
    // ideal untuk logika kamera.
    void LateUpdate()
    {
        // Keluar dari fungsi jika tidak ada target yang ditetapkan.
        if (!target)
            return;

        // Hitung rotasi dan ketinggian yang diinginkan.
        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;

        // Ambil rotasi dan ketinggian saat ini dari kamera.
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Haluskan transisi rotasi menggunakan LerpAngle.
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        // Haluskan transisi ketinggian.
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // Ubah sudut menjadi rotasi Quaternion.
        var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Atur posisi kamera di belakang target.
        transform.position = target.position;
        transform.position -= currentRotation * Vector3.forward * distance;

        // Atur ketinggian kamera.
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        // Pastikan kamera selalu melihat ke arah target.
        transform.LookAt(target);
    }
}
