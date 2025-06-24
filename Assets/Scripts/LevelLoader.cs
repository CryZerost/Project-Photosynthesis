using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider loadingSlider;
    [SerializeField] private float minimumLoadTime = 5.0f; // Minimum load time in seconds

    private float lastLoggedProgress = 0f; // Store the last logged progress

    public void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            LoadLevel(0); // this one i use to skip the cutscene when development
        }*/

        
    }

    public void QuitQ()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        // Unpause the game before starting the load
        Time.timeScale = 1f;

        // Start loading the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false; // Prevent the scene from activating immediately

        loadingScreen.SetActive(true);

        float startTime = Time.time;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Only log progress if the difference is greater than 1% to prevent spam
            if (Mathf.Abs(progress - lastLoggedProgress) >= 0.01f)
            {
                Debug.Log("Loading progress: " + progress);
                lastLoggedProgress = progress;
            }

            // Update your customie Loading UI here.

            // Calculate the elapsed time
            float elapsedTime = Time.time - startTime;
            float remainingTime = Mathf.Max(minimumLoadTime - elapsedTime, 7.0f);
            loadingSlider.value = elapsedTime / minimumLoadTime;

            // Wait for the minimum load time or until the scene is ready
            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadTime)
            {
                Debug.Log("Scene loading complete, activating scene...");
                operation.allowSceneActivation = true;
            }

            yield return new WaitForSeconds(Mathf.Min(remainingTime, 0.1f));
        }

        Debug.Log("Scene loaded successfully");
        loadingScreen.SetActive(false); // Hide the loading screen after the scene has been activated
    }
}