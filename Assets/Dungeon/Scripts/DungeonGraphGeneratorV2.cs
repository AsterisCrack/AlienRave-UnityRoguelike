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
    private TileType[,] mapMatrix;
    private bool[,] roomMatrix;
    private int matrixDisplacementX;
    private int matrixDisplacementY;

    private void UpdateBoolMatrix()
    {
        //Function to update the bool matrix according to the mapMatrix
        bool[,] newMatrix = new bool[mapMatrix.GetLength(0), mapMatrix.GetLength(1)];
        for (int i = 0; i < mapMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < mapMatrix.GetLength(1); j++)
            {
                //If there is anything but a null in the mapMatrix, we set the bool matrix to true
                if (mapMatrix[i, j] != TileType.empty)
                {
                    newMatrix[i, j] = true;
                }
                else
                {
                    newMatrix[i, j] = false;
                }
            }
        }
        roomMatrix = newMatrix;
    }

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
        if (mapMatrix == null)
        {
            mapMatrix = new TileType[roomWidth, roomHeight];
            //Set all the positions to empty tile 
            for (int i = 0; i < mapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < mapMatrix.GetLength(1); j++)
                {
                    mapMatrix[i, j] = TileType.empty;
                }
            }
        }

        if (centerX + roomWidth / 2 > mapMatrix.GetLength(0) + matrixDisplacementX)
        {
            //We need to add more columns to the right
            numColumnsRigth = (int)Mathf.Ceil(centerX + roomWidth / 2 - matrixDisplacementX - mapMatrix.GetLength(0));
        }   
        if (centerX - roomWidth / 2 < 0 + matrixDisplacementX)
        {
            //We need to add more columns to the left
            numColumnsLeft = matrixDisplacementX + (int)Mathf.Ceil(Mathf.Abs(centerX - roomWidth / 2));
        }
        if (centerY + roomHeight/2 + 1 > mapMatrix.GetLength(1) + matrixDisplacementY)
        {
            //We need to add more rows to the top
            numRowsTop = (int)Mathf.Ceil(centerY + roomHeight / 2 - matrixDisplacementY - mapMatrix.GetLength(1)) + 1;
        }
        if (centerY - roomHeight / 2 < 0 + matrixDisplacementY)
        {
            //We need to add more rows to the bottom
            numRowsBottom = matrixDisplacementY + (int)Mathf.Ceil(Mathf.Abs(centerY - roomHeight / 2));
        }

        //Now we need to create a new matrix with the new size
        TileType[,] newMatrix = new TileType[mapMatrix.GetLength(0) + numColumnsLeft + numColumnsRigth, mapMatrix.GetLength(1) + numRowsTop + numRowsBottom];
        //Set all the positions to empty tile
        for (int i = 0; i < newMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < newMatrix.GetLength(1); j++)
            {
                newMatrix[i, j] = TileType.empty;
            }
        }
        //Now we need to copy the old matrix to the new one
        for (int i = 0; i < mapMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < mapMatrix.GetLength(1); j++)
            {
                newMatrix[i + numColumnsLeft, j + numRowsBottom] = mapMatrix[i, j];
            }
        }

        //Now we need to update the mapMatrix
        mapMatrix = newMatrix;

        //Now we need to update the displacement
        matrixDisplacementX -= numColumnsLeft;
        matrixDisplacementY -= numRowsBottom;

        //Now we update the matrix by adding the new room
        (bool[,] groundMatrix, bool[,] leftWallMatrix, bool[,] rightWallMatrix, bool[,] topWallMatrix, bool[,] bottomWallMatrix) = room.GetRoomMatrix();
        //Stablish the starting points of the room
        int groundStartingPosX = centerX - groundMatrix.GetLength(0) / 2 - matrixDisplacementX;
        int groundStartingPosY = centerY - groundMatrix.GetLength(1) / 2 - matrixDisplacementY;

        int wallStartingPosX = centerX - leftWallMatrix.GetLength(0) / 2 - matrixDisplacementX;
        int wallStartingPosY = centerY - leftWallMatrix.GetLength(1) / 2 - matrixDisplacementY;

        //Now we need to update the mapMatrix
        MergeMatrix(mapMatrix, groundMatrix, TileType.Ground, groundStartingPosX, groundStartingPosY);
        MergeMatrix(mapMatrix, leftWallMatrix, TileType.leftWall, wallStartingPosX, wallStartingPosY);
        MergeMatrix(mapMatrix, rightWallMatrix, TileType.rightWall, wallStartingPosX, wallStartingPosY);
        MergeMatrix(mapMatrix, topWallMatrix, TileType.topWall, wallStartingPosX, wallStartingPosY);
        MergeMatrix(mapMatrix, bottomWallMatrix, TileType.bottomWall, wallStartingPosX, wallStartingPosY);

        //Update the bool matrix
        UpdateBoolMatrix();
    }

    private void MergeMatrix(TileType[,] map, bool[,] matrix, TileType tiletype, int centerX, int centerY)
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
                            map[i + centerX, j + centerY] = tiletype;
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

    //Initialise the graph
    public void InitialiseGraph()
    {
        //Empty the lists
        nodes = new List<Node>();
        edges = new List<Edge>();
        mapMatrix = null;
        roomMatrix = null;
        matrixDisplacementX = 0;
        matrixDisplacementY = 0;

        //Generate the nodes
        nodes = GenerateNodes();
        //Generate the edges
        edges = GenerateEdges();
        GenerateHallways();

        //Print number of nodes and edges and the number of nodes in the force graph
        Debug.Log("Nodes: " + nodes.Count + " Edges: " + edges.Count);

        if (drawTilemap)
        {
            //drawBoolMap();
            DrawTilemap();
        }
    }

    //First generate the nodes randomly
    private List<Node> GenerateNodes()
    {
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
        nodes.Add(new Node(0, 0, 0, roomWidth, roomHeight));

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
            drawBoolMap(frontierMatrix, matrixDisplacementX - spaceX - 2, matrixDisplacementY - spaceY - 2);
            //Now we have the frontier, we need to find a random point in the frontier to place the room
            //We will do it by creating a list of all the points in the frontier
            List<Vector2> frontierList = GetFrontierList(frontierMatrix, matrixDisplacementX - spaceX - 2, matrixDisplacementY - spaceY - 2);

            //Now we need to select a random point in the list
            Vector2 roomPos = frontierList[UnityEngine.Random.Range(0, frontierList.Count)];

            //Now we need to add the room to the mapMatrix
            AddRoomToMatrix(roomPos.x, roomPos.y, roomWidth, roomHeight, roomInstance);

            //Now we need to add the node to the list
            nodes.Add(new Node(i, (int)roomPos.x, (int)roomPos.y, roomWidth, roomHeight));
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

    public class NodeRanges
    {
        public int top = 0;
        public int bottom = 0;
        public int left = 0;
        public int right = 0;
        public NodeRanges(Node n, int displacementX = 0, int displacementY = 0)
        {
            top = (int)Mathf.Ceil(n.Position.y + n.RoomHeight / 2 - 1 - displacementY);
            bottom = (int)Mathf.Ceil(n.Position.y - n.RoomHeight / 2 + 1 - displacementY);
            left = (int)Mathf.Ceil(n.Position.x - n.RoomWidth / 2 + 1 - displacementX);
            right = (int)Mathf.Ceil(n.Position.x + n.RoomWidth / 2 - 1 - displacementX);
        }
    }

    private void GenerateHallways()
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
                                if (mapMatrix[p + j, i] == TileType.Ground) possible = false;
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
                                if (mapMatrix[i, p + j] == TileType.Ground) possible = false;
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
                if (horizontal) { 
                    for (int i = hallwayBottom + hallwayWidth; i < hallwayTop; i++)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            mapMatrix[hallwayPos + j, i] = TileType.Ground;
                        }
                        //and walls
                        mapMatrix[hallwayPos - 1, i] = TileType.leftWall;
                        mapMatrix[hallwayPos + hallwayWidth, i] = TileType.rightWall;
                    }
                }
                else
                {
                    for (int i = hallwayLeft + hallwayWidth; i < hallwayRight; i++)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            mapMatrix[i, hallwayPos + j] = TileType.Ground;
                        }
                        //and walls
                        mapMatrix[i, hallwayPos - 1] = TileType.bottomWall;
                        mapMatrix[i, hallwayPos + hallwayWidth] = TileType.topWall;
                        mapMatrix[i, hallwayPos + hallwayWidth + 1] = TileType.topWall;
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
                            if (mapMatrix[i, j] != TileType.empty)// && mapMatrix[i, j] != TileType.rightWall && mapMatrix[i, j-2] != TileType.Ground
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
                            if (mapMatrix[j, i] != TileType.empty) // && mapMatrix[j, i] != TileType.rightWall && mapMatrix[j, i-2] != TileType.Ground
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
                bool[,] corridors = new bool[mapMatrix.GetLength(0), mapMatrix.GetLength(1)];

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
                    int yStart = (selectedCorner[0][s1] == posBottom) ? (int)posBottom.y - 2 : (int)posBottom.y - 1;
                    for (int i = yStart; i < posTop.y + 2; i++)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            mapMatrix[start + j, i] = TileType.Ground;
                        }
                        //and walls
                        mapMatrix[start - 1, i] = TileType.leftWall;
                        mapMatrix[start + hallwayWidth, i] = TileType.rightWall;
                    }

                    //Horizontal part. Starts on s2
                    start = (int)selectedCorner[1][s2].y;
                    int xStart = (selectedCorner[1][s2] == posLeft) ? (int)posLeft.x - 1 : (int)posLeft.x;
                    //Here we have to take cate so that it doesnt intersect with the vertical part
                    int topWallLen = selectedCorner[1][s2] == posTop ? (int)posRight.x + hallwayWidth - xStart : (int)posRight.x - 1 - xStart;
                    int bottomWallLen = selectedCorner[1][s2] == posBottom ? (int)posRight.x + hallwayWidth - xStart : (int)posRight.x - 1 - xStart;
                    int topWallStart = selectedCorner[1][s2] == posLeft ? xStart : selectedCorner[1][s2] == posBottom ? xStart + hallwayWidth : xStart - 1;
                    int bottomWallStart = selectedCorner[1][s2] == posLeft ? xStart : selectedCorner[1][s2] == posBottom ? xStart -1 : xStart + hallwayWidth;

                    for (int i = xStart; i < posRight.x + 1; i++)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            mapMatrix[i, start + j] = TileType.Ground;
                        }
                    }
                    if (selectedCorner[1][s2] == posRight)
                    {
                        for (int j = 0; j < hallwayWidth; j++)
                        {
                            mapMatrix[(int)posRight.x + 1, start + j] = TileType.Ground;
                        }
                    }

                    //and walls
                    for (int i = 0; i <= topWallLen; i++)
                    {
                        mapMatrix[topWallStart + i, start + hallwayWidth] = TileType.topWall;
                        mapMatrix[topWallStart + i, start + hallwayWidth+1] = TileType.topWall;
                    }
                    for (int i = 0; i <= bottomWallLen; i++)
                    {
                        mapMatrix[bottomWallStart + i, start - 1] = TileType.bottomWall;
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
            InitialiseGraph();
        }
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
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(
                node.Position * nodeSize + offset,
                           .25f * nodeSize);

            //Draw the safe radius
            if (drawFrontier)
            {
                Gizmos.color = Color.red;
                //Draw a rectangle with node.width and node.height
                Gizmos.DrawWireCube(node.Position * nodeSize + offset, new Vector3(node.RoomWidth * nodeSize, node.RoomHeight * nodeSize, 0));
            }
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
        if (mapMatrix == null)
        {
            return;
        }

        //Clear the tilemap
        tilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int i = 0; i < mapMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < mapMatrix.GetLength(1); j++)
            {
                Vector3Int pos = new Vector3Int(i+matrixDisplacementX, j + matrixDisplacementY, 0);
                RuleTile tile = null;
                switch (mapMatrix[i, j])
                {
                    case TileType.Ground:
                        tile = groundTile;
                        break;
                    case TileType.leftWall:
                        tile = wallTile;
                        break;
                    case TileType.rightWall:
                        tile = wallTile;
                        break;
                    case TileType.topWall:
                        tile = wallTile;
                        break;
                    case TileType.bottomWall:
                        tile = wallTile;
                        break;
                    case TileType.empty:
                        tile = emptyTile;
                        break;
                    default:
                        break;
                }
                if (tile == groundTile)
                {
                    tilemap.SetTile(pos, tile);
                }
                else
                {
                    wallTilemap.SetTile(pos, tile);
                }
            }
        }
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
