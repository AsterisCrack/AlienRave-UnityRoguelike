using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


/*
 * We need to create a bullet collision handler so that we can detect when a bullet hits a player or an enemy.
 * In such case, the player/enemy should take damage and the bullet should be destroyed (it doesn't pierce the target).
 * Then, we need to create an enemy collision handler so that we can detect when an enemy hits a player.
 * 
 * To do this we have to create a new script called CollisionHandler.cs and attach it to the bullet prefab.
 * Then we have to create a new script called EnemyCollisionHandler.cs and attach it to the enemy prefab.
 * 
 */
