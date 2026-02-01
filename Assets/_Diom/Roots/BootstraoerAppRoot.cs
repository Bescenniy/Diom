using System;
using UnityEngine;

public static class AppRootBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureAppRoot()
    {
        if (AppRoot.Instance != null)
        { 
            Debug.LogException(new Exception("AppRoot is already loaded"));
            return;
            
        }
        if (AppRoot.Instance == null)
        {
            var go = new GameObject("[AppRoot]");
            go.AddComponent<AppRoot>();
        }
       
    }
}