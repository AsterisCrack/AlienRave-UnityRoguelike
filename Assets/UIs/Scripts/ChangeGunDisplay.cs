using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeGunDisplay : MonoBehaviour
{
    [SerializeField] private Image gunUIShowcase;

    public static ChangeGunDisplay instance;
    private void Awake()
    {
        instance = this;
    }

    public void ChangeGunDisplayImage(Sprite gunSprite)
    {
        gunUIShowcase.sprite = gunSprite;
    }
}
