using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGunMovement : MonoBehaviour
{
    [SerializeField] private bool alwaysPointPlayer = true;

    private GameObject player;
    private Vector3 startingPosition;

    // Start is called before the first frame update
    void Awake()
    {
        //Find player with tag "Player" or "Character"
        player = GameObject.FindGameObjectWithTag("Character");
        
        if(player == null)
        {
            Debug.LogError("Player not found");
        }
        startingPosition = transform.localPosition;
    }

    private bool IsPlayerInSight()
    {
        //Check if player is in sight
        RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, Mathf.Infinity);
        if(hit.collider != null)
        {
            if(hit.collider.gameObject.tag == "Character")
            {
                return true;
            }
        }
        return false;
    }

    private void PointToPlayer()
    {
        //Point to player
        Vector3 direction = player.transform.position - transform.position;
        Vector3 directionFromParent = player.transform.position - transform.parent.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angleFromParent = Mathf.Atan2(directionFromParent.y, directionFromParent.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        AnimateGun(angleFromParent);
    }

    private void AnimateGun(float angle)
    {
        //If the angle is between -90 and 90, then the gun is facing right
        gameObject.GetComponent<SpriteRenderer>().flipY = !(angle > -90 && angle < 90);
        transform.localPosition = (angle > -90 && angle < 90) ? startingPosition : new Vector3(-startingPosition.x, startingPosition.y, startingPosition.z);

        //Move the gun to the correct layer if the gun is behind or on top of the player
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = angle > 0 && angle < 180 ? 2 : 4;
    }

    // Update is called once per frame
    void Update()
    {
        if(alwaysPointPlayer)
        {
            PointToPlayer();
        }
        else
        {
            if(IsPlayerInSight())
            {
                PointToPlayer();
            }
        }
    }
}
