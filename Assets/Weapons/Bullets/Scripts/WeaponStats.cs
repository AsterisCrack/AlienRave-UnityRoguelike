using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    [Header("Weapon Type")]
    [SerializeField] public ParticleSystem system;
    //ennumerator on weapon type: automatic, semi-automatic, burst, etc.
    public enum WeaponType { Automatic, SemiAutomatic, Burst };
    [SerializeField] public WeaponType weaponType;

    [Header("Characteristics")]
    [SerializeField] public float reloadTime;
    [SerializeField] public float fireRate;
    [SerializeField] public float bulletSpeed;
    [SerializeField] public float damage;
    [SerializeField] public float knockback;
    [SerializeField] public float range;
    [SerializeField] public float accuracy;
    [SerializeField] public int burstCount;
    [SerializeField] public float burstDelay;
    [SerializeField] public float shakeTime;
    [SerializeField] public float shakeMagnitude;

    [Header("Ammo")]
    [SerializeField] public int totalAmmo;
    [SerializeField] public int clipSize;

    [Header("Bullet")]
    [SerializeField] public Material enemyBulletMaterial;
    [SerializeField] public Material playerBulletMaterial;
    [SerializeField] public AudioClip shootSound;
}
