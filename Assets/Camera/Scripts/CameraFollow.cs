using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float playerWeight = 0.7f; // Weight for the player's position.
    [SerializeField] private float followSpeed = 5f;

    //Inputs
    private PlayerInput playerInput;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (player == null)
        {
            return; // Make sure the player is assigned.
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(playerInput.actions["Aim"].ReadValue<Vector2>());

        // Calculate the weighted average of the player's and mouse positions.
        Vector3 targetPosition = (player.position * playerWeight + mousePosition * (1 - playerWeight));
        targetPosition.z = transform.position.z; // Keep the camera's z-position unchanged.

        // Smoothly move the camera towards the target position.
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
