using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    //Class to represent an edge in the graph
    private Node node1;
    private Node node2;
    public float weight;
    public float weightMiltiplier = 1;
    //getter for nodes
    public Node getNode1 { get { return node1; } }
    public Node getNode2 { get { return node2; } }

    //Setter for nodes, When setting, change weight
    public Node setNode1 { set { node1 = value; Vector3.Distance(node1.Position, node2.Position); } }
    public Node setNode2 { set { node2 = value; Vector3.Distance(node1.Position, node2.Position); } }

    //Function for comapring edge equality
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        bool result = false;
        if (obj is Edge)
        {
            Edge edge = (Edge)obj;
            result = (node1 == edge.node1 && node2 == edge.node2) || (node1 == edge.node2 && node2 == edge.node1);
        }
        return result;
    }

    public bool Contains(Node node)
    {
        return node1 == node || node2 == node;
    }

    //Initialise the edge
    public Edge(Node node1, Node node2)
    {
        this.node1 = node1;
        this.node2 = node2;
        //Calculate the weight of the edge
        weight = Vector3.Distance(node1.Position, node2.Position);
    }
}
