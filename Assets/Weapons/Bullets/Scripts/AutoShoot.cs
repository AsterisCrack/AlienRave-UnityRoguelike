using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoShoot : ShootScript
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
        if (shootAction.IsPressed() && canShoot && !isReloading && !playerMovement.IsDashing)
        {
            if (currentClip > 0)
            {
                Shoot();
            }
        }

        else if (reloadAction.WasPerformedThisFrame() && canShoot && !isReloading && !playerMovement.IsDashing)
        {
            if (currentClip < emmiterSettings.ClipSize)
            {
                StartCoroutine(Reload());
            }
        }

        if (shootAction.WasReleasedThisFrame() && canShoot && !isReloading && !playerMovement.IsDashing)
        {
            if (currentClip <= 0)
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

    protected override void Shoot()
    {
        emmiterSettings.Shoot();
        currentClip = emmiterSettings.CurrentClip;
        currentAmmo = emmiterSettings.CurrentAmmo;

        cooldownTimer = emmiterSettings.FireRate;
        canShoot = false;
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
