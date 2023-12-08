using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
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
    public Room room;

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
    public int depth;
    public bool visited;

    List<SecurityGuard> guards = new List<SecurityGuard>();

    //Initialise the node
    public Node(int id, int x, int y, Room room, int roomWidth=0, int roomHeight=0, roomType type=roomType.normal)
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
        depth = 0;
        visited = false;
        //Set the type of the room
        this.type = type;
        this.room = room;
    }

    public void AddNeighbour(Node node)
    {
        neighbours.Add(node);
    }

    public void CreateGuards(GameObject securityGuardPrefab, bool[,] dungeonMatrix, float displacementX, float displacementY)
    {
        this.guards = new List<SecurityGuard>();
        //Go throug the wall tilemap and see if there is an empry space at the frontier of the room. If there is, create a guard there
        for (int i = (int)x - roomWidth / 2; i < x + roomWidth / 2; i++)
        {
            try { 
                Vector3 pos = new Vector3(i - displacementX, y + roomHeight / 2 - displacementY, 0);
                if (dungeonMatrix[(int)pos.x, (int)pos.y] && dungeonMatrix.GetLength(0) > (int)pos.x + 1)
                {
                    if (!dungeonMatrix[(int)pos.x + 1, (int)pos.y])
                    {
                        Vector2 bottomDoorPos = new Vector2(pos.x + displacementX, pos.y + displacementY);
                        SecurityGuard guard = SecurityGuardSpawner.instance.CreateGuard(bottomDoorPos, SecurityGuard.GuardPosition.Up, this);
                        this.guards.Add(guard);
                    }
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                
            }
        }

        for (int i = (int)x - roomWidth / 2; i < x + roomWidth / 2; i++)
        {
            try
            {
                Vector3 pos = new Vector3(i - displacementX, y - roomHeight / 2 - displacementY, 0);
                if (dungeonMatrix[(int)pos.x, (int)pos.y] && dungeonMatrix.GetLength(0) > (int)pos.x + 1)
                {
                    if (!dungeonMatrix[(int)pos.x + 1, (int)pos.y])
                    {
                        Vector2 topDoorPos = new Vector2(pos.x + displacementX, pos.y + displacementY);
                        SecurityGuard guard = SecurityGuardSpawner.instance.CreateGuard(topDoorPos, SecurityGuard.GuardPosition.Down, this);
                        this.guards.Add(guard);
                    }
                }
            }
            catch (System.IndexOutOfRangeException)
            {

            }
        }

        for (int i = (int)y - roomHeight / 2; i < y + roomHeight / 2; i++)
        {
            try
            {
                Vector3 pos = new Vector3(x + roomWidth / 2 - displacementX, i - displacementY, 0);
                if (dungeonMatrix[(int)pos.x, (int)pos.y] && dungeonMatrix.GetLength(1) > (int)pos.y + 1)
                {
                    if (!dungeonMatrix[(int)pos.x, (int)pos.y + 1])
                    {
                        Vector2 leftDoorPos = new Vector2(pos.x + displacementX, pos.y + displacementY);
                        SecurityGuard guard = SecurityGuardSpawner.instance.CreateGuard(leftDoorPos, SecurityGuard.GuardPosition.Right, this);
                        this.guards.Add(guard);
                    }
                }
            }
            catch (System.IndexOutOfRangeException)
            {

            }
        }

        for (int i = (int)y - roomHeight / 2; i < y + roomHeight / 2; i++)
        {
            try
            {
                Vector3 pos = new Vector3(x - roomWidth / 2 - displacementX, i - displacementY, 0);
                if (dungeonMatrix[(int)pos.x, (int)pos.y] && dungeonMatrix.GetLength(1) > (int)pos.y + 1)
                {
                    if (!dungeonMatrix[(int)pos.x, (int)pos.y + 1])
                    {
                        Vector2 rightDoorPos = new Vector2(pos.x + displacementX, pos.y + displacementY);
                        SecurityGuard guard = SecurityGuardSpawner.instance.CreateGuard(rightDoorPos, SecurityGuard.GuardPosition.Left, this);
                        this.guards.Add(guard);
                    }
                }
            }
            catch (System.IndexOutOfRangeException)
            {

            }
        }
    }

    public void CreateRoomClearHandler(List<EnemyHealthHandler> enemies, BossHealthHandler boss)
    {
        RoomClearHandler roomClearHandler = new RoomClearHandler(this.guards, enemies, boss);
    }
}
