using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthHandler : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private AnimationClip deadAnimation;
    private int currentHealth; public int CurrentHealth { get { return currentHealth; } }
    private HeartUIHandler heartUIHandler;
    private PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        heartUIHandler = HeartUIHandler.instance;
        playerMovement = GetComponent<PlayerMovement>();
        currentHealth = maxHealth;
        heartUIHandler.SetHearts(currentHealth);
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
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
            TakeDamage(knockback, direction);
        }
    }

    private void TakeDamage(float knockback, Vector2 direction)
    {
        bool healthDecreased = heartUIHandler.LowerHealth();
        if (healthDecreased)
        {
            currentHealth--;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(direction.normalized * knockback, ForceMode2D.Impulse); // Normalize direction vector
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        gameObject.GetComponent<PlayerMovement>().enabled = false;
        gameObject.GetComponent<GunInventory>().enabled = false;
        //Disable children
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        //Play dead animation withouth looping
        Animator animator = GetComponent<Animator>();
        animator.SetBool("IsAlive", false);
        //animator.Play(deadAnimation.name);
        StartCoroutine(enterDeathMenu());
    }

    private IEnumerator enterDeathMenu()
    {
        yield return new WaitForSeconds(1);
        EnterMenu.instance.ToggleMenu("You Died!", false);
        //Disable this script
        enabled = false;
    }
}
