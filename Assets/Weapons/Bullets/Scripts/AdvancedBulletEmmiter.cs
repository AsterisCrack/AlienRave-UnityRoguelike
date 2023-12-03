using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AdvancedBulletEmmiter : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] WeaponStats weaponStats;
    
    private WeaponStats.WeaponType weaponType;
    private ParticleSystem system;
    private float reloadTime; public float ReloadTime { get => reloadTime; set => reloadTime = value; }
    private float fireRate; public float FireRate { get => fireRate; set => fireRate = value; }
    private float damage; public float Damage { get => damage; set => damage = value; }
    private float range; public float Range { get => range; set => range = value; }
    private float accuracy; public float Accuracy { get => accuracy; set => accuracy = value; }
    private int burstCount; public int BurstCount { get => burstCount; set => burstCount = value; }
    private float burstDelay; public float BurstDelay { get => burstDelay; set => burstDelay = value; }
    private float shakeTime; public float ShakeTime { get => shakeTime; set => shakeTime = value; }
    private float shakeMagnitude; public float ShakeMagnitude { get => shakeMagnitude; set => shakeMagnitude = value; }
    private int totalAmmo; public int TotalAmmo { get => totalAmmo; set => totalAmmo = value; }
    private int clipSize; public int ClipSize { get => clipSize; set => clipSize = value; }

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
        //If no weapon stats are set, set the Script in this object
        if (!weaponStats)
        {
            weaponStats = GetComponent<WeaponStats>();
        }
        //Set the weapon stats
        system = weaponStats.system;
        weaponType = weaponStats.weaponType;
        reloadTime = weaponStats.reloadTime;
        fireRate = weaponStats.fireRate;
        damage = weaponStats.damage;
        range = weaponStats.range;
        accuracy = weaponStats.accuracy;
        burstCount = weaponStats.burstCount;
        burstDelay = weaponStats.burstDelay;
        shakeTime = weaponStats.shakeTime;
        shakeMagnitude = weaponStats.shakeMagnitude;
        totalAmmo = weaponStats.totalAmmo;
        clipSize = weaponStats.clipSize;

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
                case WeaponStats.WeaponType.Automatic:
                    gameObject.AddComponent<AutoShoot>();
                    break;
                case WeaponStats.WeaponType.SemiAutomatic:
                    gameObject.AddComponent<SemiAutoShoot>();
                    break;
                case WeaponStats.WeaponType.Burst:
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
                case WeaponStats.WeaponType.Automatic:
                    Destroy(gameObject.GetComponent<AutoShoot>());
                    break;
                case WeaponStats.WeaponType.SemiAutomatic:
                    Destroy(gameObject.GetComponent<SemiAutoShoot>());
                    break;
                case WeaponStats.WeaponType.Burst:
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
