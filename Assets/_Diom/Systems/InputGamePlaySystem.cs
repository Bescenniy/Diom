
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputGamePlaySystem
{
    public static InputGamePlaySystem Instance;
    public static void Initialize()
    {
        Instance = new InputGamePlaySystem();
        Debug.Log(InputGamePlaySystem.Instance);
    }

    public static void Shutdown()
    {
        Instance = null;
        Debug.Log(InputGamePlaySystem.Instance);
    }
  

    private void Update()
    {
        if (Keyboard.current.wKey.isPressed)
        {
            Debug.Log("w Pressed");
        }
    }
}
