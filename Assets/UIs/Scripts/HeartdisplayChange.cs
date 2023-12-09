using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartdisplayChange : MonoBehaviour
{
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite emptyHeart;
    private int health = 3;
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void ChangeHealth(int health)
    {
        if (health == 3)
        {
            image.sprite = fullHeart;
        }
        else if (health == 2)
        {
            image.sprite = halfHeart;
        }
        else if (health == 1)
        {
            image.sprite = emptyHeart;
        }
    }

    public int LowerHealth()
    {
        health--;
        ChangeHealth(health);
        return health;
    }

    public int IncreaseHealth()
    {
        if (health != 3)
        {
            health++;
            ChangeHealth(health); 
        }
        return health;
    }
}
