using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using Cinemachine;

public class KartController : MonoBehaviour
{
   private SpawnPointManager _spawnPointManager;
   
   public Transform kartModel;
   public Transform kartNormal;
   public Rigidbody sphere;
   
   float speed, currentSpeed;
   float rotate, currentRotate;

   [Header("Parameters")]
   public float acceleration = 30f;
   public float steering = 80f;
   public float gravity = 10f;
   public LayerMask layerMask;

   [Header("Model Parts")]
   public Transform frontWheels;
   public Transform backWheels;
   public Transform steeringWheel;

   public void Awake()
   {
      _spawnPointManager = FindObjectOfType<SpawnPointManager>();
        if (_spawnPointManager == null)
        {
            Debug.LogWarning("SpawnPointManager tidak ditemukan saat Awake. Akan dicoba lagi di Respawn jika diperlukan.");
        }

        if (sphere == null)
        {
            Transform sphereTransform = transform.Find("SphereCollider");
            if (sphereTransform != null)
            {
                sphere = sphereTransform.GetComponent<Rigidbody>();
            }
            if (sphere == null)
            {
                sphere = GetComponentInChildren<Rigidbody>();
            }
            if (sphere == null)
            {
                Debug.LogError("Rigidbody 'sphere' tidak ditemukan!");
            }
        }
    }

   public void ApplyAcceleration(float input)
   {
      speed = acceleration * input;
      currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 12f);
      speed = 0f;
      currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
      rotate = 0f;
   }

   public void AnimateKart(float input)
   {
      kartModel.localEulerAngles = Vector3.Lerp(kartModel.localEulerAngles, new Vector3(0, 90 + (input * 15), kartModel.localEulerAngles.z), .2f);
      
      frontWheels.localEulerAngles = new Vector3(0, (input * 15), frontWheels.localEulerAngles.z);
      frontWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 2);
      backWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 2);
      
      steeringWheel.localEulerAngles = new Vector3(-25, 90, ((input * 45)));
   }
   
   public void Respawn()
   {
      if(_spawnPointManager == null ) _spawnPointManager = FindObjectOfType<SpawnPointManager>();
      Vector3 pos = _spawnPointManager.SelectRandomSpawnpoint();
      sphere.MovePosition(pos);
      transform.position = pos - new Vector3(0, 0.4f, 0);
   }

    public void FixedUpdate()
    {
        if (kartModel == null || sphere == null || kartNormal == null) return;

        sphere.AddForce(-kartModel.transform.right * currentSpeed, ForceMode.Acceleration);
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        transform.position = sphere.transform.position - new Vector3(0, 0.4f, 0);
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);

        RaycastHit hitOn, hitNear;
        bool didHitOn = Physics.Raycast(transform.position + (transform.up * 0.1f), Vector3.down, out hitOn, 1.1f, layerMask);
        bool didHitNear = Physics.Raycast(transform.position + (transform.up * 0.1f), Vector3.down, out hitNear, 2.0f, layerMask);

        if (didHitNear)
        {
            kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
            kartNormal.Rotate(0, transform.eulerAngles.y, 0);
        }
    }

    public void Steer(float steeringSignal)
   {
      int steerDirection = steeringSignal > 0 ? 1 : -1;
      float steeringStrength = Mathf.Abs(steeringSignal);
      
      rotate = (steering * steerDirection) * steeringStrength;
   }

}
