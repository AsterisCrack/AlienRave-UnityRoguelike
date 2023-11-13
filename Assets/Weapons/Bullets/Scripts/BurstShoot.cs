using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BurstShoot : MonoBehaviour
{
    public AdvancedBulletEmmiter emmiterSettings;

    //Needed variables
    private bool isReloading;
    private int currentAmmo;
    private int currentClip;
    private int burstCount;
    private float burstDelay;
    private bool canShoot;
    private float reloadTime;
    private InputAction shootAction;
    private InputAction reloadAction;


    // Start is called before the first frame update
    void Start()
    {
        emmiterSettings = GetComponent<AdvancedBulletEmmiter>();
        isReloading = false;
        currentAmmo = emmiterSettings.TotalAmmo;
        currentClip = emmiterSettings.ClipSize;
        burstCount = emmiterSettings.BurstCount;
        burstDelay = emmiterSettings.BurstDelay;
        canShoot = true;
        reloadTime = emmiterSettings.ReloadTime;
        shootAction = emmiterSettings.ShootAction;
        reloadAction = emmiterSettings.ReloadAction;
    }

    // Update is called once per frame
    void Update()
    {
        if (shootAction.WasPressedThisFrame() && canShoot && !isReloading)
        {
            if (currentClip > 0)
            {
                StartCoroutine(ShootBurst());
            }
            else
            {
                StartCoroutine(Reload());
            }
        }

        if (burstDelay > 0)
        {
            burstDelay -= Time.deltaTime;
            if (burstDelay <= 0)
            {
                canShoot = true;
            }
        }
    }

    private IEnumerator ShootBurst()
    {
        canShoot = false;
        int clip = currentClip;
        for (int i = 0; i < Mathf.Min(burstCount, clip); i++)
        {
            emmiterSettings.Shoot();
            currentClip = emmiterSettings.CurrentClip;
            currentAmmo = emmiterSettings.CurrentAmmo;
            yield return new WaitForSeconds(emmiterSettings.FireRate);
        }
        burstDelay = emmiterSettings.BurstDelay;
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
