using UnityEngine;

public class MainMenuRoot : MonoBehaviour
{
    private void Awake()
    {
        if (AppRoot.Instance == null)
        {
            GetComponent<AppRoot>(); return;
        }
        AppRoot.Instance.GetSystem<SceneSystem>().LoadScene("GamePlayScene");
    }
}

