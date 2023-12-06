using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using DelaunatorSharp;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;
using System;
using System.Runtime.CompilerServices;

[ExecuteAlways]
public class DungeonGraphGeneratorV2 : MonoBehaviour
{
    //Script to generate a random graph of the dungeon to be built
    [Header("Editor settings")]
    //Button to generate the graph
    [SerializeField] private bool generateGraph;
    [SerializeField] private bool verbose = false;
    [SerializeField] private bool drawTilemap;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap boolTilemap;
    //Tiles
    [SerializeField] private RuleTile groundTile;
    [SerializeField] private RuleTile wallTile;
    [SerializeField] private RuleTile emptyTile;
    [SerializeField] private RuleTile trueTile;

    [Header("Dungeon settings")]
    [SerializeField] private int minRooms;
    [SerializeField] private int maxRooms;
    [SerializeField] private float extraDistanceMin;
    [SerializeField] private float extraDistanceMax;
    [SerializeField] private float minEdgesMultiplier;
    [SerializeField] private float maxEdgesMultiplier;
    [SerializeField] private int numEdgesToPickFrom = 3;

    [Header("Room settings")]
    [SerializeField] private int minRoomSize;
    [SerializeField] private int maxRoomSize;

    [Header("Corridor settings")]
    [SerializeField] private int hallwayWidth;

    [Header("Needed prefabs")]
    [SerializeField] private GameObject roomTriggerPrefab;
    private List<TriggerCreator> triggers = new List<TriggerCreator>();

    [Header("Visualization settings")]
    [SerializeField] private bool visualizeGraph;
    [SerializeField] private float nodeSize;
    [SerializeField] private bool drawFrontier;

    private List<Vector2> testingPosList = new List<Vector2>();

    private List<Node> nodes;
    private List<Edge> edges;
    private MatrixDilation matrixDilation = new MatrixDilation();
    private bool[,] simpleKernel = new bool[3, 3] { { false, true, false }, 
                                                    { true, false, true }, 
                                                    { false, true, false } };
    //enum for the tile types
    public enum TileType
    {
        Ground,
        leftWall,
        rightWall,
        topWall,
        bottomWall,
        empty
    }
    //Matrix to store the full map
    private bool[,] roomMatrix;
    private int matrixDisplacementX;
    private int matrixDisplacementY;

    private SetAstarGraph setAstarGraph;

    //event when finished
    public event Action<Vector2> OnFinished;

