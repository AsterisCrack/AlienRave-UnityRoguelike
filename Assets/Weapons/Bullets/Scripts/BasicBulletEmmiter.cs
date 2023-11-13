using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicBulletEmmiter : MonoBehaviour
{
    [SerializeField] ParticleSystem system;
    [SerializeField] float shakeTime;
    [SerializeField] float shakeMagnitude;
    private CameraShake cameraShake;
    private PlayerInput playerInput;
    private InputAction shootAction;
    private InputAction reloadAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        shootAction = playerInput.actions["Shoot"];
        reloadAction = playerInput.actions["Reload"];
    }
    // Start is called before the first frame update
    void Start()
    {
        system = GetComponent<ParticleSystem>();
        cameraShake = CameraShake.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (shootAction.WasPressedThisFrame())
        {
            cameraShake.ShakeCamera(shakeTime, shakeMagnitude);
            system.Play();
        }
    }
}
