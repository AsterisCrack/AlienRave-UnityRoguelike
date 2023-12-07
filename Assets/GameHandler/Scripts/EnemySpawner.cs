using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnCircle;
    [SerializeField] private float spawnTime = 1;

    [Header("Enemies")]
    [SerializeField] private List<GameObject> enemiesDepth1;
    [SerializeField] private List<GameObject> enemiesDepth2;
    [SerializeField] private List<GameObject> enemiesDepth3;
    [SerializeField] private List<GameObject> enemiesDepth4;
    [SerializeField] private List<GameObject> enemiesDepth5;
    [SerializeField] private List<GameObject> enemiesDepth6;
    [SerializeField] private List<GameObject> enemiesDepth7;
    [SerializeField] private List<GameObject> enemiesDepth8;
    [SerializeField] private List<GameObject> enemiesDepth9;
    [SerializeField] private List<GameObject> enemiesDepth10;
    [SerializeField] private List<GameObject> bosses;

    [Header("Enemy counts")]
    [SerializeField] private Vector2 enemyCountPer100m2;
    [SerializeField] private int enemyCountDepth1;
    [SerializeField] private int enemyCountDepth2;
    [SerializeField] private int enemyCountDepth3;
    [SerializeField] private int enemyCountDepth4;
    [SerializeField] private int enemyCountDepth5;
    [SerializeField] private int enemyCountDepth6;
    [SerializeField] private int enemyCountDepth7;
    [SerializeField] private int enemyCountDepth8;
    [SerializeField] private int enemyCountDepth9;
    [SerializeField] private int enemyCountDepth10;

    public static EnemySpawner instance;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator Spawn(Vector2 centerPos, int width, int height, int depth)
    {
        int area = (width+4) * (height+4);
        List<GameObject> enemies = GetEnemies(depth);
        int enemyCountMultiplier = GetEnemyCount(depth);
        int enemyCount = (int)(area / 100 * Random.Range(enemyCountPer100m2.x, enemyCountPer100m2.y) * enemyCountMultiplier);
        List<Vector2> spawnPositions = new List<Vector2>();
        List<GameObject> spawnCircles = new List<GameObject>();
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 spawnPos = new Vector2(Random.Range(centerPos.x - width / 2, centerPos.x + width / 2), Random.Range(centerPos.y - height / 2, centerPos.y + height / 2));
            spawnPositions.Add(spawnPos);
            GameObject spawnCircleInstance = Instantiate(spawnCircle, spawnPos, Quaternion.identity);
            //Start at scale 0, we will increase it later
            spawnCircleInstance.transform.localScale = Vector3.zero;
            spawnCircles.Add(spawnCircleInstance);
        }

        //Increase the scale of the spawn circles until it reaches 1 at the end of the coroutine
        for (float i = 0; i < 1; i += Time.deltaTime / spawnTime)
        {
            foreach (GameObject spawnCircleInstance in spawnCircles)
            {
                spawnCircleInstance.transform.localScale = new Vector3(i, i, 0);
            }
            yield return null;
        }

        //Spawn the enemies
        for (int i = 0; i < enemyCount; i++)
        {
            Destroy(spawnCircles[i]);
            Instantiate(enemies[Random.Range(0, enemies.Count)], spawnPositions[i], Quaternion.identity);
        }
    }  
    
    public IEnumerator SpawnBoss(Vector2 centerPos)
    {
        //Only spawn 1 boss
        GameObject spawnCircleInstance = Instantiate(spawnCircle, centerPos, Quaternion.identity);

        //Start at scale 0, we will increase it later
        spawnCircleInstance.transform.localScale = Vector3.zero;

        //Increase the scale of the spawn circles until it reaches 2 at the end of the coroutine (It is bigger because it is a boss)
        for (float i = 0; i < 2; i += Time.deltaTime / (spawnTime*2))
        {
            spawnCircleInstance.transform.localScale = new Vector3(i, i, 0);
            yield return null;
        }

        //Spawn the boss
        Destroy(spawnCircleInstance);
        Instantiate(bosses[Random.Range(0, bosses.Count)], centerPos, Quaternion.identity);
    }

    private List<GameObject> GetEnemies(int depth)
    {
        switch (depth)
        {
            case 1:
                return enemiesDepth1;
            case 2:
                return enemiesDepth2;
            case 3:
                return enemiesDepth3;
            case 4:
                return enemiesDepth4;
            case 5:
                return enemiesDepth5;
            case 6:
                return enemiesDepth6;
            case 7:
                return enemiesDepth7;
            case 8:
                return enemiesDepth8;
            case 9:
                return enemiesDepth9;
            case 10:
                return enemiesDepth10;
            default:
                return enemiesDepth10;
        }
    }

    public int GetEnemyCount(int depth)
    {
        switch (depth)
        {
            case 1:
                return enemyCountDepth1;
            case 2:
                return enemyCountDepth2;
            case 3:
                return enemyCountDepth3;
            case 4:
                return enemyCountDepth4;
            case 5:
                return enemyCountDepth5;
            case 6:
                return enemyCountDepth6;
            case 7:
                return enemyCountDepth7;
            case 8:
                return enemyCountDepth8;
            case 9:
                return enemyCountDepth9;
            case 10:
                return enemyCountDepth10;
            default:
                return enemyCountDepth10;
        }
    }
}
