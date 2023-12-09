using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BurstShoot : ShootScript
{
    // Update is called once per frame
    protected override void Update()
    {
        //Call parent update
        base.Update();
        if (inMenu)
        {
            return;
        }
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

        if (burstDelay > 0)
        {
            burstDelay -= Time.deltaTime;
            if (burstDelay <= 0)
            {
                canShoot = true;
            }
        }
    }
    protected override void Shoot()
    {
        StartCoroutine(ShootBurst());
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

    protected override IEnumerator Reload()
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
