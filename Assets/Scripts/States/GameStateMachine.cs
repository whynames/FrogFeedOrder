using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityHFSM;
using VContainer.Unity;


public enum GameStates
{
    None,
    LoadingAssets,
    LoadingMap,
    Ready,
    Playing,
    Paused,
    GameOver
}

public enum PlayingStates
{
    WaitingForInput,
    FrogAction,
    CheckWinCondition
}

public enum Events
{
    OnFrogClick,
    OnPause,
    OnResume,
    OnRestart,
    OnLevelComplete,
    OnGameOver
}
public class GameStateMachine : IInitializable, ITickable
{
    private StateMachine<GameStates, Events> fsm;
    private readonly SpriteProvider spriteProvider;
    private readonly MapLoader mapLoader;
    private readonly LevelManager levelManager;
    private readonly InputHandler inputHandler;
    private readonly FrogGameSettings frogGameSettings;

    public GameStateMachine(SpriteProvider spriteProvider, MapLoader mapLoader, LevelManager levelManager, InputHandler inputHandler, FrogGameSettings frogGameSettings)
    {
        this.spriteProvider = spriteProvider;
        this.mapLoader = mapLoader;
        this.levelManager = levelManager;
        this.inputHandler = inputHandler;
        this.frogGameSettings = frogGameSettings;
    }

    public void Initialize()
    {
        fsm = new StateMachine<GameStates, Events>();

        // Main States
        fsm.AddState(GameStates.LoadingAssets, new State<GameStates, Events>(
            onEnter: async state => await LoadAssets(),
            needsExitTime: false
        ));

        fsm.AddState(GameStates.LoadingMap, new State<GameStates, Events>(
            onEnter: async state => await LoadMap(),
            needsExitTime: false



        ));

        var playingFsm = new StateMachine<GameStates, PlayingStates, Events>();
        fsm.AddState(GameStates.Playing, playingFsm);

        // Playing Sub-States
        playingFsm.AddState(PlayingStates.WaitingForInput, new State<PlayingStates, Events>(
            onEnter: state =>
            {
            },
            onLogic: state =>
            {
                inputHandler.UpdateInput();
            },
            needsExitTime: false
        ));
        playingFsm.AddState(PlayingStates.FrogAction, new State<PlayingStates, Events>(
            needsExitTime: false
        ));
        playingFsm.AddState(PlayingStates.CheckWinCondition);

        // Main Transitions
        fsm.AddTransition(GameStates.LoadingAssets, GameStates.LoadingMap,
            t => IsAssetsLoaded());

        fsm.AddTransition(GameStates.LoadingMap, GameStates.Playing,
            t => IsMapLoaded());

        fsm.AddTriggerTransition(Events.OnPause,
            GameStates.Playing, GameStates.Paused);

        fsm.AddTriggerTransition(Events.OnResume,
            GameStates.Paused, GameStates.Playing);

        // Playing State Transitions
        playingFsm.AddTriggerTransition(Events.OnFrogClick,
            PlayingStates.WaitingForInput, PlayingStates.FrogAction);

        playingFsm.AddTransition(PlayingStates.FrogAction,
            PlayingStates.CheckWinCondition,
            t => IsFrogActionComplete());

        playingFsm.AddTransition(PlayingStates.CheckWinCondition,
            PlayingStates.WaitingForInput,
            t => !IsLevelComplete());

        playingFsm.SetStartState(PlayingStates.WaitingForInput);
        fsm.SetStartState(GameStates.LoadingAssets);
        fsm.Init();
    }

    private async UniTask LoadAssets()
    {
        Debug.Log("Loading Assets");
        await spriteProvider.LoadSprites();
        fsm.StateCanExit();
    }

    private async UniTask LoadMap()
    {
        Debug.Log("Loading Map");
        await levelManager.LoadLevel(frogGameSettings.MapName);
        fsm.StateCanExit();
    }

    private bool IsAssetsLoaded() => spriteProvider.IsLoaded;

    private bool IsMapLoaded() => mapLoader.IsMapLoaded;

    private bool IsFrogActionComplete() => !levelManager.CurrentFrog?.IsShootingTongue ?? true;

    private bool IsLevelComplete() => levelManager.IsCurrentLevelComplete;

    public void Tick()
    {
        fsm.OnLogic();
    }


}