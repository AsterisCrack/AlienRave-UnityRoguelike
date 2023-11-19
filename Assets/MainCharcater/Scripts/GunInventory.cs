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
        
    }

    private void SwithGun(int gunIndex)
    {
        guns[currentGun].SetActive(false);
        currentGun = gunIndex;
        guns[currentGun].SetActive(true);
        changeGunDisplay.ChangeGunDisplayImage(guns[currentGun].GetComponent<SpriteRenderer>().sprite);
    }
}
