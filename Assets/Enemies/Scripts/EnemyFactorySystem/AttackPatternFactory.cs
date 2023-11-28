using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPatternFactory : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public AttackPatternFactory()
    {
        // empty constructor

    }

    public AttackPattern GenerateAttackPattern(int level, EnemyFactory.enemyType type)
    {
        // genera el patron de ataque con los datos que le pasamos
        // y lo devuelve para que lo use el SpawnManager

        // GameObject enemy = new GameObject();
        // enemy.AddComponent<Enemy>();
        // enemy.AddComponent<EnemyData>();
        // enemy.AddComponent<WeaponData>();
        // enemy.AddComponent<EnemyPatternData>();

        AttackPattern attackPattern = new AttackPattern();
        // hacemos cosas para crear bien el objeto attackPattern
        return attackPattern;

    }
}

public class AttackPattern
{
    public int level;
    public int damage;
    public float accuracy;

    public AttackPattern()
    {
        
    }
}