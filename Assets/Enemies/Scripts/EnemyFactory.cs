using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * hacer un script EnemyFactory (que sea un using... EnemyFactory : MonoBehaviour {clase}) 
 * que me genere un enemigo con arma, datos y patron de ataque cada una de estas partes 
 * se delega en otro script, y yo le paso el tipo de ataque que quiero (por ejemplo, semiautomatico) 
 * y eso pir detras me configura todos los parametros de eso. Puedo fijar los valores del arma 
 * (ej daño, precision, alcance, etc) manualmente al crearlo.
 * 
 * ademas, tiene que haber un sprite (aka skin) asociado a cada arma/enemigo
 * 
 * va a ser de la forma GameObject.Componente.propiedad (ej, mario.arma.daño o mario.datos.vida)
 */

public class EnemyFactory : MonoBehaviour // va a colgar de un GameObject vacío que se llama Spawner. Será instanciado por un SpawnManager
{
    public enum enemyType { melee, ranged, boss}; // Crea el enum de enemy type
    [SerializeField] private enemyType type;

    private EnemyDataFactory enemyDataFactory;
    private WeaponFactory weaponFactory;
    private AttackPatternFactory attackPatternFactory;

    public EnemyFactory()
    {
        // generate the factories for each component and attatch them to the EnemyFactory
        this.enemyDataFactory = new EnemyDataFactory();
        this.weaponFactory = new WeaponFactory();
        this.attackPatternFactory = new AttackPatternFactory();
    }

    public GameObject GenerateEnemy(int level, EnemyFactory.enemyType type)
    {
        // genera el enemigo con los datos que le pasamos
        // y lo devuelve para que lo use el SpawnManager

        // GameObject enemy = new GameObject();
        // enemy.AddComponent<Enemy>();
        // enemy.AddComponent<EnemyData>();
        // enemy.AddComponent<WeaponData>();
        // enemy.AddComponent<EnemyPatternData>();

        EnemyData enemyData = enemyDataFactory.CreateEnemyDataObject(level, type); // genera el enemigo con los 
        WeaponData weaponData = weaponFactory.CreateWeaponDataObject(level, type);
        AttackPattern attackPattern = attackPatternFactory.GenerateAttackPattern(level, type);

        /*
         * ERROR: The type 'Enemy' cannot be used as type parameter 'T' in the generic type or method 'GameObject.AddComponent<T>()'. 
         * There is no implicit reference conversion from 'Enemy' to 'UnityEngine.Component'.
         * 
         * SOLUCION: https://answers.unity.com/questions/1410701/the-type-cannot-be-used-as-type-parameter-t-in-the.html
         */
        
        GameObject enemy = new GameObject();
        enemy.AddComponent<Enemy>();
        enemy.GetComponent<Enemy>().enemyData = enemyData;
        enemy.GetComponent<Enemy>().weaponData = weaponData;
        enemy.GetComponent<Enemy>().enemyPatternData = attackPattern;

        return enemy;
    }
}

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    public WeaponData weaponData;
    public AttackPattern enemyPatternData;

    public Enemy()
    {
        // crea el enemigo con los datos que le pasamos


    }
}