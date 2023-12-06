using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private float distanceBetweenHearts = 15f;
    private int currentHealth;
    private List<GameObject> hearts = new List<GameObject>();

    //This is a singleton
    public static HeartUIHandler instance;

    private void Awake()
    {
        instance = this;
    }

    public void SetHearts(int newHealth)
    {
        currentHealth = newHealth;
        for (int i = 0; i < newHealth; i++)
        {
            Vector2 newPos = new Vector2(transform.position.x + i * distanceBetweenHearts, transform.position.y);
            GameObject heart = Instantiate(heartPrefab, newPos, Quaternion.identity, transform);
            hearts.Add(heart);
        }
    }

    public void DecreaseHearts()
    {
        currentHealth--;
        Destroy(hearts[currentHealth]);
    }

    public void IncreaseHearts()
    {
        currentHealth++;
        //Place the heart at the beggining of the list
        //we need to do this so that the hearts with no health are at the end of the list
        foreach (GameObject heart in hearts)
        {
            heart.transform.position = new Vector2(heart.transform.position.x - distanceBetweenHearts, heart.transform.position.y);
        }
        
        GameObject newHeart = Instantiate(heartPrefab, transform.position, Quaternion.identity, transform);
        hearts.Insert(0, newHeart);
    }

    public bool LowerHealth()
    {
        int thisHealth = hearts[currentHealth - 1].GetComponent<HeartdisplayChange>().LowerHealth();
        if (thisHealth == 1)
        {
            currentHealth--;
        }
        return thisHealth == 1;
    }
    public void IncreaseHealth() {
        hearts[currentHealth - 1].GetComponent<HeartdisplayChange>().IncreaseHealth();
    }
}
