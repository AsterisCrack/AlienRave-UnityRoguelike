using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    //This script is used to set up the player's weapon.
    [Header ("Sprite")]
    [SerializeField] private Sprite gunSprite;
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 rotationOffset;

    [Header ("Stats")]
    [SerializeField] private float fireRate;
    [SerializeField] private enum FireMode { Automatic, SemiAutomatic, Burst };
    [SerializeField] private float damage;
    [SerializeField] private float range;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletSize;
    [SerializeField] private float bulletLifeTime;
    [SerializeField] private float bulletSpread;

    [Header ("Ammo")]
    [SerializeField] private int ammo;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float reloadTime;
    [SerializeField] private float reloadDelay;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
