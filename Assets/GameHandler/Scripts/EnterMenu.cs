using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnterMenu : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject gameUI;
    private PlayerInput playerInput;
    private InputAction menuAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        menuAction = playerInput.actions.FindAction("Menu");
        menuAction.Enable();
        menu.SetActive(false);
    }

    private void Update()
    {
        if (menuAction.triggered)
        {
            gameUI.SetActive(!gameUI.activeSelf);
            menu.SetActive(!menu.activeSelf);
            //pause the game
            Time.timeScale = menu.activeSelf ? 0 : 1;
        }
    }
}
