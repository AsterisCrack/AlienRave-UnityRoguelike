using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    //Class to represent a node in the graph
    public int id;  
    public List<Node> neighbours;
    public List<Edge> edges;

    public float x;
    public float y;

    public Vector3 Position { get { return new Vector3(x, y, 0); } }

    public enum roomType { normal, start, boss, shop, treasure };
    public roomType type;
    private int roomWidth;
    private int roomHeight;
    public float safeRadius;
    public int setRoomWidth { set { roomWidth = value; safeRadius = (float)Mathf.Min(roomWidth, roomHeight) / 2; } }
    public int setRoomHeight { set { roomHeight = value; safeRadius = (float)Mathf.Min(roomWidth, roomHeight) / 2; } }
    public int RoomWidth { get { return roomWidth; } }
    public int RoomHeight { get { return roomHeight; } }

    public int rank;

    //Initialise the node
    public Node(int id, int x, int y, int roomWidth=0, int roomHeight=0, roomType type=roomType.normal)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        neighbours = new List<Node>();
        edges = new List<Edge>();
        //Take padding into account for walls
        this.roomWidth = roomWidth+2;
        this.roomHeight = roomHeight+2;
        //Safe radius is the radius of the circle that can fit in the room
        safeRadius = (float)Mathf.Min(roomWidth, roomHeight) / 2;

        rank = 0;
        //Set the type of the room
        this.type = type;
    }

    public void AddNeighbour(Node node)
    {
        neighbours.Add(node);
    }
}
