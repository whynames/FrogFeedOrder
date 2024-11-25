using VContainer;
using VContainer.Unity;
using UnityEngine;

[System.Serializable]
public class FrogGameSettings
{
    [field: SerializeField]
    public string MapName { get; set; }

    public FrogGameSettings(string mapName)
    {
        MapName = mapName;
    }
}

public class FrogGame : LifetimeScope
{
    [SerializeField] private PrefabFactory prefabFactory;

    [SerializeField] private AudioManager audioManager;

    [SerializeField] private UIManager uiManager;
    [SerializeField] private FrogGameSettings frogSettings;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<PrefabFactory>(prefabFactory);
        builder.Register<SpriteProvider>(Lifetime.Singleton);
        builder.RegisterEntryPoint<GameStateMachine>(Lifetime.Singleton);
        builder.Register<MapCreator>(Lifetime.Singleton);
        builder.Register<GameManager>(Lifetime.Singleton);
        builder.Register<LevelManager>(Lifetime.Singleton);
        builder.Register<MapLoader>(Lifetime.Singleton);
        builder.Register<InputHandler>(Lifetime.Singleton);
        builder.Register<MoveManager>(Lifetime.Singleton);
        builder.RegisterComponent(uiManager);
        builder.RegisterComponent(audioManager);
        builder.RegisterInstance(frogSettings);
        builder.RegisterBuildCallback(async container =>
        {
            var inputHandler = container.Resolve<InputHandler>();
            var moveManager = container.Resolve<MoveManager>();
            var uiManager = container.Resolve<UIManager>();
            uiManager.Initialize();
            moveManager.Initialize(10);
            inputHandler.Initialize();
        });
    }
}