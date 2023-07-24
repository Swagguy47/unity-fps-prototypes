using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentScene : MonoBehaviour
{
    [SerializeField] LoadingManager LoadingScreen;

    private void Awake()
    {
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    List<AsyncOperation> ScenesLoading = new List<AsyncOperation>();
    public void LoadGame()
    {
        //SceneManager.LoadScene("Loading"); //covers screen with loading panel
        LoadingScreen.gameObject.SetActive(true);
        LoadingScreen.SetLoadDescriptor("Loading Game...");
        ScenesLoading.Add(SceneManager.UnloadSceneAsync(1)); //deloads main menu
        ScenesLoading.Add(SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive)); //streams in assets from GameWorld

        StartCoroutine(GetSceneLoadProgress());
    }

    public IEnumerator GetSceneLoadProgress()
    {
        for (int i = 0; i < ScenesLoading.Count; i++)
        {
            while (!ScenesLoading[i].isDone)
            {
                yield return null;
            }
        }
        //on finish loading
        LoadingScreen.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
