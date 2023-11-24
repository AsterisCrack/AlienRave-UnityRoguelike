using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.InputSystem;

public class SemiAutoShoot : MonoBehaviour
{
    public AdvancedBulletEmmiter emmiterSettings;

    //Needed variables
    private float cooldownTimer;
    private bool isReloading;
    private int currentAmmo;
    private int currentClip;
    private bool canShoot;
    private float reloadTime;
    private InputAction shootAction;
    private InputAction reloadAction;
    private PlayerMovement playerMovement;

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
        shootAction = emmiterSettings.ShootAction;
        reloadAction = emmiterSettings.ReloadAction;
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shootAction.WasPressedThisFrame() && canShoot && !isReloading && !playerMovement.IsDashing)
        {
            if (currentClip > 0)
            {
                Shoot();
            }
            else
            {
                StartCoroutine(Reload());
            }
        }

        else if (reloadAction.WasPerformedThisFrame() && canShoot && !isReloading && !playerMovement.IsDashing)
        {
            if (currentClip < emmiterSettings.ClipSize)
            {
                StartCoroutine(Reload());
            }
        }
        
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canShoot = true;
            }
        }

    }

    private void Shoot()
    {
        emmiterSettings.Shoot();
        currentClip = emmiterSettings.CurrentClip;
        currentAmmo = emmiterSettings.CurrentAmmo;

        cooldownTimer = emmiterSettings.FireRate;
        canShoot = false;
    }

    private IEnumerator Reload()
    {
        if (currentAmmo <= 0 || currentClip >= emmiterSettings.ClipSize)
        {
            yield break;
        }
        isReloading = true;
        //Start reloading. This will play animations etc.
        emmiterSettings.StartReload();
        //Wait for the reload time
        yield return new WaitForSeconds(reloadTime);
        //Stop reloading. This will reload the magazine and play animations etc.
        emmiterSettings.Reload();
        currentClip = emmiterSettings.CurrentClip;
        currentAmmo = emmiterSettings.CurrentAmmo;
        isReloading = false;
    }
}
