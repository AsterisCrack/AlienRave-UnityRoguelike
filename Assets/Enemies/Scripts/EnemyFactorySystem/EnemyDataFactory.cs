using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDataFactory : MonoBehaviour // va a colgar de un GameObject vacío que se llama Spawner
{
    private int maxHealth;
    private int currentHealth;
    private int speed;
    private int attackSpeed;
    private int attackRange;
    private float accuracy;

    public EnemyDataFactory()
    {
        // vamos a crear el objeto una única vez en todo el funcionamiento del programa, si se intenta instanciar otra vez,
        // debería devolver el objeto que ya existe (como un singleton)

        // vamos a dar provisionales valores a todos los atributos del enemigo
        // y luego los vamos a cambiar en función del tipo de enemigo que queramos crear
        maxHealth = 100;
        currentHealth = 100;
        speed = 4;
        attackSpeed = 1;
        attackRange = 1;
        accuracy = 0.9f;

    }

    public EnemyData CreateEnemyDataObject(int level, EnemyFactory.enemyType type)
    {
        // vamos a construir un objeto de la clase EnemyData con los datos que nos pasan y 
        // devolverlo con para que lo use EnemyFactory, donde haremos this.data = EnemyDataFactory.CreateEnemyDataObject(level, type);
    

        switch (type)
        {
            case EnemyFactory.enemyType.melee:
                // caso melee 
                maxHealth = 100;
                currentHealth = 100;
                speed = 4;
                attackSpeed = 1;
                attackRange = 1;
                accuracy = 0.9f;
                break;

            case EnemyFactory.enemyType.ranged:
                // caso ranged
                maxHealth = 60;
                currentHealth = 60;
                speed = 2;
                attackSpeed = 1;
                attackRange = 6;
                accuracy = 0.9f;
                break;

            case EnemyFactory.enemyType.boss:
                // caso boss
                maxHealth = 1000;
                currentHealth = 1000;
                speed = 2;
                attackSpeed = 1;
                attackRange = 1;
                accuracy = 0.9f;
                break;

            default:
                // caso por defecto, damos valores por defecto
                maxHealth = 100;
                currentHealth = 100;
                speed = 4;
                attackSpeed = 1;
                attackRange = 1;
                accuracy = 0.9f;
                break;
        }
        EnemyData enemyData = new EnemyData(maxHealth, currentHealth, speed, attackSpeed, attackRange, accuracy); 
        // aqui tenemos que pasarle todos y cada uno de los parametros que queremos que tenga el enemigo
        return enemyData;
    }
}

public class EnemyData
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] private int speed;
    [SerializeField] private int attackSpeed;
    [SerializeField] private int attackRange;
    [SerializeField] private float accuracy;
    // no vamos a tener en cuenta el daño del enemigo; vamos a suponer que 1 bala = 1 daño = 1 vida perdida

    public EnemyData(int maxHeath, int currentHealth, int speed, int attackSpeed, int attackRange, float accuracy)
    {
        // constructor vacío
        this.maxHealth = maxHeath;
        this.currentHealth = currentHealth;
        this.speed = speed;
        this.attackSpeed = attackSpeed;
        this.attackRange = attackRange;
        this.accuracy = accuracy;
    }

}
