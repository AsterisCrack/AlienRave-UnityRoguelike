using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunMovement : MonoBehaviour
{
    [SerializeField] private int bottomLayer = 3;
    [SerializeField] private int topLayer = 5;
    Vector3 mousePosition;
    Vector3 playerPosition;
    Vector3 startingPosition;
    private PlayerInput playerInput;
    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        startingPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate the gun to face the mouse
        float angle = GetAngle();
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        //If the angle is between -90 and 90, then the gun is facing right
        gameObject.GetComponent<SpriteRenderer>().flipY = !(angle > -90 && angle < 90);
        transform.localPosition = (angle > -90 && angle < 90) ? startingPosition : new Vector3(-startingPosition.x, startingPosition.y, startingPosition.z);

        //Move the gun to the correct layer if the gun is behind or on top of the player
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = angle > 0 && angle < 180 ? bottomLayer : topLayer;
    }

    private float GetAngle()
    {
        //Rotate the gun to face the mouse
        mousePosition = Camera.main.ScreenToWorldPoint(playerInput.actions["Aim"].ReadValue<Vector2>());
        playerPosition = transform.parent.position; //Due to changing the x position of the gun when left or right, the parent position is used so that no jittering occurs
        Vector3 direction = mousePosition - playerPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;
    }
}
