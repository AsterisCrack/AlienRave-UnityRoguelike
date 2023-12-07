using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public Node node;
    private EnemySpawner enemySpawner;

    private void Start()
    {
        enemySpawner = EnemySpawner.instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Character") && !node.visited && node.type == Node.roomType.normal)
        {
            node.visited = true;
            int width = node.RoomWidth-4;
            int height = node.RoomHeight-4;
            int depth = node.depth;
            Vector2 centerPos = node.Position;
            StartCoroutine(enemySpawner.Spawn(centerPos, width, height, depth));
        }
    }

    //Draw gizmos to show the safe radius
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (node != null)
        {
            Gizmos.DrawWireCube(node.Position, new Vector3(node.RoomWidth - 4, node.RoomHeight - 4, 0));
        }  
    }
}
