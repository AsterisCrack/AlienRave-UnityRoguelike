using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    [SerializeField] private DungeonGraphGeneratorV2 dungeonGenerator;
    [SerializeField] private GameObject player;
    private GameObject camera;

    private void OnDungeonFinished(Vector2 pos)
    {
        camera.transform.position = new Vector3(pos.x, pos.y, -10);
        player.transform.position = pos;
        player.SetActive(true);
    }

    // Start is called before the first frame update
    void Awake()
    {
        camera = GameObject.Find("Main Camera");
        dungeonGenerator.OnFinished += OnDungeonFinished;
    }
}
