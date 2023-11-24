using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AdvancedBulletEmmiter : MonoBehaviour
{
    [Header("Weapon Type")]
    [SerializeField] ParticleSystem system;
    //ennumerator on weapon type: automatic, semi-automatic, burst, etc.
    private enum WeaponType { Automatic, SemiAutomatic, Burst };
    [SerializeField] private WeaponType weaponType;

    [Header("Characteristics")]
    [SerializeField] private float reloadTime;
    [SerializeField] private float fireRate;
    [SerializeField] private float damage;
    [SerializeField] private float range;
    [SerializeField] private float accuracy;
    [SerializeField] private int burstCount;
    [SerializeField] private float burstDelay;
    [SerializeField] private float shakeTime;
    [SerializeField] private float shakeMagnitude;

    [SerializeField] private AdvancedBulletEmmiter bulletEmitter;


    public float ReloadTime { get => reloadTime; set => reloadTime = value; }
    public float FireRate { get => fireRate; set => fireRate = value; }
    public float Damage { get => damage; set => damage = value; }
    public float Range { get => range; set => range = value; }
    public float Accuracy { get => accuracy; set => accuracy = value; }
    public int BurstCount { get => burstCount; set => burstCount = value; }
    public float BurstDelay { get => burstDelay; set => burstDelay = value; }
    public float ShakeTime { get => shakeTime; set => shakeTime = value; }
    public float ShakeMagnitude { get => shakeMagnitude; set => shakeMagnitude = value; }


    [Header("Ammo")]
    [SerializeField] private int totalAmmo;
    [SerializeField] private int clipSize;
    public int TotalAmmo { get => totalAmmo; set => totalAmmo = value; }
    public int ClipSize { get => clipSize; set => clipSize = value; }

    private int currentAmmo = -1;
    private int currentClip = -1;
    public int CurrentAmmo { get => currentAmmo; set => currentAmmo = value; }
    public int CurrentClip { get => currentClip; set => currentClip = value; }

    //Instances of objects needed
    //Ammo counter UI
    private UIAmmoCounter ammoCounter;
    //Camera shake
    private CameraShake cameraShake;

    //Inputs
    private PlayerInput playerInput;
    private InputAction shootAction;
    private InputAction reloadAction;
    public InputAction ShootAction { get => shootAction; set => shootAction = value; }
    public InputAction ReloadAction { get => reloadAction; set => reloadAction = value; }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        shootAction = playerInput.actions["Shoot"];
        reloadAction = playerInput.actions["Reload"];
    }

    private void SetShootScriptActive(bool active)
    {
        if (active)
        {   
            switch (weaponType)
            {
                case WeaponType.Automatic:
                    gameObject.AddComponent<AutoShoot>();
                    break;
                case WeaponType.SemiAutomatic:
                    gameObject.AddComponent<SemiAutoShoot>();
                    break;
                case WeaponType.Burst:
                    gameObject.AddComponent<BurstShoot>();
                    break;
                default:
                    break;
            }
        }
        else
        {
            //If it has no shooting script, exit the function
            if (!gameObject.GetComponent<AutoShoot>() && !gameObject.GetComponent<SemiAutoShoot>() && !gameObject.GetComponent<BurstShoot>())
            {
                return;
            }
            switch (weaponType)
            {
                case WeaponType.Automatic:
                    Destroy(gameObject.GetComponent<AutoShoot>());
                    break;
                case WeaponType.SemiAutomatic:
                    Destroy(gameObject.GetComponent<SemiAutoShoot>());
                    break;
                case WeaponType.Burst:
                    Destroy(gameObject.GetComponent<BurstShoot>());
                    break;
                default:
                    break;
            }
        }
    }

    private void OnEnable()
    {
        SetShootScriptActive(true);
        ammoCounter = UIAmmoCounter.instance;
        if (currentAmmo == -1) currentAmmo = totalAmmo;
        if (currentClip == -1) currentClip = clipSize;
        ammoCounter.SetAmmoCounter(currentAmmo);
        ammoCounter.SetClipCounter(currentClip);
    }

    private void OnDisable()
    {
           SetShootScriptActive(false);
    }

    //At start, attach the appropiate shooting script
    void Start()
    {
        system = GetComponent<ParticleSystem>();
        //Set the ammo counters
        currentAmmo = totalAmmo;
        currentClip = clipSize;

        //Object initialization
        ammoCounter = UIAmmoCounter.instance;
        ammoCounter.SetAmmoCounter(totalAmmo);
        ammoCounter.SetClipCounter(clipSize);
        cameraShake = CameraShake.instance;
    }

    public void Shoot()
    {
        //Update ammo counters
        currentAmmo--;
        currentClip--;
        ammoCounter.SetAmmoCounter(currentAmmo);
        ammoCounter.SetClipCounter(currentClip);
        //Play the particle system
        system.Play();
        //Shake the camera
        cameraShake.ShakeCamera(shakeTime, shakeMagnitude);
    }

    public void StartReload()
    {
        
    }

    public void Reload()
    {
        //Reset the clip size
        currentClip = Mathf.Min(clipSize, currentAmmo);
        //Update the ammo counter
        ammoCounter.SetClipCounter(currentClip);
    }
}
