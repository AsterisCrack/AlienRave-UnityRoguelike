using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PickaableGun : MonoBehaviour
{
    //A serialized field transform to store the local transform when the gun is picked up
    [Header("Gun Local Transform when picked up")]
    [SerializeField] private Vector3 localPosition;
    [SerializeField] private Vector3 localRotation;
    [SerializeField] private Vector3 localScale;

    [Header("Layers")]
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private LayerMask pickedGunLayer;
    [SerializeField] private int itemViewLayer;
    [SerializeField] private int pickedGunViewLayer;

    [Header("Pick up")]
    [SerializeField] private float maxDistance;

    [Header("Drop")]
    [SerializeField] private float dropForce = 5f;

    private GameObject player;
    private PlayerInput playerInput;
    private InputAction pickAction;
    private InputAction dropAction;
    private GunInventory gunInventory;

    private Collider2D gunCollider;
    private bool isPickedUp;
    private GameObject bulletEmitter;
    private AdvancedBulletEmmiter bulletEmitterScript;
    private GunMovement gunMovementScript;

    private int GetLayerNumberFromMask(LayerMask mask)
    {
        int layerNumber = 0;
        int layer = mask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber-1;
    }

    private void SetGunParams(bool picked)
    {
        if(picked)
        {
            isPickedUp = true;
            bulletEmitterScript.enabled = true;
            gunMovementScript.enabled = true;
            GetComponent<SpriteRenderer>().sortingOrder = pickedGunViewLayer;
            //set layer to picked gun
            int layerNumber = GetLayerNumberFromMask(pickedGunLayer);
            gameObject.layer = layerNumber;
            //Also the children
            foreach (Transform child in transform)
            {
                child.gameObject.layer = layerNumber;
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
        }
        else
        {
            isPickedUp = false;
            bulletEmitterScript.enabled = false;
            gunMovementScript.enabled = false;
            GetComponent<SpriteRenderer>().sortingOrder = itemViewLayer;
            //set layer to item
            int layerNumber = GetLayerNumberFromMask(itemLayer);
            gameObject.layer = layerNumber;
            //Also the children
            foreach (Transform child in transform)
            {
                child.gameObject.layer = layerNumber;
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //Get the player
        player = GameObject.FindGameObjectWithTag("Character");
        //Get the player input
        playerInput = player.GetComponent<PlayerInput>();
        //Get the pick action
        pickAction = playerInput.actions["Interact"];
        //Get the drop action
        dropAction = playerInput.actions["DropGun"];

        //Get the gun inventory
        gunInventory = player.GetComponent<GunInventory>();

        //Get the gun collider
        gunCollider = GetComponent<Collider2D>();

        //Get the bullet emitter. It is the child with tag buelletEmitter
        foreach (Transform child in transform)
        {
            if (child.CompareTag("BulletEmitter"))
            {
                bulletEmitter = child.gameObject;
                break;
            }
        }
        bulletEmitterScript = bulletEmitter.GetComponent<AdvancedBulletEmmiter>();
        gunMovementScript = GetComponent<GunMovement>();

        //Check if it has a parent
        if (transform.parent != null)
        {
            //Check if the parent is the player
            GameObject parent = transform.parent.gameObject;
            if (parent.CompareTag("Character") || parent.CompareTag("Enemy"))
            {
                SetGunParams(true);
            }
            else
            {
                SetGunParams(false);
            }
        }
        else
        {
            SetGunParams(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Check if it needs to be picked up
        if (!isPickedUp)
        {
            //Check if the pick action is triggered
            if (pickAction.triggered)
            {
                CheckPickUp();
            }
        }
        else
        {
            //Check if the drop action is triggered
            if (dropAction.triggered)
            {
                Drop();
            }
        }
        
    }

    private void CheckPickUp()
    {
        //Draw a ray from the player to where it is looking and see if it is looking to the gun collider
        Vector3 playerPosition = player.transform.position;
        //The player direction is the direction of the player relative to the mouse position
        Vector3 playerDirection = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - playerPosition;

        //Only focus in the item layer
        RaycastHit2D hit = Physics2D.Raycast(playerPosition, playerDirection, maxDistance, itemLayer);

        if (hit.collider != null)
        {
            if (hit.collider == gunCollider)
            {
                PickUp();
            }
        }
    }

    //Method to pick up the gun
    private void PickUp()
    {
        transform.SetParent(player.transform);
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.Euler(localRotation);
        transform.localScale = localScale;

        SetGunParams(true);

        gunInventory.AddGunToInventory(gameObject);
    }

    private void Drop()
    {
        transform.SetParent(null);
        SetGunParams(false);

        //Set a small velocity to the gun
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(transform.forward * dropForce, ForceMode2D.Impulse);

        gunInventory.DeleteGunFromInventory(gameObject);
    }

}
