using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    //This script is used to handle the weapons by attaching the neccessary scripts to the weapon gameobject
    //This files depend on if the user has a the weapon or if an enemy has the weapon

    private GameObject owner;
    private PickaableGun pickaableGun;
    private EnemyShoot enemyShoot;
    private EnemyGunMovement enemyGunMovement;
    private GunMovement gunMovement;
    private AdvancedBulletEmmiter playerShoot;
    private BoxCollider2D boxCollider2D;

    // Start is called before the first frame update
    void Start()
    {
        pickaableGun = GetComponent<PickaableGun>();
        enemyShoot = GetComponentInChildren<EnemyShoot>();
        enemyGunMovement = GetComponent<EnemyGunMovement>();
        gunMovement = GetComponent<GunMovement>();
        playerShoot = GetComponentInChildren<AdvancedBulletEmmiter>();
        
        
        owner = transform.parent.gameObject;
        if(owner.tag == "Enemy")
        {
            pickaableGun.enabled = false;
            playerShoot.enabled = false;
            enemyShoot.enabled = true;
            enemyGunMovement.enabled = true;
            gunMovement.enabled = false;
            
        }
        else
        {
            pickaableGun.enabled = true;
            enemyShoot.enabled = false;
            enemyGunMovement.enabled = false;
            gunMovement.enabled = true;
            playerShoot.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
