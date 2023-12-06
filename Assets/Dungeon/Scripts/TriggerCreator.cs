using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCreator
{
    private Vector2 pos; public Vector2 Pos { get { return pos; } }
    private GameObject parent;
    private Node node;

    public TriggerCreator(Vector2 pos, GameObject parent, Node node)
    {
        this.pos = pos;
        this.parent = parent;
        this.node = node;
    }

    public void InstantiateTrigger(GameObject go)
    {
        go.transform.parent = parent.transform;
        go.transform.position = pos;
        go.GetComponent<BoxCollider2D>().size = new Vector2(node.RoomWidth-2, node.RoomHeight-2);
        RoomTrigger roomTrigger = go.GetComponent<RoomTrigger>();
        roomTrigger.node = node;
    }

}
