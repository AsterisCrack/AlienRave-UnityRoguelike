using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthHandler : EnemyHealthHandler
{
    [SerializeField] private GameObject healthBar;
    private HealthBar healthBarScript;

    protected override void Start()
    {
        base.Start();
        if (healthBar == null)
        {
            //Find health bar
            healthBar = GameObject.FindGameObjectWithTag("BossHealthBar");
        }
        healthBarScript = healthBar.GetComponent<HealthBar>();
        healthBarScript.SetActive(true);
        healthBarScript.SetMaxHealth(maxHealth);
        healthBarScript.SetMinHealth(0);
        healthBarScript.SetHealth(maxHealth);
    }

    protected override void TakeDamage(float damage, float knockback, Vector2 direction)
    {
        base.TakeDamage(damage, knockback, direction);
        healthBarScript.SetHealth(currentHealth);
    }
}
