using System;

public static class GlobalSystemCatalog
{
    public static readonly Action<DiContainer>[] Register =
    {
        c => c.RegisterSingleton<SceneSystem>(),
        c => c.RegisterSingleton<EventBus>()
        
    };

    public static readonly Func<DiContainer, ISystem>[] Resolve =
    {
        c => c.Resolve<SceneSystem>(),
        c => c.Resolve<EventBus>()
        
    };



}
