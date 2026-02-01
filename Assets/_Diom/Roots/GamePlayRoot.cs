using System;
using UnityEngine;
public class GamePlayRoot : MonoBehaviour
{
    private void Awake()
    {
        InputGamePlaySystem.Initialize();
    }

    private void OnDestroy()
    {
        InputGamePlaySystem.Shutdown();
    }
}


