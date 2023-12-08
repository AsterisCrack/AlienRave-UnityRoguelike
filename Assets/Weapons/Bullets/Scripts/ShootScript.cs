using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ShootScript : MonoBehaviour
{
    public AdvancedBulletEmmiter emmiterSettings;

    //Needed variables
    protected float cooldownTimer;
    protected bool isReloading;
    protected int currentAmmo;
    protected int currentClip;
    protected bool canShoot;
    protected float reloadTime;
    protected int burstCount;
    protected float burstDelay;
    protected InputAction shootAction;
    protected InputAction reloadAction;
    protected InputAction menuAction;
    protected PlayerMovement playerMovement;
    protected bool inMenu = false;


    // Start is called before the first frame update
    void Start()
    {
        emmiterSettings = GetComponent<AdvancedBulletEmmiter>();
        cooldownTimer = 0;
        isReloading = false;
        currentAmmo = emmiterSettings.CurrentAmmo;
        currentClip = emmiterSettings.CurrentClip;
        canShoot = true;
        reloadTime = emmiterSettings.ReloadTime;
        burstCount = emmiterSettings.BurstCount;
        burstDelay = emmiterSettings.BurstDelay;
        shootAction = emmiterSettings.ShootAction;
        reloadAction = emmiterSettings.ReloadAction;
        menuAction = emmiterSettings.MenuAction;
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (menuAction.WasPerformedThisFrame())
        {
            inMenu = !inMenu;
        }
    }

    protected abstract void Shoot();

    protected abstract IEnumerator Reload();
}
