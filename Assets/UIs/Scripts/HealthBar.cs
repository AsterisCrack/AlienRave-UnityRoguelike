using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        gameObject.SetActive(false);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    public void SetMinHealth(int health)
    {
        slider.minValue = health;
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }
}
