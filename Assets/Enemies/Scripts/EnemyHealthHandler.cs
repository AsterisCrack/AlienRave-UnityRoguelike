using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthHandler : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 100;
    private PickaableGun pickableGun;
    private GameObject gun;
    protected int currentHealth;
    // Start is called before the first frame update
    protected virtual void Start()
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
            Vector2 direction = other.transform.forward;

            //destroy bullet
            ParticleSystem ps = other.GetComponent<ParticleSystem>();
            var particles = new ParticleSystem.Particle[ps.particleCount];
            ps.GetParticles(particles);
            int closestParticle = 0;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < particles.Length; i++)
            {
                float distance = Vector2.Distance(particles[i].position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestParticle = i;
                }
            }
            particles[closestParticle].remainingLifetime = 0;
            ps.SetParticles(particles);

            TakeDamage(damage, knockback, direction);
        }
    }

    protected virtual void TakeDamage(float damage, float knockback, Vector2 direction)
    {
        currentHealth -= (int)damage;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(direction.normalized * knockback, ForceMode2D.Impulse); // Normalize direction vector
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
