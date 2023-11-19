using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    [Header("Menu settings")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject loadingScreen;

    [Header("Loading settings")]
    [SerializeField] private string sceneName;
    [SerializeField] private Slider slider;

    public void LoadSceneByName()
    {
        StartCoroutine(LoadAsynchronously());
    }

    IEnumerator LoadAsynchronously()
    {
        menu.SetActive(false);
        loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            slider.value = progress;

            yield return null;
        }
    }
}
