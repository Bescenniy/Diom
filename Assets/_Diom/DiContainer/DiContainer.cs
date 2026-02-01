using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Простой DI контейнер.
/// Хранит "регистрации" сервисов и умеет создавать объекты,
/// автоматически собирая их зависимости через конструктор.
/// </summary>
public sealed class DiContainer
{
    // Внутренняя запись о том, как создавать сервис.
    private sealed class Registration
    {
        public Func<DiContainer, object> Factory; // как создать объект
        public bool IsSingleton;                  // singleton или transient
        public object CachedInstance;             // если singleton — хранится созданный экземпляр
    }

    // Карта: тип сервиса -> регистрация
    private readonly Dictionary<Type, Registration> _map = new Dictionary<Type, Registration>();

    // -------------------------------------------------------
    // REGISTRATION (как контейнер узнаёт "что создавать")
    // -------------------------------------------------------

    /// <summary>
    /// Регистрируем уже готовый объект как singleton.
    /// Например: EventBus, Config, и т.п.
    /// </summary>
    public void RegisterInstance<TService>(TService instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        _map[typeof(TService)] = new Registration
        {
            IsSingleton = true,
            CachedInstance = instance,
            Factory = _ => instance
        };
    }

    /// <summary>
    /// Singleton: контейнер создаст объект один раз и будет возвращать его же всегда.
    /// </summary>
    public void RegisterSingleton<TService, TImpl>() where TImpl : TService
    {
        _map[typeof(TService)] = new Registration
        {
            IsSingleton = true,
            Factory = c => c.CreateByConstructor(typeof(TImpl))
        };
    }

    /// <summary>
    /// Transient: контейнер создаёт новый объект при каждом Resolve.
    /// </summary>
    public void RegisterTransient<TService, TImpl>() where TImpl : TService
    {
        _map[typeof(TService)] = new Registration
        {
            IsSingleton = false,
            Factory = c => c.CreateByConstructor(typeof(TImpl))
        };
    }

    /// <summary>
    /// Частый случай: регистрируем сам тип как singleton (без интерфейса).
    /// </summary>
    public void RegisterSingleton<TService>() where TService : class
    {
        _map[typeof(TService)] = new Registration
        {
            IsSingleton = true,
            Factory = c => c.CreateByConstructor(typeof(TService))
        };
    }

    /// <summary>
    /// Частый случай: регистрируем сам тип как transient (без интерфейса).
    /// </summary>
    public void RegisterTransient<TService>() where TService : class
    {
        _map[typeof(TService)] = new Registration
        {
            IsSingleton = false,
            Factory = c => c.CreateByConstructor(typeof(TService))
        };
    }

    // -------------------------------------------------------
    // RESOLVE (получение и создание объектов)
    // -------------------------------------------------------

    public TService Resolve<TService>()
    {
        return (TService)Resolve(typeof(TService));
    }

    public object Resolve(Type serviceType)
    {
        if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

        // 1) Ищем регистрацию
        if (_map.TryGetValue(serviceType, out var reg))
        {
            // 2) Если singleton уже создавался — вернём кеш
            if (reg.IsSingleton && reg.CachedInstance != null)
                return reg.CachedInstance;

            // 3) Иначе создаём через factory
            var created = reg.Factory(this);

            if (created == null)
                throw new InvalidOperationException($"Factory returned null for type {serviceType.Name}");

            // 4) Если singleton — кешируем
            if (reg.IsSingleton)
                reg.CachedInstance = created;

            return created;
        }

        // 5) Если не зарегистрировано:
        // - Если это конкретный класс (не интерфейс и не abstract) можно попробовать создать автоматически.
        //   Это удобно, но ты можешь отключить это правило для "строгости".
        if (!serviceType.IsInterface && !serviceType.IsAbstract)
        {
            return CreateByConstructor(serviceType);
        }

        throw new InvalidOperationException($"Type {serviceType.Name} is not registered in DiContainer");
    }

    // -------------------------------------------------------
    // CORE: создание объекта через конструктор
    // -------------------------------------------------------

    /// <summary>
    /// Создаёт объект, выбирая "самый жирный" конструктор (с максимумом параметров),
    /// и для каждого параметра делает Resolve(...).
    /// </summary>
    private object CreateByConstructor(Type implementationType)
    {
        // Берём публичные конструкторы
        var ctors = implementationType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .ToArray();

        if (ctors.Length == 0)
            throw new InvalidOperationException($"Type {implementationType.Name} has no public constructors");

        // Берём самый "длинный" конструктор:
        // Если он не сможет быть собран (что-то не зарегистрировано), будет понятная ошибка.
        var ctor = ctors[0];
        var parameters = ctor.GetParameters();

        // Для каждого параметра конструктора — резолвим зависимость
        var args = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var dependencyType = parameters[i].ParameterType;
            args[i] = Resolve(dependencyType);
        }

        // Создаём экземпляр
        return ctor.Invoke(args);
    }
}