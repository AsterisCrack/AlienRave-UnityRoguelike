using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocator : MonoBehaviour
{

    public GameObject player;
    // Start is called before the first frame update
    public void FindPlayer()
    {
        //Find player with tag "Player" or "Character"
        player = GameObject.FindGameObjectWithTag("Character");

        if (player == null)
        {
            Debug.LogError("Player not found");
        }
    }

    public bool IsPlayerInSight()
    {
        //Check if player is in sight
        float distanceToPlayer = GetDistanceToPlayer();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, Mathf.Infinity, LayerMask.GetMask("CollidableWall"));
        if (hit.collider != null)
        {
            if (distanceToPlayer < hit.distance)
            {
                return true;
            }
        }
        return false;
    }

    public float GetDistanceToPlayer()
    {
        //Get distance to player
        return Vector2.Distance(transform.position, player.transform.position);
    }

    public bool IsPlayerAlive()
    {
        //Check if player is alive
        return player.GetComponent<HealthHandler>().CurrentHealth > 0;
    }
}
