using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthHandler : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private PickaableGun pickableGun;
    private GameObject gun;
    private int currentHealth;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        pickableGun = GetComponentInChildren<PickaableGun>();
        gun = pickableGun.gameObject;
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("CharacterBullet"))
        {
            float damage = other.GetComponent<WeaponStats>().damage;
            float knockback = other.GetComponent<WeaponStats>().knockback;
            Vector2 direction = other.GetComponent<WeaponStats>().system.transform.right;
            TakeDamage(damage, knockback, direction);
        }
    }

    private void TakeDamage(float damage, float knockback, Vector2 direction)
    {
        currentHealth -= (int)damage;
        GetComponent<Rigidbody2D>().AddForce(direction * knockback, ForceMode2D.Impulse);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //Disable enemy shoot and enemy gun movement
        gun.GetComponentInChildren<EnemyShoot>().enabled = false;
        gun.GetComponent<EnemyGunMovement>().enabled = false;
        //Make gun drop
        pickableGun.Drop(false);
        Destroy(gameObject);
    }
}
