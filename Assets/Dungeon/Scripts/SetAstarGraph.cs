using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAstarGraph : MonoBehaviour
{
    //This script will set the Astar graph to the correct size and scan it
    //This script is attached to the DungeonGenerator object
    //This script is called from the DungeonGenerator script when the dungeon is finished generating

    //The AstarPath script is a singleton, so we can access it from anywhere
    private AstarPath pathfinder;

    public void SetGraph(int width, int height, float centerX, float centerY)
    {
        //Get the AstarPath script
        pathfinder = AstarPath.active;

        //Check if pathfinder exists
        if (pathfinder == null || pathfinder.data == null || pathfinder.data.gridGraph == null)
        {
            return;
        }

        //Set the graph size to the width and height of the dungeon
        pathfinder.data.gridGraph.SetDimensions(width, height, 1);

        //Set the center of the graph to the center of the dungeon
        pathfinder.data.gridGraph.center = new Vector3(centerX, centerY, 0);

        //Scan the graph
        StartCoroutine(ScanGraph());
    }

    private IEnumerator ScanGraph()
    {
        //Scan the graph
        //Wait for a bit to scan
        //This is because the displacement of the A star graph seems to be done in a coroutine and it can take a bit
        yield return new WaitForSeconds(0.5f);
        AstarPath.active.Scan();
    }
}
