using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// <remarks>FullExecution() with FrameTime being an average frametime
///  - or - 
/// SingleStepExecution() with FrameTime being Time.deltaTime</remarks>
public class ForceGraph
{
    [System.Serializable]
    public class ForceNode
    {
        public List<ForceNode> ConnectedForceNodes = new List<ForceNode>();
        public List<float> EdgeLengths = new List<float>();
        public Node node;
        public string Key;
        public Vector3 Position;
        public int Id;
        public static int PreviousId = 0;

        public ForceNode(int id, Vector3 position, Node node)
        {
            Id = id;
            Position = position;
            this.node = node;
        }
        
        public void AddConnectedForceNode(ForceNode node, float length) 
        {
            ConnectedForceNodes.Add(node);
            EdgeLengths.Add(length);
        }
        
        public bool IsNode(Node n)
        {
            return (n == this.node);
        }

        public float GetDistanceToNode(ForceNode n)
        {
            if (ConnectedForceNodes.Contains(n))
            {
                return EdgeLengths[ConnectedForceNodes.IndexOf(n)];
            }
            return float.MaxValue;
        }
    }

    public List<ForceNode> ForceNodes = new List<ForceNode>();

    public ForceGraphSettings Settings;
    public Vector3 AnchorPosition = Vector3.zero;

    public void FullExecution()
    {
        for(int i = 0; i < Settings.Iterations; i++)
        {
            SingleStepExecution();
        }
    }

    public void SingleStepExecution()
    {
        foreach (var n1 in ForceNodes)
        {
            ApplyPull(n1, AnchorPosition, n1.Position.magnitude);
            foreach (var n2 in ForceNodes)
            {
                if (n1 == n2) continue;

                var connected = n1.ConnectedForceNodes.Contains(n2);
                var distance = (n1.Position - n2.Position).magnitude;

                if (connected)
                {
                    ApplyPull(n1, n2.Position, distance);

                }

                ApplyPush(n1, n2.Position, distance);

            }
        }
    }

    private void ApplyPush(ForceNode n1, Vector3 toPosition, float distance)
    {
        var diff = n1.Position - toPosition;
        var dir = diff.normalized;
        var force =
            Settings.PushForce
            * dir
            * (1f - (Mathf.Clamp(distance, 0, 1f)));

        n1.Position += force;
    }

    private void ApplyPull(ForceNode n1, Vector3 toPosition, float distance)
    {
        var diff = n1.Position - toPosition;
        var dir = diff.normalized;
        var force =
            Settings.PullForce
            * dir
            * Mathf.Clamp(distance, 0, 1f);

        n1.Position -= force;
    }
}
