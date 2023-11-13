using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class DungeonGraphGenerator : MonoBehaviour
{
    //Script to generate a random graph of the dungeon to be built
    [Header("Editor settings")]
    //Button to generate the graph
    [SerializeField] private bool generateGraph;
    [SerializeField] private bool applyForce;

    [Header("Dungeon settings")]
    [SerializeField] private int minRooms;
    [SerializeField] private int maxRooms;
    [SerializeField] private float minEdgesMultiplier;
    [SerializeField] private float maxEdgesMultiplier;

    [Header("Force graph settings")]
    public float PushForce = .01f;
    public float PullForce = .01f;
    public int Iterations = 100;
    public AnimationCurve BlendingOverDistance;

    [Header("Room settings")]
    [SerializeField] private int minRoomSize;
    [SerializeField] private int maxRoomSize;

    [Header("Corridor settings")]
    [SerializeField] private int minCorridorMultiplier;
    [SerializeField] private int maxCorridorMultiplier;

    [Header("Visualization settings")]
    [SerializeField] private bool visualizeGraph;
    [SerializeField] private float nodeSize;
    [SerializeField] private bool drawRadius;
    [SerializeField] private bool executeInstantly;
    [SerializeField] private float frameTime = .01f;

    private List<Node> nodes;
    private List<Edge> edges;
    private ForceGraph forceGraph;

    //Initialise the graph
    public void InitialiseGraph()
    {
        //Empty the lists
        nodes = new List<Node>();
        edges = new List<Edge>();

        //Generate the nodes
        nodes = GenerateNodes();
        //Generate the edges
        edges = GenerateEdges(nodes);

        //Build the force graph
        forceGraph = BuildForceGraph();

        //Print number of nodes and edges and the number of nodes in the force graph
        Debug.Log("Nodes: " + nodes.Count + " Edges: " + edges.Count + " Force nodes: " + forceGraph.ForceNodes.Count);
        
    }

    //First generate the nodes randomly
    private List<Node> GenerateNodes()
    {
        var nodes = new List<Node>();
        //Generate a random number of rooms
        int numRooms = Random.Range(minRooms, maxRooms);
        //Generate the rooms
        for (int i = 0; i < numRooms; i++)
        {
            //Generate a random room size
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize);
            //Generate a random position for the room
            int x = Random.Range(0, 10);
            int y = Random.Range(0, 10);
            //Create the room
            nodes.Add(new Node(i, x, y, roomWidth, roomHeight));
        }
        return nodes;
    }

    //Then generate the edges
    private List<Edge> GenerateEdges(List<Node> nodes)
    {
        var edges = new List<Edge>();
        //Generate a random number of edges
        int numEdges = (int)Random.Range(nodes.Count * minEdgesMultiplier, nodes.Count * maxEdgesMultiplier);
        //Confirm that the number of edges is not greater than the number of possible edges
        numEdges = Mathf.Min(numEdges, nodes.Count * (nodes.Count - 1) / 2);
        //Confirm that there are enough edges to connect all the nodes
        numEdges = Mathf.Max(numEdges, nodes.Count - 1);

        List<Edge> allPossibleEdges = new List<Edge>();
        //Generate all the possible edges
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                float multiplier = Random.Range(minCorridorMultiplier, maxCorridorMultiplier);
                allPossibleEdges.Add(new Edge(nodes[i], nodes[j]));
            }
        }

        //First, we generate edges to connect all the nodes
        for (int i = 1; i < nodes.Count; i++)
        {
            int j = Random.Range(0, i);
            while (i == j)
            {
                j = Random.Range(0, i);
            }
            float multiplier = Random.Range(minCorridorMultiplier, maxCorridorMultiplier);
            Edge edge = new Edge(nodes[i], nodes[j]);
            edges.Add(edge);
            nodes[i].AddNeighbour(nodes[j]);
            nodes[j].AddNeighbour(nodes[i]);
            allPossibleEdges.Remove(edge);
        }

        //Generate the random edges
        for (int i = 0; i < numEdges - edges.Count; i++)
        {
            int e = Random.Range(0, allPossibleEdges.Count);
            Edge edge = allPossibleEdges[e];
            edges.Add(edge);
            edge.getNode1.AddNeighbour(edge.getNode2);
            edge.getNode2.AddNeighbour(edge.getNode1);
            allPossibleEdges.Remove(edge);
        }
        return edges;
    }

    private Edge GetEdge(Node n1, Node n2)
    {
           return edges.Find(x => x.Contains(n1) && x.Contains(n2));
    }

    //Now we build the force graph
    private ForceGraph BuildForceGraph()
    {
        //Create the force graph
        ForceGraph forceGraph = new ForceGraph();
        //Apply settings
        forceGraph.Settings = new ForceGraphSettings();
        forceGraph.Settings.PullForce = PullForce;
        forceGraph.Settings.PushForce = PushForce;
        forceGraph.Settings.Iterations = Iterations;
        forceGraph.Settings.BlendingOverDistance = BlendingOverDistance;
        
        //Add the nodes to the force graph
        foreach (Node node in nodes)
        {
            int id = node.id;
            Vector3 position = node.Position;
            ForceGraph.ForceNode forceGraphNode = new ForceGraph.ForceNode(id, position, node);
            forceGraph.ForceNodes.Add(forceGraphNode);
        }
        //Add the edges to the force graph
        foreach (Node n in nodes)
        {
            ForceGraph.ForceNode forceNode = forceGraph.ForceNodes.Find(x => x.IsNode(n));
            foreach (Node neighbour in n.neighbours)
            {
                Edge edge = GetEdge(n, neighbour);
                ForceGraph.ForceNode forceNeighbour = forceGraph.ForceNodes.Find(x => x.IsNode(neighbour));
                forceNode.AddConnectedForceNode(forceNeighbour, edge.weight);
            }
        }
        return forceGraph;
    }

    private IEnumerator ExecuteForceGraph(ForceGraph forceGraph)
    {
        Debug.Log("Force graph executing");
        for (int i = 0; i < forceGraph.Settings.Iterations; i++)
        {
            forceGraph.SingleStepExecution();
            Debug.Log("Iteration " + i + " of " + forceGraph.Settings.Iterations);
            yield return new WaitForSeconds(frameTime);
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
            Gizmos.DrawLine(
            i.getNode1.Position * nodeSize + offset,
            i.getNode2.Position * nodeSize + offset);
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
            if (drawRadius)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(
                    node.Position * nodeSize + offset,
                                       node.safeRadius * nodeSize);
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
        //Apply the force
        if (applyForce)
        {
            //Stop the coroutine if it is already running
            StopAllCoroutines();
            if (executeInstantly)
            {
                forceGraph.FullExecution();
            }
            else
            {
                StartCoroutine(ExecuteForceGraph(forceGraph));
            }
            applyForce = false;
        }
    }
}

