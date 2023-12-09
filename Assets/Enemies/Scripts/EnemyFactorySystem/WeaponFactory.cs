using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFactory : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public WeaponFactory()
    {
        // empty constructor

    }


    public WeaponData CreateWeaponDataObject(int level, EnemyFactory.enemyType type)
    {
        // vamos a construir un objeto de la clase WeaponData con los datos que nos pasan y 
        // devolverlo con para que lo use EnemyFactory, donde haremos this.data = WeaponFactory.CreateWeaponDataObject(level, type);

        switch (type)
        {
            case EnemyFactory.enemyType.melee:
                // caso melee
                break;

            case EnemyFactory.enemyType.ranged:
                // caso ranged
                break;

            case EnemyFactory.enemyType.boss:
                // caso boss
                break;
                
            default:
                break;
        }
        return new WeaponData();
    }
}

public class WeaponData
{

    public int level;
    public int damage;
    public float accuracy;

    public WeaponData()
    {
        // constructor a rellenar
    }

}
