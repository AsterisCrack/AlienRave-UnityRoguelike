using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyShoot : PlayerLocator
{
    //Needed variables
    [Header("Weapon Stats")]
    [SerializeField] WeaponStats weaponStats;

    [Header("Enemy options")]
    [SerializeField] float minshootDistance = 10f;
    [SerializeField] float bulletSpeed = 5f;
    [SerializeField] float timeBetweenShots = 0.5f;

    private WeaponStats.WeaponType weaponType;
    private ParticleSystem system;
    private float reloadTime;
    private float fireRate;
    private float damage;
    private float range;
    private float accuracy;
    private int burstCount;
    private float burstDelay;
    private float shakeTime; 
    private float shakeMagnitude;
    //Enemies have infinite ammo
    private int clipSize;

    private AudioClip shootSound;

    private int currentAmmo = -1;
    private int currentClip = -1;

    //Camera shake
    private CameraShake cameraShake;

    //Neccessary variables
    private float reloadTimer;
    private float shootTimer;

    // Start is called before the first frame update
    void Awake()
    {
        FindPlayer();
        //If no weapon stats are set, set the Script in this object
        if (!weaponStats)
        {
            weaponStats = GetComponent<WeaponStats>();
        }
        //Set the weapon stats
        system = weaponStats.system;
        weaponType = weaponStats.weaponType;
        reloadTime = timeBetweenShots;
        fireRate = weaponStats.fireRate;
        damage = weaponStats.damage;
        range = weaponStats.range;
        accuracy = weaponStats.accuracy;
        burstCount = weaponStats.burstCount;
        burstDelay = weaponStats.burstDelay;
        shakeTime = weaponStats.shakeTime;
        shakeMagnitude = weaponStats.shakeMagnitude;
        clipSize = weaponStats.clipSize;
        shootSound = weaponStats.shootSound;

        //Set the ammo counters according to the 
        switch (weaponType)
        {
            case WeaponStats.WeaponType.SemiAutomatic:
                clipSize = 1;
                break;
            case WeaponStats.WeaponType.Automatic:
                break;
            case WeaponStats.WeaponType.Burst:
                clipSize = burstCount;
                break;
            default:
                clipSize = 1;
                break;
        }

        //Set particle system particle speed to bullet speed
        var main = system.main;
        main.startSpeed = bulletSpeed;
    }
    void Start()
    {
        reloadTimer = timeBetweenShots;
        shootTimer = 0;
        system = GetComponent<ParticleSystem>();
        //Set the ammo counters
        currentClip = clipSize;

        //Object initialization
        cameraShake = CameraShake.instance;
    }

    private void OnEnable()
    {
        currentClip = clipSize;
    }

    public void Shoot()
    {
        shootTimer = fireRate;
        //Update ammo counters
        currentAmmo--;
        currentClip--;
        //Play the particle system
        system.Play();
        //Play the shoot sound
        GameAudioManager.instance.PlaySound(shootSound);
        //Shake the camera
        cameraShake.ShakeCamera(shakeTime, shakeMagnitude);
        if (currentClip <= 0)
        {
            //Reload if the clip is empty
            StartCoroutine(Reload());
        }
    }
    private IEnumerator Reload()
    {
        //Wait for the reload time
        yield return new WaitForSeconds(reloadTime);
        //Stop reloading
        //Reset the clip size
        currentClip = clipSize;
    }

    public bool CanShoot()
    {
        //Check if the enemy can shoot
        if (currentClip > 0 && IsPlayerInSight() && GetDistanceToPlayer() < minshootDistance && IsPlayerAlive())
        {
            return true;
        }
        return false;
    }

    public void ShootingBehaviour()
    {
        reloadTimer -= Time.deltaTime;
        shootTimer -= Time.deltaTime;
        //The enemy tries to shoot if they can
        if (CanShoot() && reloadTimer <= 0 && shootTimer <= 0)
        {
            Shoot();
        }
    }

    // Update is called once per frame
    void Update()
    {
        ShootingBehaviour();
    }
}
