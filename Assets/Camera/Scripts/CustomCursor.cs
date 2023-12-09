using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private GameObject mCursorVisual;
    private Vector3 mousePosition;
    //Inputs
    private PlayerInput playerInput;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    void Start()
    {
        //Instantiate the cursor visual
        mCursorVisual = Instantiate(mCursorVisual, transform.position, Quaternion.identity);
        Cursor.visible = false;
    }

    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(playerInput.actions["Aim"].ReadValue<Vector2>());
        mCursorVisual.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);
    }
}
