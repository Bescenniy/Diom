using System;
using System.Collections.Generic;
using UnityEngine;


public class EventBus : ISystem
{
    private static EventBus _instance;
    private static readonly Dictionary<Type, List<Delegate> > SignalCollbacks = new Dictionary<Type, List<Delegate>>();

    public static void Subscribe<T>(Action<T> callback) where T : class
    {
        if (!SignalCollbacks.TryGetValue(typeof(T), out var list))
        {
            list = new List<Delegate>();
            SignalCollbacks[typeof(T)] = list;
        }

        list.Add(callback);
    }

    public static void Unsubscribe<T>(Action<T> callback) where T : class
    {
        if (SignalCollbacks.TryGetValue(typeof(T), out var list))
        {
            list.Remove(callback);

            if (list.Count == 0)
                SignalCollbacks.Remove(typeof(T));
        }
    }

    public static void Invoke<T>(T signal) where T : class
    {
        if (SignalCollbacks.TryGetValue(typeof(T), out var list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is Action<T> action)
                    action(signal);
            }
        }
    }

    public void Initialize()
    {
        Debug.Log("EventBus initialized");
    }

    public void Shutdown()
    {
        
    }
}

//public class DamageSignal { public int Amount; }

//EventBus.Initialize();

//EventBus.Subscribe<DamageSignal>(OnDamage);

//EventBus.Invoke(new DamageSignal { Amount = 10 });

//EventBus.Unsubscribe<DamageSignal>(OnDamage);

//void OnDamage(DamageSignal s) => Console.WriteLine(s.Amount);