/*
 * else
            {
                //The hallway is L shaped
                //Since we dont know the orientation of the hallway, we will generate a random orientation, and if it does not fit, we will try the other one
                bool DrawLShapedHallway(bool right)
                {
                    //Get the possible range for the hallway, wich depend on the orientation
                    int hallwayRight= 0;
                    int hallwayLeft = 0;
                    int hallwayBottom = 0;
                    int hallwayTop = 0;
                    if (right)
                    {
                        hallwayRight = Mathf.Max(n1Right, n2Right) - matrixDisplacementX;
                        hallwayLeft = Mathf.Min(n1Right, n2Right) - matrixDisplacementX;
                        hallwayBottom = Mathf.Min(n1Top, n2Top) - matrixDisplacementY;
                        hallwayTop = Mathf.Max(n1Top, n2Top) - matrixDisplacementY;
                    }
                    else
                    {
                        hallwayRight = Mathf.Max(n1Left, n2Left) - matrixDisplacementX;
                        hallwayLeft = Mathf.Min(n1Left, n2Left) - matrixDisplacementX;
                        hallwayBottom = Mathf.Min(n1Bottom, n2Bottom) - matrixDisplacementY;
                        hallwayTop = Mathf.Max(n1Bottom, n2Bottom) - matrixDisplacementY;
                    }

                    //Generate a random position for the hallway
                    Vector2Int hallwayIntersection = new Vector2Int(UnityEngine.Random.Range(hallwayLeft, hallwayRight), UnityEngine.Random.Range(hallwayBottom, hallwayTop));
                    
                    //Now we need to check if the hallway fits
                    bool fits = true;
                    if (right)
                    {
                        //Check if the hallway fits
                        for (int i = hallwayLeft; i < hallwayIntersection.x; i++)
                        {
                            for (int j = 0; j < hallwayWidth; j++)
                            {
                                if (mapMatrix[i, hallwayIntersection.y + j] != TileType.empty)
                                {
                                    fits = false;
                                }
                            }
                        }
                        for (int i = hallwayIntersection.y; i < hallwayTop; i++)
                        {
                            for (int j = 0; j < hallwayWidth; j++)
                            {
                                if (mapMatrix[hallwayIntersection.x + j, i] != TileType.empty)
                                {
                                    fits = false;
                                }
                            }   
                        }
                    }

                    else
                    {
                        //check if the hallway fits
                        for (int i = hallwayIntersection.x; i < hallwayRight; i++)
                        {
                            for (int j = 0; j < hallwayWidth; j++)
                            {
                                if (mapMatrix[i, hallwayIntersection.y + j] != TileType.empty)
                                {
                                    fits = false;
                                }
                            }
                        }
                        for (int i = hallwayBottom; i < hallwayIntersection.y; i++)
                        {
                            for (int j = 0; j < hallwayWidth; j++)
                            {
                                if (mapMatrix[hallwayIntersection.x + j, i] != TileType.empty)
                                {
                                    fits = false;
                                }
                            }
                        }
                    }

                    //If it fits we can draw the hallway
                    if (fits)
                    {
                        if (right)
                        {
                            //Check if the hallway fits
                            for (int i = hallwayLeft; i < hallwayIntersection.x; i++)
                            {
                                for (int j = 0; j < hallwayWidth; j++)
                                {
                                    mapMatrix[i, hallwayIntersection.y + j] = TileType.Ground;
                                }
                                //and walls
                                mapMatrix[hallwayIntersection.x - 1, i] = TileType.leftWall;
                                mapMatrix[hallwayIntersection.x + hallwayWidth, i] = TileType.rightWall;
                            }
                            for (int i = hallwayIntersection.y; i < hallwayTop; i++)
                            {
                                for (int j = 0; j < hallwayWidth; j++)
                                {
                                    mapMatrix[hallwayIntersection.x + j, i] = TileType.Ground;
                                }
                                //and walls
                                mapMatrix[hallwayIntersection.x - 1, i] = TileType.leftWall;
                                mapMatrix[hallwayIntersection.x + hallwayWidth, i] = TileType.rightWall;
                            }
                        }

                        else
                        {
                            //check if the hallway fits
                            for (int i = hallwayIntersection.x; i < hallwayRight; i++)
                            {
                                for (int j = 0; j < hallwayWidth; j++)
                                {
                                    mapMatrix[i, hallwayIntersection.y + j] = TileType.Ground;
                                }
                                //and walls
                                mapMatrix[i, hallwayIntersection.y - 1] = TileType.bottomWall;
                                mapMatrix[i, hallwayIntersection.y + hallwayWidth] = TileType.topWall;
                            }
                            for (int i = hallwayBottom; i < hallwayIntersection.y; i++)
                            {
                                for (int j = 0; j < hallwayWidth; j++)
                                {
                                    mapMatrix[hallwayIntersection.x + j, i] = TileType.Ground;
                                }
                                //and walls
                                mapMatrix[hallwayIntersection.x - 1, i] = TileType.leftWall;
                                mapMatrix[hallwayIntersection.x + hallwayWidth, i] = TileType.rightWall;
                            }
                        }
                    }
                    return fits;
                }
                bool orientation = UnityEngine.Random.Range(0, 2) == 0;
                bool fits = DrawLShapedHallway(orientation);
                
                if (!fits)
                {
                    //The hallway does not fit, we need to try the other orientation
                    DrawLShapedHallway(!orientation);
                }
            }
*/