using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSystem : ISystem
{
    public string CurrentSceneName { get; private set; }
    public string LoadingSceneName { get; private set; }

    public void LoadScene(string sceneName)
    {
        LoadingSceneName = sceneName;
        SceneManager.LoadScene(sceneName);
    }

    public void Initialize()
    {
        Debug.Log("Initializing SceneSystem");
        CurrentSceneName = SceneManager.GetActiveScene().name;
    }

    public void Shutdown() { }
}