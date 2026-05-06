using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    /// Loads a scene by its name. 
    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("SceneSwitcher: Scene name is empty!");
        }
    }

    /// Loads a scene by its index in the Build Settings.
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void ReloadSceneAndConnect()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        Debug.Log("Restarting scene to attempt ML-Agents handshake...");

        SceneManager.LoadScene(currentSceneName);
    }
}