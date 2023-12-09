using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EnterMenu : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject gameUI;
    private Text menuText;
    private PlayerInput playerInput;
    private InputAction menuAction;
    public static EnterMenu instance;
    private bool canToggle = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        playerInput = GetComponent<PlayerInput>();
        menuAction = playerInput.actions.FindAction("Menu");
        menuAction.Enable();
        menu.SetActive(false);
        menuText = menu.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        if (menuAction.triggered && canToggle)
        {
            ToggleMenu("Game Paused");
        }
    }

    public void ToggleMenu(string text, bool canExit = true)
    {
        canToggle = canExit;
        menuText.text = text;
        gameUI.SetActive(!gameUI.activeSelf);
        menu.SetActive(!menu.activeSelf);
        //pause the game
        Time.timeScale = menu.activeSelf ? 0 : 1;
        //Play menu sound or game sound
        if (menu.activeSelf)
        {
            GameAudioManager.instance.PlayMenuSong();
        }
        else
        {
            GameAudioManager.instance.ResumeGameSong();
        }
    }
}
