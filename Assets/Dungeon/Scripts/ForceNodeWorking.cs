using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// <remarks>FullExecution() with FrameTime being an average frametime
///  - or - 
/// SingleStepExecution() with FrameTime being Time.deltaTime</remarks>
public class ForceGraphWorking
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
        for (int i = 0; i < Settings.Iterations; i++)
        {
            SingleStepExecution();
        }
    }

    public void SingleStepExecution()
    {
        foreach (var n1 in ForceNodes)
        {
            ApplyPullToAnchor(n1, AnchorPosition, n1.Position.magnitude);
            foreach (var n2 in ForceNodes)
            {
                if (n1 == n2) continue;

                var connected = n1.ConnectedForceNodes.Contains(n2);
                var distance = (n1.Position - n2.Position).magnitude;

                SeparateNodes(n1, n2, distance);

            }
        }
    }

    private void ApplyPush(ForceNode n1, ForceNode n2, Vector3 toPosition, float distance)
    {
        var diff = n1.Position - toPosition;
        var dir = diff.normalized;
        var force =
            Settings.PushForce
            * dir
            * (1f - (Mathf.Clamp(distance, 0, 1f)));

        n1.Position += force;
        n1.node.x = n1.Position.x;
        n1.node.y = n1.Position.y;
    }

    private void ApplyPullToAnchor(ForceNode n1, Vector3 toPosition, float distance)
    {
        var diff = n1.Position - toPosition;
        var dir = diff.normalized;
        var force =
            Settings.PullForce
            * dir
            * Mathf.Clamp(distance, 0, 1f);

        n1.Position -= force;
        n1.node.x = n1.Position.x;
        n1.node.y = n1.Position.y;
    }

    private void ApplyPull(ForceNode n1, Vector3 toPosition, float distance, float edgeLength)
    {
        //We need to keep the required distance
        //We pull (or Push) the node towards the other node until the distance is correct
        var diff = n1.Position - toPosition;
        var dir = diff.normalized;
        var desiredPosition = toPosition + dir * edgeLength;
        n1.Position = desiredPosition;
    }

    static void SeparateNodes(ForceNode n1, ForceNode n2, float distance)
    {
        if (distance < n1.node.safeRadius + n2.node.safeRadius)
        {
            float repulsionForce = (n1.node.safeRadius + n2.node.safeRadius - distance) * 0.5f;

            float forceDirectionX = (n1.node.x - n2.node.x) / distance;
            float forceDirectionY = (n1.node.y - n2.node.y) / distance;

            float displacement = repulsionForce / 2;

            n1.Position.x += displacement * forceDirectionX;
            n1.Position.y += displacement * forceDirectionY;
            n2.Position.x -= displacement * forceDirectionX;
            n2.Position.y -= displacement * forceDirectionY;
        }
    }
}
