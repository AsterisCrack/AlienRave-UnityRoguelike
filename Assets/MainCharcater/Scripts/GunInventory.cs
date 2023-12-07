using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GunInventory : MonoBehaviour
{
    [SerializeField] private GameObject[] guns;
    [SerializeField] private int currentGun = 0;
    [SerializeField] private float delayBetweenSwitches = 0.2f;
    private float lastSwitchTime = 0f;

    private PlayerInput playerInput;
    private InputAction switchGunUp;
    private InputAction switchGunDown;

    private ChangeGunDisplay changeGunDisplay;

    public GameObject CurrentGun { get { return guns[currentGun]; } }

    // Start is called before the first frame update
    void Start()
    {
        changeGunDisplay = ChangeGunDisplay.instance;
        playerInput = GetComponent<PlayerInput>();
        switchGunUp = playerInput.actions["GunChangeUp"];
        switchGunDown = playerInput.actions["GunChangeDown"];

        SwithGun(currentGun);
    }

    // Update is called once per frame
    void Update()
    {
        //Check if the player wants to switch gun
        if ((switchGunUp.WasPerformedThisFrame() && switchGunDown.WasPerformedThisFrame()) || Time.time - lastSwitchTime < delayBetweenSwitches)
        {
            return;
        }
        else if (switchGunUp.WasPerformedThisFrame() && guns.Length > 1)
        {
            lastSwitchTime = Time.time;
            if (currentGun == guns.Length - 1)
            {
                SwithGun(0);
            }
            else
            {
                SwithGun(currentGun + 1);
            }
        }
        else if (switchGunDown.WasPerformedThisFrame() && guns.Length > 1)
        {
            lastSwitchTime = Time.time;
            if (currentGun == 0)
            {
                SwithGun(guns.Length - 1);
            }
            else
            {
                SwithGun(currentGun - 1);
            }
        }
    }

    private void SwithGun(int gunIndex, bool hidePreviuos = true)
    {
        if(hidePreviuos) guns[currentGun].SetActive(false);
        
        currentGun = gunIndex;
        guns[currentGun].SetActive(true);
        changeGunDisplay.ChangeGunDisplayImage(guns[currentGun].GetComponent<SpriteRenderer>().sprite);
    }

    public void AddGunToInventory(GameObject gun)
    {
        //Check if the gun is already in the inventory
        foreach(GameObject g in guns)
        {
            if(gun.name == g.name)
            {
                //Recharge ammo and delete the gun
                g.GetComponentInChildren<AdvancedBulletEmmiter>().RechargeAmmo();
                Destroy(gun);
                return;
            }
        }

        //Add the gun to the inventory
        GameObject[] newGuns = new GameObject[guns.Length + 1];
        for(int i = 0; i < guns.Length; i++)
        {
            newGuns[i] = guns[i];
        }
        newGuns[guns.Length] = gun;
        guns = newGuns;

        //Switch the gun to the new gun
        SwithGun(guns.Length - 1);
    }

    public void DeleteGunFromInventory(GameObject gun)
    {
        //Check if the gun is in the inventory
        bool gunFound = false;
        foreach (GameObject g in guns)
        {
            if (gun.name == g.name)
            {
                gunFound = true;
                break;
            }
        }
        if (!gunFound)
        {
            return;
        }
        bool hasToSwitch = false;
        //Check if the gun is the current gun
        if(gun.name == guns[currentGun].name)
        {
            hasToSwitch= true;
        }
        //Delete the gun from the inventory
        GameObject[] newGuns = new GameObject[guns.Length - 1];
        int j = 0;
        for (int i = 0; i < guns.Length; i++)
        {
            if(gun.name != guns[i].name)
            {
                newGuns[j] = guns[i];
                j++;
            }
        }
        guns = newGuns;
        if (hasToSwitch)
        {
            //Switch the gun to the previous gun
            if (currentGun == guns.Length)
            {
                SwithGun(0, false);
            }
            else
            {
                SwithGun(currentGun, false);
            }
        }
    }
}