    private void AddRoomToMatrix(float x, float y, int roomWidth, int roomHeight, Room room)
    {
        //First we need to create a bigger matrix to store the room
        //Check how much we need to add to the matrix
        int centerX = (int)Mathf.Ceil(x);
        int centerY = (int)Mathf.Ceil(y);
        int numColumnsRigth = 0;
        int numColumnsLeft = 0;
        int numRowsTop = 0;
        int numRowsBottom = 0;

        //Accomodate for walls
        roomWidth += 2;
        roomHeight += 3;

        //if room matrix is null, we need to create it
        if (roomMatrix == null)
        {
            roomMatrix = new bool[roomWidth, roomHeight];
        }

        if (centerX + roomWidth / 2 > roomMatrix.GetLength(0) + matrixDisplacementX)
        {
            //We need to add more columns to the right
            numColumnsRigth = (int)Mathf.Ceil(centerX + roomWidth / 2 - matrixDisplacementX - roomMatrix.GetLength(0));
        }   
        if (centerX - roomWidth / 2 < 0 + matrixDisplacementX)
        {
            //We need to add more columns to the left
            numColumnsLeft = matrixDisplacementX + (int)Mathf.Ceil(Mathf.Abs(centerX - roomWidth / 2));
        }
        if (centerY + roomHeight/2 + 1 > roomMatrix.GetLength(1) + matrixDisplacementY)
        {
            //We need to add more rows to the top
            numRowsTop = (int)Mathf.Ceil(centerY + roomHeight / 2 - matrixDisplacementY - roomMatrix.GetLength(1)) + 1;
        }
        if (centerY - roomHeight / 2 < 0 + matrixDisplacementY)
        {
            //We need to add more rows to the bottom
            numRowsBottom = matrixDisplacementY + (int)Mathf.Ceil(Mathf.Abs(centerY - roomHeight / 2));
        }

        //Now we need to create a new matrix with the new size
        bool[,] newMatrix = new bool[roomMatrix.GetLength(0) + numColumnsLeft + numColumnsRigth, roomMatrix.GetLength(1) + numRowsTop + numRowsBottom];
        //Set all the positions to empty tile
        for (int i = 0; i < newMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < newMatrix.GetLength(1); j++)
            {
                newMatrix[i, j] = false;
            }
        }
        //Now we need to copy the old matrix to the new one
        for (int i = 0; i < roomMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < roomMatrix.GetLength(1); j++)
            {
                newMatrix[i + numColumnsLeft, j + numRowsBottom] = roomMatrix[i, j];
            }
        }

        //Now we need to update the mapMatrix
        roomMatrix = newMatrix;

        //Now we need to update the displacement
        matrixDisplacementX -= numColumnsLeft;
        matrixDisplacementY -= numRowsBottom;

        //Now we update the matrix by adding the new room
        bool[,] groundMatrix = room.GetRoomMatrix();
        //Stablish the starting points of the room
        int groundStartingPosX = centerX - groundMatrix.GetLength(0) / 2 - matrixDisplacementX;
        int groundStartingPosY = centerY - groundMatrix.GetLength(1) / 2 - matrixDisplacementY;

        //Now we need to update the mapMatrix
        MergeMatrix(roomMatrix, groundMatrix, groundStartingPosX, groundStartingPosY);
    }

    private void MergeMatrix(bool[,] map, bool[,] matrix, int centerX, int centerY)
    {
        if (map == null)
        {
            return;
        }
        if (matrix == null)
        {
            return;
        }

        //Now we need to merge the matrix with the map
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            //Check if the position is inside the map
            if (i + centerX < map.GetLength(0) && i + centerX >= 0)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    //Check if the position is inside the map
                    if (j + centerY < map.GetLength(1) && j + centerY >= 0)
                    {
                        //Now we need to check if the position is true in the matrix
                        if (matrix[i, j])
                        {
                            map[i + centerX, j + centerY] = true;
                        }
                    }
                }
            }
        }   
    }
    private List<Vector2> GetFrontierList(bool[,] matrix, int matrixDisplacementX, int matrixDisplacementY)
    {
        if (matrix == null)
        {
            return null;
        }

        List<Vector2> frontierList = new List<Vector2>();
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j])
                {
                    frontierList.Add(new Vector2(i + matrixDisplacementX, j + matrixDisplacementY));
                }
            }
        }
        return frontierList;
    }

    private void ClearPreviousTriggers()
    {
        //Destroy all children
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
        triggers.Clear();
    }
    //Initialise the graph
    public void InitialiseGraph()
    {
        //Measure times of each function
        //Empty the lists
        nodes = new List<Node>();
        edges = new List<Edge>();
        roomMatrix = null;
        matrixDisplacementX = 0;
        matrixDisplacementY = 0;

        //Generate the nodes
        //Measure time 
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        nodes = GenerateNodes();
        stopwatch.Stop();
        if (verbose) Debug.Log("Nodes generated in: " + stopwatch.ElapsedMilliseconds + " ms");

        //Generate the edges
        stopwatch.Reset();
        stopwatch.Start();
        edges = GenerateEdges();
        stopwatch.Stop();
        if (verbose) Debug.Log("Edges generated in: " + stopwatch.ElapsedMilliseconds + " ms");

        //Generate the hallways
        stopwatch.Reset();
        stopwatch.Start();
        bool errorInHallways = GenerateHallways();
        stopwatch.Stop();
        if (verbose) Debug.Log("Hallways generated in: " + stopwatch.ElapsedMilliseconds + " ms");

        if (errorInHallways)
        {
            return;
        }
        //Get room depths
        stopwatch.Reset();
        stopwatch.Start();
        GetRoomDepths();
        stopwatch.Stop();
        if (verbose) Debug.Log("Room depths calculated in: " + stopwatch.ElapsedMilliseconds + " ms");

        //Print number of nodes and edges and the number of nodes in the force graph
        if (verbose) Debug.Log("Nodes: " + nodes.Count + " Edges: " + edges.Count);

        if (drawTilemap)
        {
            //drawBoolMap();
            stopwatch.Reset();
            stopwatch.Start();
            DrawTilemap();
            stopwatch.Stop();
            if (verbose) Debug.Log("Tilemap drawn in: " + stopwatch.ElapsedMilliseconds + " ms");
        }

        //Instantiate all triggers
        foreach (TriggerCreator trigger in triggers)
        {
            Vector2 pos = trigger.Pos;
            GameObject go = Instantiate(roomTriggerPrefab, pos, Quaternion.identity);
            trigger.InstantiateTrigger(go);
        }

        //Set the Astar graph
        setAstarGraph= GetComponent<SetAstarGraph>();
        setAstarGraph.SetGraph(roomMatrix.GetLength(0), roomMatrix.GetLength(1), matrixDisplacementX + (float)roomMatrix.GetLength(0)/ 2f + 0.5f, matrixDisplacementY + (float)roomMatrix.GetLength(1)/2f);

        //Invoke the event with the position of the first node
        if (Application.isPlaying)
        {
            Node node = nodes[0];
            foreach (Node n in nodes)
            {
                if (n.type == Node.roomType.start)
                {
                    node = n;
                    break;
                }
            }
            node.visited = true;
            OnFinished.Invoke(node.Position);
        }
    }

    //First generate the nodes randomly
    private List<Node> GenerateNodes()
    {
        ClearPreviousTriggers();
        //Generate a random number of rooms
        int numRooms = UnityEngine.Random.Range(minRooms, maxRooms);
        //First we generate first node in the center of the map
        //Generate a random room size. It has to be even
        int roomWidth = UnityEngine.Random.Range(minRoomSize, maxRoomSize);
        int roomHeight = UnityEngine.Random.Range(minRoomSize, maxRoomSize);
        if (roomWidth % 2 != 0)
        {
            roomWidth++;
        }
        if (roomHeight % 2 != 0)
        {
            roomHeight++;
        }
        Room roomInstance = new Room(0, 0, roomWidth, roomHeight);
        roomInstance.GenerateRoom();
        //Set the initial displacement to the center of the room
        //take into account walls
        matrixDisplacementX = -roomWidth / 2 - 1;
        matrixDisplacementY = -roomHeight / 2 - 1;
        //Add the room to the mapMatrix
        AddRoomToMatrix(0, 0, roomWidth, roomHeight, roomInstance);
        //Add the node to the list
        nodes.Add(new Node(0, 0, 0, roomInstance, roomWidth, roomHeight));
        //Add this rooms trigger
        triggers.Add(new TriggerCreator(new Vector2(0, 0), gameObject, nodes[0]));

        //Generate the rooms
        for (int i = 1; i < numRooms; i++)
        {
            //Generate a random room size
            roomWidth = UnityEngine.Random.Range(minRoomSize, maxRoomSize);
            roomHeight = UnityEngine.Random.Range(minRoomSize, maxRoomSize);
            if (roomWidth % 2 != 0)
            {
                roomWidth++;
            }
            if (roomHeight % 2 != 0)
            {
                roomHeight++;
            }
            roomInstance = new Room(0, 0, roomWidth, roomHeight);
            roomInstance.GenerateRoom();

            float extraDistance = UnityEngine.Random.Range(extraDistanceMin, extraDistanceMax);
            int spaceX = (int)Mathf.Ceil(roomWidth / 2 + extraDistance);
            int spaceY = (int)Mathf.Ceil(roomHeight / 2 + extraDistance);

            //Now we need to obtain the frontier of the current graph, displaced by this width and height and place the room somewere on this frontier
            //We will do it by Dilating the bool matrix of the rooms. We will create a kernel that accomodates the space needed for the room
            
            bool[,] kernel = new bool[Mathf.Max(spaceX, spaceY) * 2 + 1, Mathf.Max(spaceX, spaceY) * 2 + 1];
            //Set the kernel to true completely
            for (int j = 0; j < kernel.GetLength(0); j++)
            {
                for (int k = 0; k < kernel.GetLength(1); k++)
                {
                    kernel[j, k] = true;
                }
            }

            //Now add the padding to the matrix
            bool[,] paddedMatrix = matrixDilation.Padding(roomMatrix, Mathf.Max(spaceX + 2, spaceY + 2));

            //Now we need to dilate the matrix
            bool[,] dilatedMatrix = matrixDilation.Dilate(paddedMatrix, kernel);

            //Now we have a matrix with the frontier of the graph, but the frontier is too wide, we just need a single line wide frontier
            //We will do it by dilating the matrix with a 3x3 kernel without center point
            bool[,] frontierMatrix = matrixDilation.DilateOnlyFrontier(dilatedMatrix, simpleKernel);
            
            //Now we have the frontier, we need to find a random point in the frontier to place the room
            //We will do it by creating a list of all the points in the frontier
            List<Vector2> frontierList = GetFrontierList(frontierMatrix, matrixDisplacementX - spaceX - 2, matrixDisplacementY - spaceY - 2);

            //Now we need to select a random point in the list
            Vector2 roomPos = frontierList[UnityEngine.Random.Range(0, frontierList.Count)];

            //Now we need to add the room to the mapMatrix
            AddRoomToMatrix(roomPos.x, roomPos.y, roomWidth, roomHeight, roomInstance);

            //Now we need to add the node to the list
            nodes.Add(new Node(i, (int)roomPos.x, (int)roomPos.y, roomInstance, roomWidth, roomHeight));
            //Add this rooms trigger
            triggers.Add(new TriggerCreator(roomPos, gameObject, nodes[i]));
        }
        return nodes;
    }

    private Node GetNodeFromPosition(Vector3 position)
    {
        foreach (Node node in nodes)
        {
            if (node.Position == position)
            {
                return node;
            }
        }
        return null;
    }

    private List<Edge> DelaunayTriangulation(List<Node> nodes)
    {
        //First we need a list of points to generate the delaunay triangulation
        List<Vector2> points = new List<Vector2>();
        foreach (Node node in nodes)
        {
            points.Add(node.Position);
        }

        //Now we need to generate the delaunay triangulation
        Delaunator delaunator = new Delaunator(points.ToArray().ToPoints());

        //Now we need to generate the edges
        delaunator.ForEachTriangleEdge(edge =>
        {
            Vector3 pos1 = edge.P.ToVector3();
            Vector3 pos2 = edge.Q.ToVector3();
            Node node1 = GetNodeFromPosition(pos1);
            Node node2 = GetNodeFromPosition(pos2);
            float distance = Vector3.Distance(pos1, pos2);
            if (node1 != null && node2 != null)
            {
                edges.Add(new Edge(node1, node2));
            }

        });

        return edges;
    }

    private List<Edge> Kruskal(List<Edge> edges)
    {
        //Kruskal algorithm to obtain the minimum spanning tree of our graph
        //First we need to sort the edges by weight
        edges.Sort((a, b) => a.weight.CompareTo(b.weight));

        //Create a list to store the minimum spanning tree
        List<Edge> mst = new List<Edge>();

        //Create a dictionary to store the parent of each node
        Dictionary<Node, Node> parent = new Dictionary<Node, Node>();

        //Initialize the parent of each node to itself
        foreach (Node node in nodes)
        {
            parent[node] = node;
        }

        //Iterate over the edges in ascending order of weight
        foreach (Edge edge in edges)
        {
            //Find the parent of each node in the edge
            Node parent1 = FindParent(edge.getNode1, parent);
            Node parent2 = FindParent(edge.getNode2, parent);

            //If the parents are not the same, add the edge to the minimum spanning tree
            if (parent1 != parent2)
            {
                mst.Add(edge);

                //Set the parent of the node with the smaller rank to the node with the larger rank
                if (parent1.rank < parent2.rank)
                {
                    parent[parent1] = parent2;
                }
                else if (parent1.rank > parent2.rank)
                {
                    parent[parent2] = parent1;
                }
                else
                {
                    parent[parent1] = parent2;
                    parent[parent2].rank++;
                }
            }
        }
        return mst;
    }

    private Node FindParent(Node node, Dictionary<Node, Node> parent)
    {
        //Find the parent of the node recursively
        if (parent[node] != node)
        {
            parent[node] = FindParent(parent[node], parent);
        }
        return parent[node];
    }

    private List<Edge> GenerateEdges()
    {
        //First we need a list of points to generate the delaunay triangulation
        List<Edge> allPossibleEdges = new List<Edge>();

        if (nodes.Count > 2) { 
            allPossibleEdges = DelaunayTriangulation(nodes);

            //Now we need the minimum spanning tree
            //We will use Kruskal algorithm
            edges = Kruskal(allPossibleEdges);

            //Remove edges from the list of all possible edges and add the neighbours to the nodes
            foreach (Edge edge in edges)
            {
                allPossibleEdges.Remove(edge);
                edge.getNode1.AddNeighbour(edge.getNode2);
                edge.getNode2.AddNeighbour(edge.getNode1);
            }

            //Finally we need to add the random edges to make the graph more interesting
            //Order the edges by weight from smallest to largest
            allPossibleEdges.Sort((a, b) => a.weight.CompareTo(b.weight));

            int numEdges = (int)Mathf.Ceil(edges.Count * UnityEngine.Random.Range(minEdgesMultiplier, maxEdgesMultiplier));
            //Confirm that the number of edges is not greater than the number of possible edges
            numEdges = Mathf.Min(numEdges, nodes.Count * (nodes.Count - 1) / 2);
            //Confirm that there are enough edges to connect all the nodes
            numEdges = Mathf.Max(numEdges, nodes.Count - 1);

            //Generate the random edges
            for (int i = 0; i < numEdges - edges.Count; i++)
            {
                //Select a random edge. Dont use uniform random because we want to select the edges with less weight more often
                //int e = UnityEngine.Random.Range(0, allPossibleEdges.Count);
                List<int> possibleEdges = new List<int>();
                //Introduce first numEdgesToPickFrom edges if they exist and select one of them randomly
                //This is to not pick the longest corridors
                for (int j = 0; j < Mathf.Min(numEdgesToPickFrom, allPossibleEdges.Count()); j++)
                {
                    possibleEdges.Add(j);
                }
                int e = possibleEdges[UnityEngine.Random.Range(0, possibleEdges.Count)];
                Edge edge = allPossibleEdges[e];
                edges.Add(edge);
                edge.getNode1.AddNeighbour(edge.getNode2);
                edge.getNode2.AddNeighbour(edge.getNode1);
                allPossibleEdges.Remove(edge);
            }
        }
        else
        {
            //Just add the edge between the two nodes
            edges.Add(new Edge(nodes[0], nodes[1]));
            nodes[0].AddNeighbour(nodes[1]);
            nodes[1].AddNeighbour(nodes[0]);
        }
        return edges;
    }

    private int CalculateNodeDistance(Node n1, Node n2)
    {
        //Calculate the distance in the graph between two nodes
        //We will use BFS
        //First we need to create a queue to store the nodes to visit
        Queue<Node> queue = new Queue<Node>();
        //Create a dictionary to store the distance of each node
        Dictionary<Node, int> distance = new Dictionary<Node, int>();
        //Create a dictionary to store the parent of each node
        Dictionary<Node, Node> parent = new Dictionary<Node, Node>();

        //Initialize the distance of each node to infinity
        foreach (Node node in nodes)
        {
            distance[node] = int.MaxValue;
        }

        //Initialize the distance of the first node to 0
        distance[n1] = 0;
        //Add the first node to the queue
        queue.Enqueue(n1);

        //Iterate over the queue
        while (queue.Count > 0)
        {
            //Get the first node in the queue
            Node node = queue.Dequeue();
            //Iterate over the neighbours of the node
            foreach (Node neighbour in node.neighbours)
            {
                //If the distance of the neighbour is infinity, add it to the queue
                if (distance[neighbour] == int.MaxValue)
                {
                    queue.Enqueue(neighbour);
                    //Set the distance of the neighbour to the distance of the node + 1
                    distance[neighbour] = distance[node] + 1;
                    //Set the parent of the neighbour to the node
                    parent[neighbour] = node;
                }
            }
        }
        //Return the distance of the second node
        return distance[n2];
    }

    private Node GetFurthestNode(Node n)
    {
        //Function to get the furthest node from a node
        //We will use BFS
        //First we need to create a queue to store the nodes to visit
        Queue<Node> queue = new Queue<Node>();
        //Create a dictionary to store the distance of each node
        Dictionary<Node, int> distance = new Dictionary<Node, int>();
        //Create a dictionary to store the parent of each node
        Dictionary<Node, Node> parent = new Dictionary<Node, Node>();
        foreach (Node node in nodes)
        {
            //Initialize the distance of each node to infinity
            distance[node] = int.MaxValue;
            parent[node] = node;
        }

        //Initialize the distance of the first node to 0
        distance[n] = 0;
        //Add the first node to the queue
        queue.Enqueue(n);
        while (queue.Count > 0)
        {
            //Get the first node in the queue
            Node node = queue.Dequeue();
            //Iterate over the neighbours of the node
            foreach (Node neighbour in node.neighbours)
            {
                //If the distance of the neighbour is infinity, add it to the queue
                if (distance[neighbour] == int.MaxValue)
                {
                    queue.Enqueue(neighbour);
                    //Set the distance of the neighbour to the distance of the node + 1
                    distance[neighbour] = distance[node] + 1;
                    //Set the parent of the neighbour to the node
                    parent[neighbour] = node;
                }
            }
        }
        //Now we need to find the node with the maximum distance
        int maxDistance = 0;
        Node furthestNode = null;
        foreach (Node node in nodes)
        {
            if (distance[node] > maxDistance)
            {
                maxDistance = distance[node];
                furthestNode = node;
            }
        }
        return furthestNode;
    }

    private void GetRoomDepths()
    {
        //Get the 2 furthst nodes. One will be entrance and the other the boss room
        Node entrance = GetFurthestNode(nodes[0]);
        Node boss = GetFurthestNode(entrance);
        entrance.type = Node.roomType.start;
        boss.type = Node.roomType.boss;

        //Now set the depth of each node
        entrance.depth = 0;
        foreach (Node node in nodes)
        {
            if (node != entrance)
            {
                node.depth = CalculateNodeDistance(entrance, node);
            }
        }
    }

    public class NodeRanges
    {
        public int top = 0;
        public int bottom = 0;
        public int left = 0;
        public int right = 0;
        public NodeRanges(Node n, int displacementX = 0, int displacementY = 0)
        {
            top = (int)n.Position.y + n.RoomHeight / 2 - 1 - displacementY;
            bottom = (int)n.Position.y - n.RoomHeight / 2 + 1 - displacementY;
            left = (int)n.Position.x - n.RoomWidth / 2 + 1 - displacementX;
            right = (int)n.Position.x + n.RoomWidth / 2 - 1 - displacementX;
        }
    }

    private bool GenerateHallways()
    {
        //Generate and draw the hallways between the rooms
        List<Edge> edgesCopy = new List<Edge>(edges);
        bool success = true;

        foreach (Edge edge in edges)
        {
            //Find thhe midpoint between the two nodes
            Node n1 = edge.getNode1;
            Node n2 = edge.getNode2;
            Vector3 midpoint = (n1.Position + n2.Position) / 2;

            //We need to check where the midpoint is in relation to the rooms.
            //There are 3 cases:
            //1. The midpoint x is between the rooms height boundaries => The hallway will be horizontal
            //2. The midpoint y is between the rooms width boundaries => The hallway will be vertical
            //3. The midpoint is not between the rooms boundaries => The hallway will be L shape

            int n1Left = (int)Mathf.Ceil(n1.Position.x - n1.RoomWidth / 2 + 1);
            int n1Right = (int)Mathf.Ceil(n1.Position.x + n1.RoomWidth / 2 - 1 - hallwayWidth);
            int n1Top = (int)Mathf.Ceil(n1.Position.y + n1.RoomHeight / 2 - 1 - hallwayWidth);
            int n1Bottom = (int)Mathf.Ceil(n1.Position.y - n1.RoomHeight / 2 + 1);

            int n2Left = (int)Mathf.Ceil(n2.Position.x - n2.RoomWidth / 2 + 1);
            int n2Right = (int)Mathf.Ceil(n2.Position.x + n2.RoomWidth / 2 - 1 - hallwayWidth);
            int n2Top = (int)Mathf.Ceil(n2.Position.y + n2.RoomHeight / 2 - 1 - hallwayWidth);
            int n2Bottom = (int)Mathf.Ceil(n2.Position.y - n2.RoomHeight / 2 + 1);

            bool n1Horizontal = midpoint.x >= n1Left && midpoint.x <= n1Right;
            bool n1Vertical = midpoint.y >= n1Bottom && midpoint.y <= n1Top;

            bool n2Horizontal = midpoint.x >= n2Left && midpoint.x <= n2Right;
            bool n2Vertical = midpoint.y >= n2Bottom && midpoint.y <= n2Top;

            //Now we need to generate the hallway
            //First we need to check if the hallway is horizontal or vertical
            void DrawStraightHallway(bool horizontal)
            {
                //Get the possible range for the hallway, wich is the intesection of the segments n1Left, n1Right and n2Left, n2Right
                int hallwayRight = Mathf.Max(n1Left, n2Left) - matrixDisplacementX;
                int hallwayLeft = Mathf.Min(n1Right, n2Right) - matrixDisplacementX;
                int hallwayBottom = Mathf.Min(n1Top, n2Top) - matrixDisplacementY;
                int hallwayTop = Mathf.Max(n1Bottom, n2Bottom) - matrixDisplacementY;
                
                //Generate a random position for the hallway
                //int hallwayPos = horizontal ? UnityEngine.Random.Range(hallwayLeft, hallwayRight) : UnityEngine.Random.Range(hallwayBottom, hallwayTop);

                int hallwayPos;
                List<int> possiblePos = new List<int>();
                //We need to check that there is space on both sides for the hallway
                if (horizontal)
                {
                    for (int p = hallwayRight; p < hallwayLeft; p++)
                    {
                        bool possible = true;
                        for (int i = hallwayBottom + hallwayWidth + 2; i < hallwayTop - 2; i++)
                        {
                            for (int j = -1; j <= hallwayWidth; j++)
                            {
                                if (roomMatrix[p + j, i]) possible = false;
                            }
                            if(!possible)
                            {
                                break;
                            }
                        }
                        if (possible)
                        {
                            possiblePos.Add(p);
                        }
                    }
                }
                else
                {
                    for (int p = hallwayTop; p < hallwayBottom; p++)
                    {
                        bool possible = true;
                        for (int i = hallwayLeft + hallwayWidth + 2; i < hallwayRight - 2; i++)
                        {
                            for (int j = -1; j <= hallwayWidth; j++)
                            {
                                if (roomMatrix[i, p + j]) possible = false;
                            }
                            if (!possible)
                            {
                                break;
                            }
                        }
                        if (possible)
                        {
                            possiblePos.Add(p);
                        }
                    }
                }   

                if (possiblePos.Count == 0)
                {
                    success = false;
                    return;
                }

                hallwayPos = possiblePos[UnityEngine.Random.Range(0, possiblePos.Count)];

                //Now we need to generate the hallway
                //Insert it in the MapMatrix

                //Generate hallways
                if (horizontal)
                {
                    for (int i = hallwayBottom + hallwayWidth; i < hallwayTop; i++)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            roomMatrix[hallwayPos + j, i] = true;
                        }
                    }

                }
                else
                {
                    for (int i = hallwayLeft + hallwayWidth; i < hallwayRight; i++)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            roomMatrix[i, hallwayPos + j] = true;
                        }
                    }
                }
            }
            if (n1Horizontal && n2Horizontal) DrawStraightHallway(true);
            else if (n1Vertical && n2Vertical) DrawStraightHallway(false);
            else
            {
                //L shape hallway
                //We will use another metod. Basically, extend the walls of the rooms until they intersect and substract where there exists a room
                Node nodeBottom = n1.Position.y < n2.Position.y ? n1 : n2;
                Node nodeTop = n1.Position.y > n2.Position.y ? n1 : n2;
                Node nodeLeft = n1.Position.x < n2.Position.x ? n1 : n2;
                Node nodeRight = n1.Position.x > n2.Position.x ? n1 : n2;
               

                NodeRanges nodeBottomRanges = new NodeRanges(nodeBottom, matrixDisplacementX, matrixDisplacementY);
                NodeRanges nodeTopRanges = new NodeRanges(nodeTop, matrixDisplacementX, matrixDisplacementY);
                NodeRanges nodeLeftRanges = new NodeRanges(nodeLeft, matrixDisplacementX, matrixDisplacementY);
                NodeRanges nodeRightRanges = new NodeRanges(nodeRight, matrixDisplacementX, matrixDisplacementY);

                //Extend the walls
                //First the walls of the top left corridor, starting from the bottom
                void ExtendCorridorVertical(bool[,] matrix, int r1, int r2, int r3, int r4)
                {
                    for (int i = r1; i < r2; i++)
                    {
                        for (int j = r3; j < r4; j++)
                        {
                            if (roomMatrix[i, j] || roomMatrix[i-1, j] || roomMatrix[i+1, j])
                            {
                                //Its not possible to extend the wall. Delete the previous walls
                                for (int k = r3; k < j; k++)
                                {
                                    matrix[i, k] = false;
                                }
                                break;
                            }
                            else
                            {
                                matrix[i, j] = true;
                            }
                        }
                    }
                }
                void ExtendCorridorHorizontal(bool[,] matrix, int r1, int r2, int r3, int r4)
                {
                    for (int i = r1; i < r2; i++)
                    {
                        for (int j = r3; j < r4; j++)
                        {
                            if (roomMatrix[j, i] || roomMatrix[j, i - 1] || roomMatrix[j, i + 1])
                            {
                                //Its not possible to extend the wall. Delete the previous walls
                                for (int k = r3; k < j; k++)
                                {
                                    matrix[k, i] = false;
                                }
                                break;
                            }
                            else
                            {
                                matrix[j, i] = true;
                            }
                        }
                    }
                }

                //Matices the same size as map that store the extended walls
                bool[,] corridors = new bool[roomMatrix.GetLength(0), roomMatrix.GetLength(1)];

                ExtendCorridorVertical(corridors, nodeTopRanges.left, nodeTopRanges.right, nodeBottomRanges.bottom, nodeTopRanges.bottom-1);
                ExtendCorridorVertical(corridors, nodeBottomRanges.left, nodeBottomRanges.right, nodeBottomRanges.top+2, nodeTopRanges.top);

                ExtendCorridorHorizontal(corridors, nodeRightRanges.bottom, nodeRightRanges.top, nodeLeftRanges.left, nodeRightRanges.left-1);
                ExtendCorridorHorizontal(corridors, nodeLeftRanges.bottom, nodeLeftRanges.top, nodeLeftRanges.right+1, nodeRightRanges.right);

                //Now we have the possible corridors, we need to select one randomly
                //First, we need to reduce the list of corridors because we need space for the whole hallway.

                List<Vector2> n1StartsTop = new List<Vector2>();
                List<Vector2> n1StartsBottom = new List<Vector2>();
                List<Vector2> n1StartsLeft = new List<Vector2>();
                List<Vector2> n1StartsRight = new List<Vector2>();

                List<Vector2> n2StartsTop = new List<Vector2>();
                List<Vector2> n2StartsBottom = new List<Vector2>();
                List<Vector2> n2StartsLeft = new List<Vector2>();
                List<Vector2> n2StartsRight = new List<Vector2>();

                NodeRanges n1Ranges = new NodeRanges(n1, matrixDisplacementX, matrixDisplacementY);
                NodeRanges n2Ranges = new NodeRanges(n2, matrixDisplacementX, matrixDisplacementY);

                void GetStartPos(List<Vector2> list, int start, int end, int fixedPos, bool horizontal)
                {
                    if (!horizontal && (fixedPos >= corridors.GetLength(1) || fixedPos <= 0))
                    {
                        return;
                    }
                    else if (horizontal && (fixedPos >= corridors.GetLength(0) || fixedPos <= 0))
                    {
                        return;
                    }

                    for (int i = start; i < end; i++)
                    {
                        bool possible = true;
                        for (int j = 1; j <= hallwayWidth; j++)
                        {
                            if (horizontal)
                            {
                                if (corridors.GetLength(1) > (i + j - 1) && (i + j - 1) > 0)
                                {
                                    //Here we need one more since wallas are 2 tiles tall.
                                    if (corridors[fixedPos, i + j - 1] != true || corridors[fixedPos, i + j] != true)
                                    {
                                        possible = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (corridors.GetLength(0) > i + j - 1 && i + j - 1 > 0) { 
                                    if (corridors[i + j - 1, fixedPos] != true)
                                    {
                                        possible = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (possible)
                        {
                            if (horizontal) list.Add(new Vector2(fixedPos, i));
                            else list.Add(new Vector2(i, fixedPos));
                        }
                    }
                }   

                GetStartPos(n1StartsTop, n1Ranges.left, n1Ranges.right, n1Ranges.top + 2, false);
                GetStartPos(n1StartsBottom, n1Ranges.left, n1Ranges.right, n1Ranges.bottom - 2, false);
                GetStartPos(n1StartsLeft, n1Ranges.bottom, n1Ranges.top, n1Ranges.left - 2, true);
                GetStartPos(n1StartsRight, n1Ranges.bottom, n1Ranges.top, n1Ranges.right + 1, true);

                GetStartPos(n2StartsTop, n2Ranges.left, n2Ranges.right, n2Ranges.top + 2, false);
                GetStartPos(n2StartsBottom, n2Ranges.left, n2Ranges.right, n2Ranges.bottom - 2, false);
                GetStartPos(n2StartsLeft, n2Ranges.bottom, n2Ranges.top, n2Ranges.left - 2, true);
                GetStartPos(n2StartsRight, n2Ranges.bottom, n2Ranges.top, n2Ranges.right + 1, true);

                testingPosList.Clear();
                //Now we can finally build the hallway
                //First we pick a corner and then we pick a random point in the list
                List<List<List<Vector2>>> corners = new List<List<List<Vector2>>>();
                void AddCorners(List<Vector2> list1, List<Vector2> list2)
                {
                    //List 1 have the paths that have to go vertically and list 2 the ones that have to go horizontally
                    if (list1.Count != 0 && list2.Count != 0)
                    {
                        List<List<Vector2>> corner = new List<List<Vector2>>();
                        corner.Add(list1);
                        corner.Add(list2);
                        foreach (Vector2 pos1 in list1)
                        {
                            testingPosList.Add(pos1);
                        }
                        foreach (Vector2 pos2 in list2)
                        {
                            testingPosList.Add(pos2);
                        }


                        corners.Add(corner);
                    }
                }
                
                AddCorners(n1StartsTop, n2StartsLeft);
                AddCorners(n1StartsTop, n2StartsRight);
                AddCorners(n1StartsBottom, n2StartsLeft);
                AddCorners(n1StartsBottom, n2StartsRight);
                AddCorners(n2StartsTop, n1StartsLeft);
                AddCorners(n2StartsTop, n1StartsRight);
                AddCorners(n2StartsBottom, n1StartsLeft);
                AddCorners(n2StartsBottom, n1StartsRight);

                if (corners.Count() > 0)
                {
                    //select a random corner
                    int c = UnityEngine.Random.Range(0, corners.Count);
                    List<List<Vector2>> selectedCorner = corners[c];

                    //select a random start point from both lists
                    int s1 = UnityEngine.Random.Range(0, selectedCorner[0].Count);
                    int s2 = UnityEngine.Random.Range(0, selectedCorner[1].Count);

                    //Now we need to generate the hallway
                    //Insert it in the MapMatrix
                    Vector2 posTop = selectedCorner[0][s1].y > selectedCorner[1][s2].y ? selectedCorner[0][s1] : selectedCorner[1][s2];
                    Vector2 posBottom = selectedCorner[0][s1].y < selectedCorner[1][s2].y ? selectedCorner[0][s1] : selectedCorner[1][s2];

                    Vector2 posLeft = selectedCorner[0][s1].x < selectedCorner[1][s2].x ? selectedCorner[0][s1] : selectedCorner[1][s2];
                    Vector2 posRight = selectedCorner[0][s1].x > selectedCorner[1][s2].x ? selectedCorner[0][s1] : selectedCorner[1][s2];

                    //Generate hallways
                    //Vertical part

                    //We know that it has to start on s1 because it is the vertical part
                    int start = (int)selectedCorner[0][s1].x;
                    int yStart = (selectedCorner[0][s1] == posBottom) ? (int)posBottom.y - 2 : (int)posBottom.y;
                    for (int i = yStart; i < posTop.y + 2; i++)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            roomMatrix[start + j, i] = true;
                        }
                    }
                    
                    //Horizontal part. Starts on s2
                    start = (int)selectedCorner[1][s2].y;
                    int xStart = (selectedCorner[1][s2] == posLeft) ? (int)posLeft.x - 1 : (int)posLeft.x;            

                    for (int i = xStart; i < posRight.x + 1; i++)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            roomMatrix[i, start + j] = true;
                        }
                    }
                    if (selectedCorner[1][s2] == posRight)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            roomMatrix[(int)posRight.x + 1, start + j] = true;
                        }
                    }
                }
                else
                {
                    //No possible corridors
                    //Check if both of the nodes have other corridors
                    if (n1.neighbours.Count() > 1 && n2.neighbours.Count() > 1)
                    {
                        edgesCopy.Remove(edge);
                        n1.neighbours.Remove(n2);
                        n2.neighbours.Remove(n1);
                    }

                    //If not, retry the generation of the graph
                    success = false;
                    break;
                }
            }
        }

        if (success)
        {
            edges = new List<Edge>(edgesCopy);
        }
        else
        {
            if (verbose) Debug.Log("Failed to generate the graph. Retrying...");

            InitialiseGraph();
        }
        return success == false;
    }

    public void OnDrawGizmos()
    {
        //Draw the graph
        var offset = transform.position;
        // Draw Center Mark
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(offset, .1f * 2f);

        // Draw edges
        if (edges == null) return;
        Gizmos.color = Color.blue;
        foreach (Edge i in edges)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
            i.getNode1.Position * nodeSize + offset,
            i.getNode2.Position * nodeSize + offset);

            //Draw midpoint
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(
                               (i.getNode1.Position + i.getNode2.Position) / 2 * nodeSize + offset,
                                                         .1f * nodeSize);
        }

        //Draw node points
        if (nodes == null) return;
        Gizmos.color = Color.green;

        foreach (var node in nodes)
        {
            if (node.type == Node.roomType.start)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(
                node.Position * nodeSize + offset,
                           .50f * nodeSize);
            }
            else if (node.type == Node.roomType.boss)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(
                node.Position * nodeSize + offset,
                           .50f * nodeSize);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(
                node.Position * nodeSize + offset,
                           .25f * nodeSize);
            }
            

            //Draw the safe radius
            if (drawFrontier)
            {
                Gizmos.color = Color.red;
                //Draw a rectangle with node.width and node.height
                Gizmos.DrawWireCube(node.Position * nodeSize + offset, new Vector3(node.RoomWidth * nodeSize, node.RoomHeight * nodeSize, 0));
            }

            //Text of node.rank
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 10;
            UnityEditor.Handles.Label(node.Position * nodeSize + offset, node.depth.ToString(), style);

        }

        Gizmos.color = Color.yellow;
        foreach(Vector2 pos in testingPosList)
        {
            Vector3 posFull = new Vector3(pos.x + matrixDisplacementX + 0.5f, pos.y + matrixDisplacementY + 0.5f, 0);
            Gizmos.DrawSphere(posFull, .1f);
        }
    }

    private void DrawTilemap()
    {
        //Draw the tilemap
        if (roomMatrix == null)
        {
            return;
        }

        //Clear the tilemap
        tilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        void PaintMatrix(Tilemap tilemap, RuleTile ruleTile, bool[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j])
                    {
                        Vector3Int pos = new Vector3Int(i + matrixDisplacementX, j + matrixDisplacementY, 0);
                        tilemap.SetTile(pos, ruleTile);
                    }
                }
            }
        }
        (bool[,] wallsTop, bool[,] wallsBottom, bool[,] wallsLeft, bool[,] wallsRight) = matrixDilation.GetWallDilations(roomMatrix);
        PaintMatrix(tilemap, groundTile, roomMatrix);
        PaintMatrix(wallTilemap, wallTile, wallsTop);
        PaintMatrix(wallTilemap, wallTile, wallsBottom);
        PaintMatrix(wallTilemap, wallTile, wallsLeft);
        PaintMatrix(wallTilemap, wallTile, wallsRight);

        //Draw the rooms
        //IGNORE FOR THE MOMENT!!
        //WILL BE USED WHEN ROOM TYPES HAVE DIFFERENT TILES
        //foreach (Node node in nodes)
        //{
        //node.room.PaintWholeRoom(tilemap, wallTilemap, groundTile, wallTile, wallTile, wallTile, wallTile, (int)node.Position.x, (int)node.Position.y);
        //}
    }

    private void drawBoolMap(bool[,] matrix = null, int matrixDisplacementX = 0, int matrixDisplacementY = 0)
    {
        //Draw the tilemap
        if (matrix == null)
        {
            if (roomMatrix == null) return;
            matrix = roomMatrix;
        }
        if (matrixDisplacementX == 0)
        {
            matrixDisplacementX = this.matrixDisplacementX;
        }
        if (matrixDisplacementY == 0)
        {
            matrixDisplacementY = this.matrixDisplacementY;
        }

        //Clear the tilemap
        boolTilemap.ClearAllTiles();

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Vector3Int pos = new Vector3Int(i + matrixDisplacementX, j + matrixDisplacementY, 0);
                RuleTile tile = null;
                switch (matrix[i, j])
                {
                    case true:
                        tile = trueTile;
                        break;
                    case false:
                        tile = emptyTile;
                        break;
                }
                boolTilemap.SetTile(pos, tile);
            }
        }
    }

    //Start is called before the first frame update
    void Start()
    {
        //Initialise the graph
        InitialiseGraph();
    }

    // Update is called once per frame
    void Update()
    {
        //Generate the graph
        if (generateGraph)
        {
            InitialiseGraph();
            generateGraph = false;

        }
    }
}
