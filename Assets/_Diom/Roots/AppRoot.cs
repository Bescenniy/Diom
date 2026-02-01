using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppRoot : MonoBehaviour
{
    public static AppRoot Instance { get; private set; }

    private DiContainer _diContainer;

    // Быстрый доступ к системам по типу
    private readonly Dictionary<Type, ISystem> _systemsByType = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _diContainer = new DiContainer();

        RegisterAllSystems(_diContainer);
        ResolveAllSystems(_diContainer);

       

        if (SceneManager.GetActiveScene().name == "BootstrapScene")
        {
            GetSystem<SceneSystem>().LoadScene("MainMenuScene");
        }
        DontDestroyOnLoad(gameObject);
    }

    public T GetSystem<T>() where T : class, ISystem
    {
        if (_systemsByType.TryGetValue(typeof(T), out var sys))
            return (T)sys;

        throw new InvalidOperationException($"System {typeof(T).Name} not resolved. " +
                                            $"Check GlobalSystemCatalog.Resolve registration.");
    }

    private void RegisterAllSystems(DiContainer c)
    {
        foreach (var r in GlobalSystemCatalog.Register)
            r(c);

        Debug.Log("All Systems Registered");
    }

    private void ResolveAllSystems(DiContainer c)
    {
        foreach (var resolver in GlobalSystemCatalog.Resolve)
        {
            var sys = resolver(c);
            sys.Initialize();

            // кладем по точному типу
            _systemsByType[sys.GetType()] = sys;

            // если хочешь получать по интерфейсу/базовому типу — можно расширить отдельно
        }

        Debug.Log("All Systems Resolved");
    }

    private void OnDestroy()
    {
        // корректное выключение систем (если нужно)
        foreach (var sys in _systemsByType.Values)
            sys.Shutdown();

        _systemsByType.Clear();

        if (Instance == this) Instance = null;
    }
}