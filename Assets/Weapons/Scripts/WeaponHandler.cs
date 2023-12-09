using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    //This script is used to handle the weapons by attaching the neccessary scripts to the weapon gameobject
    //This files depend on if the user has a the weapon or if an enemy has the weapon

    private GameObject owner;
    private EnemyShoot enemyShoot;
    private EnemyGunMovement enemyGunMovement;
    private GunMovement gunMovement;
    private AdvancedBulletEmmiter playerShoot;
    private WeaponStats weaponStats;
    private ParticleSystem system;

    // Start is called before the first frame update
    void Start()
    {
        ChangeOwner();
    }
    public void ChangeOwner()
    {
        enemyShoot = GetComponentInChildren<EnemyShoot>();
        enemyGunMovement = GetComponent<EnemyGunMovement>();
        gunMovement = GetComponent<GunMovement>();
        playerShoot = GetComponentInChildren<AdvancedBulletEmmiter>();
        weaponStats = GetComponentInChildren<WeaponStats>();
        system = weaponStats.system;
        
        //If it has a parent
        if(transform.parent != null)
        {
            owner = transform.parent.gameObject;
            if (owner.tag == "Enemy")
            {
                playerShoot.enabled = false;
                enemyShoot.enabled = true;
                enemyGunMovement.enabled = true;
                gunMovement.enabled = false;
                //Set the bullet material to the enemy material
                system.GetComponent<ParticleSystemRenderer>().material = weaponStats.enemyBulletMaterial;
                //Set the correct collision layers in the particle system. In this case with CollidableWall and Player
                var collision = system.collision;
                collision.collidesWith = LayerMask.GetMask("CollidableWall", "Player");

            
            }
            else if(owner.tag == "Character")
            {
                enemyShoot.enabled = false;
                enemyGunMovement.enabled = false;
                gunMovement.enabled = true;
                playerShoot.enabled = true;
                //Set the bullet material to the player material
                system.GetComponent<ParticleSystemRenderer>().material = weaponStats.playerBulletMaterial;
                //Set the correct collision layers in the particle system. In this case with CollidableWall and Enemy
                var collision = system.collision;
                collision.collidesWith = LayerMask.GetMask("CollidableWall", "Enemy");
            }
        }       
        else
        {
            enemyShoot.enabled = false;
            enemyGunMovement.enabled = false;
            gunMovement.enabled = false;
            playerShoot.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
