using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    [SerializeField] private DungeonGraphGeneratorV2 dungeonGenerator;
    [SerializeField] private GameObject player;

    private void OnDungeonFinished(Vector2 pos)
    {
        player.transform.position = pos;
        player.SetActive(true);
    }

    // Start is called before the first frame update
    void Awake()
    {
        dungeonGenerator.OnFinished += OnDungeonFinished;
    }
}
