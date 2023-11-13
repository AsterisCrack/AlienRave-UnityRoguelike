using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIAmmoCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoCounter;
    [SerializeField] private TextMeshProUGUI clipcounter;
    public static UIAmmoCounter instance;
    private void Awake()
    {
        instance = this;
    }
    public void SetAmmoCounter(int ammo)
    {
        ammoCounter.text = "Ammo: " + ammo.ToString();
    }
    public void SetClipCounter(int clip)
    {
        clipcounter.text = "Clip: " + clip.ToString();
    }
}
